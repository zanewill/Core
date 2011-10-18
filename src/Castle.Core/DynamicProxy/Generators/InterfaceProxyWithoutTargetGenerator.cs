// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

	using Castle.Core.Internal;
	using Castle.DynamicProxy.Contributors;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Serialization;

	public class InterfaceProxyWithoutTargetGenerator : InterfaceProxyWithTargetGenerator
	{
		private readonly Type[] genericArguments;
		private readonly Type openInterface;

		public InterfaceProxyWithoutTargetGenerator(ModuleScope scope, Type @interface, Type[] additionalInterfacesToProxy, ProxyGenerationOptions proxyGenerationOptions)
			: base(scope, GetTargetType(@interface, additionalInterfacesToProxy, proxyGenerationOptions), typeof(object), additionalInterfacesToProxy, proxyGenerationOptions)
		{
			if (targetType.IsGenericTypeDefinition)
			{
				genericArguments = @interface.GetGenericArguments();
				openInterface = @interface.GetGenericTypeDefinition();
			}
		}

		protected override string GeneratorType
		{
			get { return ProxyTypeConstants.InterfaceWithoutTarget; }
		}

		protected override ITypeContributor AddMappingForTargetType(
			IDictionary<Type, ITypeContributor> interfaceTypeImplementerMapping, Type proxyTargetType,
			ICollection<Type> targetInterfaces, ICollection<Type> additionalInterfaces, INamingScope namingScope)
		{
			var contributor = new InterfaceProxyWithoutTargetContributor(namingScope, (c, m) => NullExpression.Instance)
			{ Logger = Logger };
			foreach (var @interface in targetType.GetAllInterfaces())
			{
				contributor.AddInterfaceToProxy(@interface);
				AddMappingNoCheck(@interface, contributor, interfaceTypeImplementerMapping);
			}
			return contributor;
		}

		protected override ClassEmitter BuildClassEmitter(string typeName, Type baseType, Type[] interfaces)
		{
			var emitter = base.BuildClassEmitter(typeName, baseType, interfaces);
			if(openInterface != null)
			{
				emitter.CopyGenericParametersFromType(openInterface);
			}

			return emitter;
		}

		protected override Type GenerateType(string typeName, Type proxyTargetType, Type[] interfaces, INamingScope namingScope)
		{
			ITypeContributor[] contributors;
			var allInterfaces = GetTypeImplementerMapping(interfaces, targetType, out contributors, namingScope);
			var model = new MetaType();
			// collect elements
			foreach (var contributor in contributors)
			{
				contributor.CollectElementsToProxy(ProxyGenerationOptions.Hook, model);
			}

			ProxyGenerationOptions.Hook.MethodsInspected();

			ClassEmitter emitter;
			FieldReference interceptorsField;
			var baseType = Init(typeName, out emitter, proxyTargetType, out interceptorsField, allInterfaces);

			// Constructor

			var cctor = GenerateStaticConstructor(emitter);
			var mixinFieldsList = new List<FieldReference>();

			foreach (var contributor in contributors)
			{
				contributor.Generate(emitter, ProxyGenerationOptions);

				// TODO: redo it
				if (contributor is MixinContributor)
				{
					mixinFieldsList.AddRange((contributor as MixinContributor).Fields);
				}
			}

			var ctorArguments = new List<FieldReference>(mixinFieldsList) { interceptorsField, targetField };
			var selector = emitter.GetField("__selector");
			if (selector != null)
			{
				ctorArguments.Add(selector);
			}

			GenerateConstructors(emitter, baseType, ctorArguments.ToArray());

			// Complete type initializer code body
			CompleteInitCacheMethod(cctor.CodeBuilder);

			// Crosses fingers and build type
			var generatedType = emitter.BuildType();

			InitializeStaticFields(generatedType);
			return generatedType;
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

		private static Type GetTargetType(Type @interface, Type[] additionalInterfaces, ProxyGenerationOptions options)
		{
			options.Initialize();
			if (@interface.IsGenericType && additionalInterfaces.None(i => i.IsGenericType) && options.MixinData.MixinInterfaces.None(m => m.IsGenericType))
			{
				return @interface.GetGenericTypeDefinition();
			}
			return @interface;
		}
	}
}