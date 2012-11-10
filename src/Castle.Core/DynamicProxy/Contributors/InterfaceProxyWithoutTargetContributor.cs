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

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

	public class InterfaceProxyWithoutTargetContributor : CompositeTypeContributor
	{
		public InterfaceProxyWithoutTargetContributor(INamingScope namingScope)
			: base(namingScope)
		{
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
				return new MinimialisticMethodGenerator(method, createMethod);
			}

			return new MethodWithInvocationGenerator(method,
			                                         () => GetInvocationType(method, @class),
			                                         NullExpression.Instance,
			                                         createMethod,
			                                         null);
		}

		private Type GetInvocationType(MetaMethod method, ClassEmitter proxy)
		{
			return new CompositionInvocationTypeGenerator(method, proxy, namingScope)
			{
				Logger = Logger
			}.GetProxyType();
		}
	}
}