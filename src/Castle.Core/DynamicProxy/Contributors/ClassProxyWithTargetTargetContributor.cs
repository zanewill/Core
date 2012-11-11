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

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Tokens;

	public class ClassProxyWithTargetTargetContributor : CompositeTypeContributor
	{
		private readonly IList<MethodInfo> methodsToSkip;
		private readonly Type targetType;

		public ClassProxyWithTargetTargetContributor(Type targetType, IList<MethodInfo> methodsToSkip,
		                                             INamingScope namingScope)
			: base(namingScope)
		{
			this.targetType = targetType;
			this.methodsToSkip = methodsToSkip;
		}

		protected override IEnumerable<MembersCollector> CollectElementsToProxyInternal(IProxyGenerationHook hook)
		{
			Debug.Assert(hook != null, "hook != null");

			var targetItem = new WrappedClassMembersCollector(targetType) { Logger = Logger };
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

			if (IsDirectlyAccessible(method) == false)
			{
				return IndirectlyCalledMethodGenerator(method, @class, createMethod);
			}

			return new MethodWithInvocationGenerator(method,
			                                         () => GetInvocationType(method, @class),
			                                         @class.GetField("__target").ToExpression(),
			                                         createMethod,
			                                         null);
		}

		private IInvocationCreationContributor GetContributor(Type @delegate, MetaMethod method)
		{
			if (@delegate.IsGenericType == false)
			{
				return new InvocationWithDelegateContributor(@delegate, targetType, method, namingScope);
			}
			return new InvocationWithGenericDelegateContributor(@delegate,
			                                                    method,
			                                                    new FieldReference(InvocationMethods.Target));
		}

		private Type GetDelegateType(MetaMethod method, ClassEmitter @class)
		{
			return new DelegateTypeGenerator(method, targetType, namingScope, @class.ModuleScope)
			{
				Logger = Logger
			}.GetProxyType();
		}

		private Type GetInvocationType(MetaMethod method, ClassEmitter @class)
		{
			return new CompositionInvocationTypeGenerator(method, @class, namingScope)
			{
				Logger = Logger
			}.GetProxyType();
		}

		private MethodGenerator IndirectlyCalledMethodGenerator(MetaMethod method, ClassEmitter proxy, CreateMethodDelegate createMethod)
		{
			var @delegate = GetDelegateType(method, proxy);
			var contributor = GetContributor(@delegate, method);
			return new MethodWithInvocationGenerator(method,
			                                         () => new CompositionInvocationTypeGenerator(method,
			                                                                                      proxy,
			                                                                                      namingScope, contributor)
			                                         {
				                                         Logger = Logger
			                                         }.GetProxyType(),
			                                         proxy.GetField("__target").ToExpression(),
			                                         createMethod,
			                                         contributor);
		}

		private bool IsDirectlyAccessible(MetaMethod method)
		{
			return method.MethodOnTarget.IsPublic || method.Method.DeclaringType.IsInterface;
		}
	}
}