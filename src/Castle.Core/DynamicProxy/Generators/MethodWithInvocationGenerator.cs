// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.DynamicProxy.Generators
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
#if !SILVERLIGHT
	using System.Xml.Serialization;
#endif

	using Castle.Core.Internal;
	using Castle.DynamicProxy.Contributors;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;
	using Castle.DynamicProxy.Tokens;

	public class MethodWithInvocationGenerator : MethodGenerator
	{
		private readonly IInvocationCreationContributor contributor;
		private readonly GetTargetExpressionDelegate getTargetExpression;
		private readonly Reference interceptors;
		private readonly Func<Type> invocationFactory;

		public MethodWithInvocationGenerator(MetaMethod method, Reference interceptors, Func<Type> invocationFactory,
		                                     GetTargetExpressionDelegate getTargetExpression,
		                                     OverrideMethodDelegate createMethod, IInvocationCreationContributor contributor)
			: base(method, createMethod)
		{
			this.invocationFactory = invocationFactory;
			this.getTargetExpression = getTargetExpression;
			this.interceptors = interceptors;
			this.contributor = contributor;
		}

		protected FieldReference BuildMethodInterceptorsField(ClassEmitter @class, MethodInfo method, INamingScope namingScope)
		{
			var methodInterceptors = @class.CreateField(
				namingScope.GetUniqueName(string.Format("interceptors_{0}", method.Name)),
				typeof(IInterceptor[]),
				false);
#if !SILVERLIGHT
			@class.DefineCustomAttributeFor<XmlIgnoreAttribute>(methodInterceptors);
#endif
			return methodInterceptors;
		}

		protected override MethodEmitter BuildProxiedMethodBody(MethodEmitter emitter, ClassEmitter proxy, INamingScope namingScope)
		{
			var invocationType = invocationFactory();

			var genericArguments = Type.EmptyTypes;

			var constructor = invocationType.GetConstructors()[0];
			var proxiedMethodToken = proxy.CreateStaticField(namingScope.GetUniqueName("token_" + MethodToOverride.Name), typeof(MethodInfo));

			if (MethodToOverride.IsGenericMethod)
			{
				// bind generic method arguments to invocation's type arguments
				genericArguments = emitter.MethodBuilder.GetGenericArguments();
				var invocationTypeGenericArguments = CollectionExtensions.ConcatAll(proxy.TypeBuilder.GetGenericArguments(), genericArguments);
				invocationType = invocationType.MakeGenericType(invocationTypeGenericArguments);
				constructor = TypeBuilder.GetConstructor(invocationType, constructor);

				if (proxy.IsGenericType)
				{
					// NOTE: This works around VerificationException that's thrown if we try to load directly via ldtoken a method with some generic arguments that have constraints on type arguments
					proxy.ClassConstructor.CodeBuilder.AddStatement(new AssignStatement(proxiedMethodToken, new MethodInvocationExpression(
					                                                                                         	(Expression) null,
					                                                                                         	GenericsHelper.GetAdjustedOpenMethodToken,
					                                                                                         	new TypeTokenExpression(proxy.AdjustMethod(MethodToOverride).DeclaringType),
					                                                                                         	new ConstReference(MethodToOverride.MetadataToken).ToExpression())));
				}
				else
				{
					var methodForToken = proxy.AdjustMethod(MethodToOverride);
					proxy.ClassConstructor.CodeBuilder.AddStatement(new AssignStatement(proxiedMethodToken,
					                                                                    new MethodTokenExpression(
						                                                                    methodForToken.GetGenericMethodDefinition())));
				}
			}
			else
			{
				var methodForToken = proxy.AdjustMethod(MethodToOverride);
				proxy.ClassConstructor.CodeBuilder.AddStatement(new AssignStatement(proxiedMethodToken, new MethodTokenExpression(methodForToken)));
			}

			var methodInterceptors = SetMethodInterceptors(proxy, namingScope, emitter, proxiedMethodToken.ToExpression());

			var dereferencedArguments = IndirectReference.WrapIfByRef(emitter.Arguments);
			var hasByRefArguments = HasByRefArguments(emitter.Arguments);

			var arguments = GetCtorArguments(proxy, proxiedMethodToken.ToExpression(), dereferencedArguments, methodInterceptors);
			var ctorArguments = ModifyArguments(proxy, arguments);

			var invocationLocal = emitter.CodeBuilder.DeclareLocal(invocationType);
			emitter.CodeBuilder.AddStatement(new AssignStatement(invocationLocal, new NewInstanceExpression(constructor, ctorArguments)));

			if (MethodToOverride.IsGenericMethod)
			{
				EmitLoadGenricMethodArguments(emitter, MethodToOverride.MakeGenericMethod(genericArguments), invocationLocal);
			}

			if (hasByRefArguments)
			{
				emitter.CodeBuilder.AddStatement(new TryStatement());
			}

			var proceed = new ExpressionStatement(new MethodInvocationExpression(invocationLocal, InvocationMethods.Proceed));
			emitter.CodeBuilder.AddStatement(proceed);

			if (hasByRefArguments)
			{
				emitter.CodeBuilder.AddStatement(new FinallyStatement());
			}

			GeneratorUtil.CopyOutAndRefParameters(dereferencedArguments, invocationLocal, MethodToOverride, emitter);

			if (hasByRefArguments)
			{
				emitter.CodeBuilder.AddStatement(new EndExceptionBlockStatement());
			}

			if (MethodToOverride.ReturnType != typeof(void))
			{
				// Emit code to return with cast from ReturnValue
				var getRetVal = new MethodInvocationExpression(invocationLocal, InvocationMethods.GetReturnValue);
				emitter.CodeBuilder.AddStatement(new ReturnStatement(new ConvertExpression(emitter.ReturnType, getRetVal)));
			}
			else
			{
				emitter.CodeBuilder.AddStatement(new ReturnStatement());
			}

			return emitter;
		}

		private Expression SetMethodInterceptors(ClassEmitter @class, INamingScope namingScope, MethodEmitter emitter, Expression proxiedMethodTokenExpression)
		{
			var selector = @class.GetField("__selector");
			if(selector == null)
			{
				return null;
			}


			var methodInterceptorsField = BuildMethodInterceptorsField(@class, MethodToOverride, namingScope);

			var emptyInterceptors = new NewArrayExpression(0, typeof(IInterceptor));
			var selectInterceptors = new MethodInvocationExpression(selector, InterceptorSelectorMethods.SelectInterceptors,
			                                                        new MethodInvocationExpression(default(Reference),
				                                                        TypeUtilMethods.GetTypeOrNull,
				                                                        getTargetExpression(@class, MethodToOverride)),
			                                                        proxiedMethodTokenExpression, interceptors.ToExpression())
			{ VirtualCall = true };

			emitter.CodeBuilder.AddExpression(
				new IfNullExpression(methodInterceptorsField,
				                     new AssignStatement(methodInterceptorsField,
				                                         new NullCoalescingOperatorExpression(selectInterceptors, emptyInterceptors))));

			return methodInterceptorsField.ToExpression();
		}

		private void EmitLoadGenricMethodArguments(MethodEmitter methodEmitter, MethodInfo method, Reference invocationLocal)
		{
			var genericParameters = method.GetGenericArguments().FindAll(t => t.IsGenericParameter);
			var genericParamsArrayLocal = methodEmitter.CodeBuilder.DeclareLocal(typeof(Type[]));
			methodEmitter.CodeBuilder.AddStatement(
				new AssignStatement(genericParamsArrayLocal, new NewArrayExpression(genericParameters.Length, typeof(Type))));

			for (var i = 0; i < genericParameters.Length; ++i)
			{
				methodEmitter.CodeBuilder.AddStatement(
					new AssignArrayStatement(genericParamsArrayLocal, i, new TypeTokenExpression(genericParameters[i])));
			}
			methodEmitter.CodeBuilder.AddExpression(
				new MethodInvocationExpression(invocationLocal,
				                               InvocationMethods.SetGenericMethodArguments,
				                               new ReferenceExpression(
					                               genericParamsArrayLocal)));
		}

		private Expression[] GetCtorArguments(ClassEmitter @class, Expression proxiedMethodTokenExpression, TypeReference[] dereferencedArguments, Expression methodInterceptors)
		{
			return new[]
			{
				getTargetExpression(@class, MethodToOverride),
				SelfReference.Self.ToExpression(),
				methodInterceptors ?? interceptors.ToExpression(),
				proxiedMethodTokenExpression,
				new ReferencesToObjectArrayExpression(dereferencedArguments)
			};
		}

		private bool HasByRefArguments(ArgumentReference[] arguments)
		{
			for (var i = 0; i < arguments.Length; i++)
			{
				if (arguments[i].Type.IsByRef)
				{
					return true;
				}
			}

			return false;
		}

		private Expression[] ModifyArguments(ClassEmitter @class, Expression[] arguments)
		{
			if (contributor == null)
			{
				return arguments;
			}

			return contributor.GetConstructorInvocationArguments(arguments, @class);
		}
	}
}
