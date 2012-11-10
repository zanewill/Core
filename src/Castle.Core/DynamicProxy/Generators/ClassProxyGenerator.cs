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
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.DynamicProxy.Contributors;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Internal;
	using Castle.DynamicProxy.Serialization;

	public class ClassProxyGenerator : BaseProxyGenerator
	{
		private readonly Type[] additionalInterfacesToProxy;
		private readonly Type[] genericArguments;

		public ClassProxyGenerator(ModuleScope scope, Type targetType, Type[] additionalInterfacesToProxy,
		                           ProxyGenerationOptions options) : base(scope, targetType, options)
		{
			this.additionalInterfacesToProxy = additionalInterfacesToProxy;
			EnsureDoesNotImplementIProxyTargetAccessor(targetType, "targetType");
			this.targetType = GetTargetType(targetType, additionalInterfacesToProxy ?? Type.EmptyTypes, ProxyGenerationOptions);
			if (this.targetType.IsGenericTypeDefinition)
			{
				genericArguments = targetType.GetGenericArguments();
			}
		}

		public override Type GetProxyType()
		{
			return GenerateCode(additionalInterfacesToProxy, ProxyGenerationOptions);
		}

		protected override ClassEmitter BuildClassEmitter(string typeName, Type baseType, Type[] interfaces)
		{
			var emitter = new ClassEmitterSupportingGenericsTEMP(Scope, typeName, baseType, interfaces);
			if (targetType.IsGenericTypeDefinition)
			{
				emitter.CopyGenericParametersFromType(targetType);
			}

			return emitter;
		}

		protected Type GenerateCode(Type[] interfaces, ProxyGenerationOptions options)
		{
			// make sure ProxyGenerationOptions is initialized
			options.Initialize();

			interfaces = TypeUtil.GetAllInterfaces(interfaces);
			CheckNotGenericTypeDefinitions(interfaces, "interfaces");
			var cacheKey = new CacheKey(targetType, interfaces, options);
			return ObtainProxyType(cacheKey, (n, s) => GenerateType(n, interfaces, s));
		}

		protected virtual Type GenerateType(string name, Type[] interfaces, INamingScope namingScope)
		{
			IEnumerable<ITypeContributor> contributors;
			var implementedInterfaces = GetTypeImplementerMapping(interfaces, out contributors, namingScope);

			var model = new MetaType();
			// Collect methods
			foreach (var contributor in contributors)
			{
				contributor.CollectElementsToProxy(ProxyGenerationOptions.Hook, model);
			}
			ProxyGenerationOptions.Hook.MethodsInspected();

			var emitter = BuildClassEmitter(name, targetType, implementedInterfaces);

			CreateFields(emitter);
			CreateTypeAttributes(emitter);

			// Constructor
			var cctor = GenerateStaticConstructor(emitter);

			var constructorArguments = new List<FieldReference>();
			foreach (var contributor in contributors)
			{
				contributor.Generate(emitter, ProxyGenerationOptions);

				// TODO: redo it
				if (contributor is MixinContributor)
				{
					constructorArguments.AddRange((contributor as MixinContributor).Fields);
				}
			}

			// constructor arguments
			var interceptorsField = emitter.GetField("__interceptors");
			constructorArguments.Add(interceptorsField);
			var selector = emitter.GetField("__selector");
			if (selector != null)
			{
				constructorArguments.Add(selector);
			}

			GenerateConstructors(emitter, targetType, constructorArguments.ToArray());
			GenerateParameterlessConstructor(emitter, targetType, interceptorsField);

			// Complete type initializer code body
			CompleteInitCacheMethod(cctor.CodeBuilder);

			// Crosses fingers and build type

			var proxyType = emitter.BuildType();
			InitializeStaticFields(proxyType);
			return proxyType;
		}

		protected virtual Type[] GetTypeImplementerMapping(Type[] interfaces,
		                                                   out IEnumerable<ITypeContributor> contributors,
		                                                   INamingScope namingScope)
		{
			var methodsToSkip = new List<MethodInfo>();
			var proxyInstance = new ClassProxyInstanceContributor(targetType, methodsToSkip, interfaces, ProxyTypeConstants.Class);
			// TODO: the trick with methodsToSkip is not very nice...
			var proxyTarget = new ClassProxyTargetContributor(targetType, methodsToSkip, namingScope) { Logger = Logger };
			IDictionary<Type, ITypeContributor> typeImplementerMapping = new Dictionary<Type, ITypeContributor>();

			// Order of interface precedence:
			// 1. first target
			// target is not an interface so we do nothing

			var targetInterfaces = targetType.GetAllInterfaces();
			var additionalInterfaces = TypeUtil.GetAllInterfaces(interfaces);
			// 2. then mixins
			var mixins = new MixinContributor(namingScope, false) { Logger = Logger };
			if (ProxyGenerationOptions.HasMixins)
			{
				foreach (var mixinInterface in ProxyGenerationOptions.MixinData.MixinInterfaces)
				{
					if (targetInterfaces.Contains(mixinInterface))
					{
						// OK, so the target implements this interface. We now do one of two things:
						if (additionalInterfaces.Contains(mixinInterface) && typeImplementerMapping.ContainsKey(mixinInterface) == false)
						{
							AddMappingNoCheck(mixinInterface, proxyTarget, typeImplementerMapping);
							proxyTarget.AddInterfaceToProxy(mixinInterface);
						}
						// we do not intercept the interface
						mixins.AddEmptyInterface(mixinInterface);
					}
					else
					{
						if (!typeImplementerMapping.ContainsKey(mixinInterface))
						{
							mixins.AddInterfaceToProxy(mixinInterface);
							AddMappingNoCheck(mixinInterface, mixins, typeImplementerMapping);
						}
					}
				}
			}
			var additionalInterfacesContributor = new InterfaceProxyWithoutTargetContributor(namingScope)
			{ Logger = Logger };
			// 3. then additional interfaces
			foreach (var @interface in additionalInterfaces)
			{
				if (targetInterfaces.Contains(@interface))
				{
					if (typeImplementerMapping.ContainsKey(@interface))
					{
						continue;
					}

					// we intercept the interface, and forward calls to the target type
					AddMappingNoCheck(@interface, proxyTarget, typeImplementerMapping);
					proxyTarget.AddInterfaceToProxy(@interface);
				}
				else if (ProxyGenerationOptions.MixinData.ContainsMixin(@interface) == false)
				{
					additionalInterfacesContributor.AddInterfaceToProxy(@interface);
					AddMapping(@interface, additionalInterfacesContributor, typeImplementerMapping);
				}
			}
#if !SILVERLIGHT
			// 4. plus special interfaces
			if (targetType.IsSerializable)
			{
				AddMappingForISerializable(typeImplementerMapping, proxyInstance);
			}
#endif
			try
			{
				AddMappingNoCheck(typeof(IProxyTargetAccessor), proxyInstance, typeImplementerMapping);
			}
			catch (ArgumentException)
			{
				HandleExplicitlyPassedProxyTargetAccessor(targetInterfaces, additionalInterfaces);
			}

			contributors = new List<ITypeContributor>
			{
				proxyTarget,
				mixins,
				additionalInterfacesContributor,
				proxyInstance
			};
			return typeImplementerMapping.Keys.ToArray();
		}

		protected override Type ObtainProxyType(CacheKey cacheKey, Func<string, INamingScope, Type> factory)
		{
			var type = base.ObtainProxyType(cacheKey, factory);
			Debug.Assert(type.IsGenericType == (genericArguments != null));
			if (genericArguments != null)
			{
				var proxyType = type.MakeGenericType(genericArguments);
				InitializeStaticFields(proxyType);
				return proxyType;
			}
			return type;
		}

		private void EnsureDoesNotImplementIProxyTargetAccessor(Type type, string name)
		{
			if (!typeof(IProxyTargetAccessor).IsAssignableFrom(type))
			{
				return;
			}
			var message =
				string.Format(
					"Target type for the proxy implements {0} which is a DynamicProxy infrastructure interface and you should never implement it yourself. Are you trying to proxy an existing proxy?",
					typeof(IProxyTargetAccessor));
			throw new ArgumentException(message, name);
		}

		private static Type GetTargetType(Type type, Type[] additionalInterfaces, ProxyGenerationOptions options)
		{
			options.Initialize();
			if (type.IsGenericType && additionalInterfaces.None(i => i.IsGenericType) &&
			    options.MixinData.MixinInterfaces.None(m => m.IsGenericType))
			{
				return type.GetGenericTypeDefinition();
			}
			return type;
		}
	}
}