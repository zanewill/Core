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
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Logging;
	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;

	public class DelegateTypeGenerator : IProxyTypeGenerator
	{
		private const TypeAttributes DelegateFlags = TypeAttributes.Class |
		                                             TypeAttributes.Public |
		                                             TypeAttributes.Sealed |
		                                             TypeAttributes.AnsiClass |
		                                             TypeAttributes.AutoClass;

		private readonly MetaMethod method;
		private readonly ModuleScope moduleScope;
		private readonly INamingScope namingScope;
		private readonly Type targetType;

		public ILogger Logger { get; set; }

		public DelegateTypeGenerator(MetaMethod method, Type targetType, INamingScope namingScope, ModuleScope moduleScope)
		{
			this.method = method;
			this.targetType = targetType;
			this.namingScope = namingScope;
			this.moduleScope = moduleScope;
		}

		public Type GetProxyType()
		{
			var key = new CacheKey(
				typeof(Delegate),
				targetType,
				new[] { method.MethodOnTarget.ReturnType }
					.Concat(ArgumentsUtil.GetTypes(method.MethodOnTarget.GetParameters())).
					ToArray(),
				null);

			var type = moduleScope.GetFromCache(key);
			if (type != null)
			{
				Logger.DebugFormat("Found cached delegate type {0} for target method {1}.", type.FullName, method.MethodOnTarget);
				return type;
			}

			// Log details about the cache miss
			Logger.DebugFormat("No cached delegate type was found for target method {0}.", method.MethodOnTarget);
			type = Generate().BuildType();

			moduleScope.RegisterInCache(key, type);

			return type;
		}

		private void BuildConstructor(AbstractTypeEmitter emitter)
		{
			var constructor = emitter.CreateConstructor(new ArgumentReference(typeof(object)),
			                                            new ArgumentReference(typeof(IntPtr)));
			constructor.ConstructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
		}

		private void BuildInvokeMethod(AbstractTypeEmitter @delegate)
		{
			var paramTypes = GetParamTypes(@delegate);
			var invoke = @delegate.CreateMethod("Invoke",
			                                    MethodAttributes.Public |
			                                    MethodAttributes.HideBySig |
			                                    MethodAttributes.NewSlot |
			                                    MethodAttributes.Virtual,
			                                    @delegate.GetClosedParameterType(method.Method.ReturnType),
			                                    paramTypes);
			invoke.MethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
		}

		private AbstractTypeEmitter Generate()
		{
			var emitter = GetEmitter();
			BuildConstructor(emitter);
			BuildInvokeMethod(emitter);
			return emitter;
		}

		private AbstractTypeEmitter GetEmitter()
		{
			var suggestedName = string.Format("Castle.Proxies.Delegates.{0}_{1}",
			                                  method.MethodOnTarget.DeclaringType.Name,
			                                  method.Method.Name);
			var uniqueName = namingScope.ParentScope.GetUniqueName(suggestedName);

			var @delegate = new ClassEmitter(moduleScope,
			                                 uniqueName,
			                                 typeof(MulticastDelegate),
			                                 Type.EmptyTypes,
			                                 DelegateFlags);
			if (targetType.ContainsGenericParameters)
			{
				@delegate.CopyGenericParametersFromType(targetType, method.Method);
			}
			else
			{
				@delegate.CopyGenericParametersFromMethod(method.Method);
			}
			return @delegate;
		}

		private Type[] GetParamTypes(AbstractTypeEmitter @delegate)
		{
			var parameters = method.MethodOnTarget.GetParameters();
			if (@delegate.TypeBuilder.IsGenericType)
			{
				var types = new Type[parameters.Length];

				for (var i = 0; i < parameters.Length; i++)
				{
					types[i] = @delegate.GetClosedParameterType(parameters[i].ParameterType);
				}
				return types;
			}
			var paramTypes = new Type[parameters.Length + 1];
			paramTypes[0] = targetType;
			for (var i = 0; i < parameters.Length; i++)
			{
				paramTypes[i + 1] = @delegate.GetClosedParameterType(parameters[i].ParameterType);
			}
			return paramTypes;
		}
	}
}