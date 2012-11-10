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

	public class InterfaceProxyWithOptionalTargetContributor : CompositeTypeContributor
	{
		private readonly GetTargetExpressionDelegate getTargetExpression;
		private readonly GetTargetReferenceDelegate getTargetReference;

		public InterfaceProxyWithOptionalTargetContributor(INamingScope namingScope, GetTargetExpressionDelegate getTargetExpression, GetTargetReferenceDelegate getTargetReference)
			: base(namingScope)
		{
			this.getTargetExpression = getTargetExpression;
			this.getTargetReference = getTargetReference;
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

		protected override MethodGenerator GetMethodGenerator(MetaMethod method, ClassEmitter @class, OverrideMethodDelegate overrideMethod)
		{
			if (!method.Proxyable)
			{
				return new OptionallyForwardingMethodGenerator(method, overrideMethod, getTargetReference);
			}

			return new MethodWithInvocationGenerator(method,
			                                         @class.GetField("__interceptors"),
			                                         () => GetInvocationType(method, @class),
			                                         getTargetExpression,
			                                         overrideMethod,
			                                         null);
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