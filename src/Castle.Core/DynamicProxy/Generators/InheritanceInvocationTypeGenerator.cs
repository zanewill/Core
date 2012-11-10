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

	using Castle.Core.Logging;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;
	using Castle.DynamicProxy.Tokens;

	public class InheritanceInvocationTypeGenerator : InvocationTypeGenerator, IProxyTypeGenerator
	{
		public static readonly Type BaseType = typeof(InheritanceInvocation);
		private readonly ClassEmitter @class;
		private readonly ProxyGenerationOptions options;
		private readonly INamingScope namingScope;

		public InheritanceInvocationTypeGenerator(MetaMethod method, MethodInfo callback, IInvocationCreationContributor contributor, ClassEmitter @class, ProxyGenerationOptions options, INamingScope namingScope)
			: base(method, callback, contributor)
		{
			this.@class = @class;
			this.options = options;
			this.namingScope = namingScope;
		}

		public ILogger Logger { get; set; }

		public Type GetProxyType()
		{
			Logger.DebugFormat("No cache for invocation for target method {0}.", method.Method);
			var type = Generate(@class, options, namingScope).BuildType();
			return type;
		}

		protected override ArgumentReference[] GetBaseCtorArguments(out ConstructorInfo baseConstructor)
		{
			baseConstructor = InvocationMethods.InheritanceInvocationConstructor;
			return new[]
			{
				new ArgumentReference(typeof(Type)),
				new ArgumentReference(typeof(object)),
				new ArgumentReference(typeof(IInterceptor[])),
				new ArgumentReference(typeof(MethodInfo)),
				new ArgumentReference(typeof(object[]))
			};
		}

		protected override Type GetBaseType()
		{
			return BaseType;
		}

		protected override FieldReference GetTargetReference()
		{
			return new FieldReference(InvocationMethods.ProxyObject);
		}
	}
}