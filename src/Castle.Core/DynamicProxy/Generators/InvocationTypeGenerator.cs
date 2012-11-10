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
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.CodeBuilders;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;
	using Castle.DynamicProxy.Tokens;

	public abstract class InvocationTypeGenerator : IProxyTypeGenerator
	{
		protected readonly MetaMethod method;
		protected readonly ClassEmitter proxy;
		private readonly MethodInfo callback;
		private readonly IInvocationCreationContributor contributor;
		private readonly INamingScope namingScope;

		protected InvocationTypeGenerator(MetaMethod method, MethodInfo callback, ClassEmitter proxy, INamingScope namingScope, IInvocationCreationContributor contributor)
		{
			this.method = method;
			this.callback = callback;
			this.namingScope = namingScope;
			this.proxy = proxy;
			this.contributor = contributor;
		}

		protected virtual Type[] AdditionalInterfaces
		{
			get { return Type.EmptyTypes; }
		}

		/// <summary>
		///   Generates the constructor for the class that extends
		///   <see cref="AbstractInvocation" />
		/// </summary>
		/// <param name="baseConstructor"> </param>
		protected abstract ArgumentReference[] GetBaseCtorArguments(out ConstructorInfo baseConstructor);

		protected abstract Type GetBaseType();

		protected abstract FieldReference GetTargetReference();

		protected AbstractCodeBuilder EmitCallEnsureValidTarget(MethodEmitter invokeMethodOnTarget)
		{
			return invokeMethodOnTarget.CodeBuilder.AddStatement(
				new ExpressionStatement(
					new MethodInvocationExpression(SelfReference.Self, InvocationMethods.EnsureValidTarget)));
		}

		protected AbstractTypeEmitter Generate()
		{
			var methodInfo = method.Method;

			var invocation = GetEmitter(AdditionalInterfaces, methodInfo, proxy.ModuleScope);

			if (proxy.TypeBuilder.IsGenericTypeDefinition)
			{
				invocation.CopyGenericParametersFromType(proxy.GenericParametersSource, methodInfo);
			}
			else
			{
				invocation.CopyGenericParametersFromMethod(methodInfo);
			}

			CreateConstructor(invocation);

			ImplementInvokeMethodOnTarget(invocation, methodInfo.GetParameters());

#if !SILVERLIGHT
			invocation.DefineCustomAttribute<SerializableAttribute>();
#endif

			return invocation;
		}

		protected virtual MethodInvocationExpression GetCallbackMethodInvocation(AbstractTypeEmitter invocation,
		                                                                         Expression[] args, MethodInfo callbackMethod,
		                                                                         Reference targetField,
		                                                                         MethodEmitter invokeMethodOnTarget)
		{
			if (contributor != null)
			{
				return contributor.GetCallbackMethodInvocation(invocation, args, targetField, invokeMethodOnTarget);
			}
			var methodOnTargetInvocationExpression = new MethodInvocationExpression(
				new AsTypeReference(targetField, callbackMethod.DeclaringType),
				callbackMethod,
				args) { VirtualCall = true };
			return methodOnTargetInvocationExpression;
		}

		protected virtual void ImplementInvokeMethodOnTarget(AbstractTypeEmitter invocation, ParameterInfo[] parameters,
		                                                     MethodEmitter invokeMethodOnTarget, Reference targetField)
		{
			var callbackMethod = GetCallbackMethod(invocation);
			if (callbackMethod == null)
			{
				EmitCallThrowOnNoTarget(invokeMethodOnTarget);
				return;
			}
			var args = new Expression[parameters.Length];

			// Idea: instead of grab parameters one by one
			// we should grab an array
			var byRefArguments = new Dictionary<int, LocalReference>();

			for (var i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];

				var paramType = invocation.GetClosedParameterType(param.ParameterType);
				if (paramType.IsByRef)
				{
					var localReference = invokeMethodOnTarget.CodeBuilder.DeclareLocal(paramType.GetElementType());
					invokeMethodOnTarget.CodeBuilder
						.AddStatement(
							new AssignStatement(localReference,
							                    new ConvertExpression(paramType.GetElementType(),
							                                          new MethodInvocationExpression(SelfReference.Self,
							                                                                         InvocationMethods.GetArgumentValue,
							                                                                         new LiteralIntExpression(i)))));
					var byRefReference = new ByRefReference(localReference);
					args[i] = new ReferenceExpression(byRefReference);
					byRefArguments[i] = localReference;
				}
				else
				{
					args[i] =
						new ConvertExpression(paramType,
						                      new MethodInvocationExpression(SelfReference.Self,
						                                                     InvocationMethods.GetArgumentValue,
						                                                     new LiteralIntExpression(i)));
				}
			}

			if (byRefArguments.Count > 0)
			{
				invokeMethodOnTarget.CodeBuilder.AddStatement(new TryStatement());
			}

			var methodOnTargetInvocationExpression = GetCallbackMethodInvocation(invocation, args, callbackMethod, targetField,
			                                                                     invokeMethodOnTarget);

			LocalReference returnValue = null;
			if (callbackMethod.ReturnType != typeof(void))
			{
				var returnType = invocation.GetClosedParameterType(method.Method.ReturnType);
				returnValue = invokeMethodOnTarget.CodeBuilder.DeclareLocal(returnType);
				invokeMethodOnTarget.CodeBuilder.AddStatement(new AssignStatement(returnValue, methodOnTargetInvocationExpression));
			}
			else
			{
				invokeMethodOnTarget.CodeBuilder.AddStatement(new ExpressionStatement(methodOnTargetInvocationExpression));
			}

			AssignBackByRefArguments(invokeMethodOnTarget, byRefArguments);

			if (callbackMethod.ReturnType != typeof(void))
			{
				var setRetVal =
					new MethodInvocationExpression(SelfReference.Self,
					                               InvocationMethods.SetReturnValue,
					                               new ConvertExpression(typeof(object), returnValue.Type, returnValue.ToExpression()));

				invokeMethodOnTarget.CodeBuilder.AddStatement(new ExpressionStatement(setRetVal));
			}

			invokeMethodOnTarget.CodeBuilder.AddStatement(new ReturnStatement());
		}

		private void AssignBackByRefArguments(MethodEmitter invokeMethodOnTarget,
		                                      Dictionary<int, LocalReference> byRefArguments)
		{
			if (byRefArguments.Count == 0)
			{
				return;
			}

			invokeMethodOnTarget.CodeBuilder.AddStatement(new FinallyStatement());
			foreach (var byRefArgument in byRefArguments)
			{
				var index = byRefArgument.Key;
				var localReference = byRefArgument.Value;
				invokeMethodOnTarget.CodeBuilder.AddStatement(
					new ExpressionStatement(
						new MethodInvocationExpression(
							SelfReference.Self,
							InvocationMethods.SetArgumentValue,
							new LiteralIntExpression(index),
							new ConvertExpression(
								typeof(object),
								localReference.Type,
								new ReferenceExpression(localReference)))
						));
			}
			invokeMethodOnTarget.CodeBuilder.AddStatement(new EndExceptionBlockStatement());
		}

		private void CreateConstructor(AbstractTypeEmitter invocation)
		{
			ConstructorInfo baseConstructor;
			var baseCtorArguments = GetBaseCtorArguments(out baseConstructor);

			var constructor = CreateConstructor(invocation, baseCtorArguments);
			constructor.CodeBuilder.InvokeBaseConstructor(baseConstructor, baseCtorArguments);
			constructor.CodeBuilder.AddStatement(new ReturnStatement());
		}

		private ConstructorEmitter CreateConstructor(AbstractTypeEmitter invocation, ArgumentReference[] baseCtorArguments)
		{
			if (contributor == null)
			{
				return invocation.CreateConstructor(baseCtorArguments);
			}
			return contributor.CreateConstructor(baseCtorArguments, invocation);
		}

		private void EmitCallThrowOnNoTarget(MethodEmitter invokeMethodOnTarget)
		{
			var throwOnNoTarget = new ExpressionStatement(new MethodInvocationExpression(InvocationMethods.ThrowOnNoTarget));

			invokeMethodOnTarget.CodeBuilder.AddStatement(throwOnNoTarget);
			invokeMethodOnTarget.CodeBuilder.AddStatement(new ReturnStatement());
		}

		private MethodInfo GetCallbackMethod(AbstractTypeEmitter invocation)
		{
			if (contributor != null)
			{
				return contributor.GetCallbackMethod();
			}
			var callbackMethod = callback;
			if (callbackMethod == null)
			{
				return null;
			}

			if (!callbackMethod.IsGenericMethod)
			{
				return callbackMethod;
			}

			return callbackMethod.MakeGenericMethod(invocation.GetGenericArgumentsFor(method.Method));
		}

		private AbstractTypeEmitter GetEmitter(Type[] interfaces, MethodInfo methodInfo, ModuleScope moduleScope)
		{
			var suggestedName = string.Format("Castle.Proxies.Invocations.{0}_{1}", methodInfo.DeclaringType.Name,
			                                  methodInfo.Name);
			var uniqueName = namingScope.ParentScope.GetUniqueName(suggestedName);
			return new ClassEmitter(moduleScope, uniqueName, GetBaseType(), interfaces);
		}

		private void ImplementInvokeMethodOnTarget(AbstractTypeEmitter invocation, ParameterInfo[] parameters)
		{
			var invokeMethodOnTarget = invocation.CreateMethod("InvokeMethodOnTarget", typeof(void));
			ImplementInvokeMethodOnTarget(invocation, parameters, invokeMethodOnTarget, GetTargetReference());
		}

		public abstract Type GetProxyType();
	}
}