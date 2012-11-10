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

namespace Castle.DynamicProxy.Contributors
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Tokens;

	public class ClassProxyTargetContributor : CompositeTypeContributor
	{
		private readonly IList<MethodInfo> methodsToSkip;
		private readonly Type targetType;

		public ClassProxyTargetContributor(Type targetType, IList<MethodInfo> methodsToSkip, INamingScope namingScope)
			: base(namingScope)
		{
			this.targetType = targetType;
			this.methodsToSkip = methodsToSkip;
		}

		protected override IEnumerable<MembersCollector> CollectElementsToProxyInternal(IProxyGenerationHook hook)
		{
			Debug.Assert(hook != null, "hook != null");

			var targetItem = new ClassMembersCollector(targetType) { Logger = Logger };
			targetItem.CollectMembersToProxy(hook);
			yield return targetItem;

			foreach (var @interface in interfaces)
			{
				var item = new InterfaceMembersOnClassCollector(@interface,
				                                                true,
				                                                targetType.GetInterfaceMap(@interface)) { Logger = Logger };
				item.CollectMembersToProxy(hook);
				yield return item;
			}
		}

		protected override MethodGenerator GetMethodGenerator(MetaMethod method, ClassEmitter @class, CreateMethodDelegate createMethod)
		{
			if (methodsToSkip.Contains(method.Method))
			{
				return null;
			}

			if (!method.Proxyable)
			{
				return new MinimialisticMethodGenerator(method, createMethod);
			}

			if (ExplicitlyImplementedInterfaceMethod(method))
			{
#if SILVERLIGHT
				return null;
#else
				return ExplicitlyImplementedInterfaceMethodGenerator(method, @class, createMethod);
#endif
			}

			var type = targetType.IsGenericTypeDefinition ? targetType.MakeGenericType(@class.GenericTypeParams) : targetType;
			return new MethodWithInvocationGenerator(method,
			                                         @class.GetField("__interceptors"),
			                                         () => BuildInvocationType(method, @class, GetCallback(method, @class, method.Method), null),
			                                         new TypeTokenExpression(type),
			                                         createMethod,
			                                         null);
		}

		private Type BuildInvocationType(MetaMethod method, ClassEmitter @class, MethodInfo callback, IInvocationCreationContributor contributor)
		{
			return new InheritanceInvocationTypeGenerator(method, callback, @class, namingScope, contributor)
			{
				Logger = Logger
			}.GetProxyType();
		}

		private MethodBuilder CreateCallbackMethod(ClassEmitter emitter, MethodInfo methodInfo, MethodInfo methodOnTarget)
		{
			var targetMethod = methodOnTarget ?? methodInfo;
			var callBackMethod = emitter.CreateMethod(namingScope.GetUniqueName(methodInfo.Name + "_callback"), targetMethod);

			if (targetMethod.IsGenericMethod)
			{
				targetMethod = targetMethod.MakeGenericMethod(callBackMethod.GenericTypeParams);
			}

			var exps = new Expression[callBackMethod.Arguments.Length];
			for (var i = 0; i < callBackMethod.Arguments.Length; i++)
			{
				exps[i] = callBackMethod.Arguments[i].ToExpression();
			}

			// invocation on base class

			callBackMethod.CodeBuilder.AddStatement(
				new ReturnStatement(
					new MethodInvocationExpression(SelfReference.Self,
					                               targetMethod,
					                               exps)));

			return callBackMethod.MethodBuilder;
		}

		private bool ExplicitlyImplementedInterfaceMethod(MetaMethod method)
		{
			return method.MethodOnTarget.IsPrivate;
		}

		private MethodGenerator ExplicitlyImplementedInterfaceMethodGenerator(MetaMethod method, ClassEmitter @class, CreateMethodDelegate createMethod)
		{
			var @delegate = GetDelegateType(method, @class);
			var contributor = GetContributor(@delegate, method);
			var type = targetType.IsGenericTypeDefinition ? targetType.MakeGenericType(@class.GenericTypeParams) : targetType;
			return new MethodWithInvocationGenerator(method,
			                                         @class.GetField("__interceptors"),
			                                         () => BuildInvocationType(method, @class, null, contributor),
			                                         new TypeTokenExpression(type),
			                                         createMethod,
			                                         contributor);
		}

		private MethodBuilder GetCallback(MetaMethod method, ClassEmitter @class, MethodInfo methodInfo)
		{
			if (method.HasTarget)
			{
				return CreateCallbackMethod(@class, methodInfo, method.MethodOnTarget);
			}
			return null;
		}

		private IInvocationCreationContributor GetContributor(Type @delegate, MetaMethod method)
		{
			if (@delegate.IsGenericType == false)
			{
				return new InvocationWithDelegateContributor(@delegate, targetType, method, namingScope);
			}
			return new InvocationWithGenericDelegateContributor(@delegate, method, new FieldReference(InvocationMethods.ProxyObject));
		}

		private Type GetDelegateType(MetaMethod method, ClassEmitter @class)
		{
			return new DelegateTypeGenerator(method, targetType, namingScope, @class.ModuleScope)
			{
				Logger = Logger
			}.GetProxyType();
		}
	}
}