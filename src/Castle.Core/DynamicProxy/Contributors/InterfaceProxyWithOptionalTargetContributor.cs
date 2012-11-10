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

	public class InterfaceProxyWithOptionalTargetContributor : CompositeTypeContributor
	{
		private Type proxyTargetType;

		public InterfaceProxyWithOptionalTargetContributor(INamingScope namingScope, Type proxyTargetType)
			: base(namingScope)
		{
			this.proxyTargetType = proxyTargetType;
		}

		protected override IEnumerable<MembersCollector> CollectElementsToProxyInternal(IProxyGenerationHook hook)
		{
			Debug.Assert(hook != null, "hook != null");
			foreach (var @interface in interfaces)
			{
				var item = new InterfaceMembersCollector(@interface);
				item.CollectMembersToProxy(hook);
				yield return item;
			}
		}

		protected override MethodGenerator GetMethodGenerator(MetaMethod method, ClassEmitter @class, CreateMethodDelegate createMethod)
		{
			if (!method.Proxyable)
			{
				return new OptionallyForwardingMethodGenerator(method, createMethod, GetTarget(@class, method.Method));
			}

			return new MethodWithInvocationGenerator(method,
			                                         () => GetInvocationType(method, @class),
			                                         GetTarget(@class, method.Method).ToExpression(),
			                                         createMethod,
			                                         null);
		}


		private Reference GetTarget(ClassEmitter @class, MethodInfo method)
		{
			if (method.DeclaringType.IsAssignableFrom(proxyTargetType))
			{
				return @class.GetField("target");
			}
			return new AsTypeReference(@class.GetField("__target"), method.DeclaringType);
		}

		private Type GetInvocationType(MetaMethod method, ClassEmitter proxy)
		{
			return new ChangeTargetInvocationTypeGenerator(method, proxy, namingScope)
			{
				Logger = Logger
			}.GetProxyType();
		}
	}
}