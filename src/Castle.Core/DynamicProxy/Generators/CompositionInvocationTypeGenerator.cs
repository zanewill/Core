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

	public class CompositionInvocationTypeGenerator : InvocationTypeGenerator, IProxyTypeGenerator
	{
		public static readonly Type BaseType = typeof(CompositionInvocation);
		private readonly ModuleScope moduleScope;
		private readonly ClassEmitter @class;
		private readonly ProxyGenerationOptions options;
		private readonly INamingScope namingScope;

		public CompositionInvocationTypeGenerator(Type target, MetaMethod method, MethodInfo callback,
		                                          IInvocationCreationContributor contributor, ModuleScope moduleScope, ClassEmitter @class, ProxyGenerationOptions options, INamingScope namingScope)
			: base(target, method, callback, contributor)
		{
			this.moduleScope = moduleScope;
			this.@class = @class;
			this.options = options;
			this.namingScope = namingScope;
		}

		protected override ArgumentReference[] GetBaseCtorArguments(Type targetFieldType,
		                                                            ProxyGenerationOptions proxyGenerationOptions,
		                                                            out ConstructorInfo baseConstructor)
		{
			baseConstructor = InvocationMethods.CompositionInvocationConstructor;
			return new[]
			{
				new ArgumentReference(targetFieldType),
				new ArgumentReference(typeof(object)),
				new ArgumentReference(typeof(IInterceptor[])),
				new ArgumentReference(typeof(MethodInfo)),
				new ArgumentReference(typeof(object[])),
			};
		}

		protected override Type GetBaseType()
		{
			return BaseType;
		}

		public ILogger Logger { get; set; }

		protected override FieldReference GetTargetReference()
		{
			return new FieldReference(InvocationMethods.Target);
		}

		protected override void ImplementInvokeMethodOnTarget(AbstractTypeEmitter invocation, ParameterInfo[] parameters,
		                                                      MethodEmitter invokeMethodOnTarget, Reference targetField)
		{
			invokeMethodOnTarget.CodeBuilder.AddStatement(
				new ExpressionStatement(
					new MethodInvocationExpression(SelfReference.Self, InvocationMethods.EnsureValidTarget)));
			base.ImplementInvokeMethodOnTarget(invocation, parameters, invokeMethodOnTarget, targetField);
		}

		public Type GetProxyType()
		{

			var key = new CacheKey(method.Method, BaseType, AdditionalInterfaces, null);

			var type = moduleScope.GetFromCache(key);
			if (type != null)
			{
				Logger.DebugFormat("Found cached invocation type {0} for target method {1}.", type.FullName, method.MethodOnTarget);
				return type;
			}

			// Log details about the cache miss
			Logger.DebugFormat("No cached invocation type was found for target method {0}.", method.MethodOnTarget);
			type = Generate(@class, options, namingScope).BuildType();

			moduleScope.RegisterInCache(key, type);

			return type;
		}
	}
}