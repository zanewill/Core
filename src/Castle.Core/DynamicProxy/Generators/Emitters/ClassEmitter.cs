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

namespace Castle.DynamicProxy.Generators.Emitters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;

	public class ClassEmitter : AbstractTypeEmitter
	{
		private const TypeAttributes DefaultAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable;

		private readonly ModuleScope moduleScope;

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, Type[] interfaces)
			: this(modulescope, name, baseType, interfaces, DefaultAttributes, ShouldForceUnsigned())
		{
		}

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, Type[] interfaces, TypeAttributes flags)
			: this(modulescope, name, baseType, interfaces, flags, ShouldForceUnsigned())
		{
		}

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, Type[] interfaces, TypeAttributes flags, bool forceUnsigned)
			: this(CreateTypeBuilder(modulescope, name, baseType, interfaces, flags, forceUnsigned))
		{
			interfaces = InitializeGenericArgumentsFromBases(ref baseType, interfaces);

			if (interfaces != null)
			{
				foreach (var inter in interfaces)
				{
					TypeBuilder.AddInterfaceImplementation(inter);
				}
			}
			var namingScope = modulescope.NamingScope.SafeSubScope();
			var cache = new List<string>();
			CollectGenericParameters(baseType, namingScope, cache);
			if (interfaces != null)
			{
				foreach (var @interface in interfaces)
				{
					CollectGenericParameters(@interface, namingScope, cache);
				}
			}
			DefineGenericParameters(cache);
			TypeBuilder.SetParent(baseType);
			moduleScope = modulescope;
		}

		public ClassEmitter(TypeBuilder typeBuilder)
			: base(typeBuilder)
		{
		}

		public ModuleScope ModuleScope
		{
			get { return moduleScope; }
		}

		protected virtual Type[] InitializeGenericArgumentsFromBases(ref Type baseType, Type[] interfaces)
		{
			if (baseType != null && baseType.IsGenericTypeDefinition)
			{
				throw new NotSupportedException("ClassEmitter does not support open generic base types. Type: " + baseType.FullName);
			}
			return interfaces;
		}

		private void CollectGenericParameters(Type type, INamingScope namingScope, IList<string> cache)
		{
			if (type.IsGenericTypeDefinition == false)
			{
				return;
			}
			var arguments = type.GetGenericArguments();
			foreach (var argument in arguments)
			{
				cache.Add(namingScope.GetUniqueName(argument.Name));
			}
		}

		private void DefineGenericParameters(IList<string> cache)
		{
			if (cache.Count == 0)
			{
				return;
			}
			var arguments = TypeBuilder.DefineGenericParameters(cache.ToArray());
		}

		private static TypeBuilder CreateTypeBuilder(ModuleScope modulescope, string name, Type baseType,
		                                             IEnumerable<Type> interfaces,
		                                             TypeAttributes flags, bool forceUnsigned)
		{
			var isAssemblySigned = !forceUnsigned && !StrongNameUtil.IsAnyTypeFromUnsignedAssembly(baseType, interfaces);
			return modulescope.DefineType(isAssemblySigned, name, flags);
		}

		private static bool ShouldForceUnsigned()
		{
			return StrongNameUtil.CanStrongNameAssembly == false;
		}
	}
}