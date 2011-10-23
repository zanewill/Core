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
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;

	using Castle.Core.Internal;
	using Castle.DynamicProxy.Internal;

	internal delegate GenericTypeParameterBuilder[] ApplyGenArgs(String[] argumentNames);

	internal class GenericUtil
	{
		public static GenericTypeParameterBuilder[] CopyGenericArguments(Type typeToCopyGenericsFrom, TypeBuilder builder, Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
		{
			return CopyGenericArguments(name2GenericType, builder.DefineGenericParameters, typeToCopyGenericsFrom.GetGenericArguments(), typeToCopyGenericsFrom.DeclaringType);
		}

		public static GenericTypeParameterBuilder[] CopyGenericArguments(Type typeToCopyGenericsFrom, MethodInfo methodToCopyGenericsFrom, TypeBuilder builder,
		                                                                 Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
		{
			if (methodToCopyGenericsFrom == null)
			{
				return CopyGenericArguments(typeToCopyGenericsFrom, builder, name2GenericType);
			}
			var typeArguments = typeToCopyGenericsFrom.GetGenericArguments();
			var methodArguments = methodToCopyGenericsFrom.GetGenericArguments();
			return CopyGenericArguments(name2GenericType, builder.DefineGenericParameters, typeArguments.Concat(methodArguments).ToArray(), methodToCopyGenericsFrom.DeclaringType);
		}

		public static GenericTypeParameterBuilder[] CopyGenericArguments(MethodInfo methodToCopyGenericsFrom, TypeBuilder builder, Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
		{
			return CopyGenericArguments(name2GenericType, builder.DefineGenericParameters, methodToCopyGenericsFrom.GetGenericArguments(), methodToCopyGenericsFrom.DeclaringType);
		}

		public static GenericTypeParameterBuilder[] CopyGenericArguments(MethodInfo methodToCopyGenericsFrom, MethodBuilder builder, Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
		{
			return CopyGenericArguments(name2GenericType, builder.DefineGenericParameters, methodToCopyGenericsFrom.GetGenericArguments(), methodToCopyGenericsFrom.DeclaringType);
		}

		public static void CopyGenericConstraintsAndAttributes(Dictionary<string, GenericTypeParameterBuilder> name2GenericType, string[] argumentNames, Type[] originalGenericArguments,
		                                                       GenericTypeParameterBuilder[] newGenericParameters, Type declaringType)
		{
			for (var i = 0; i < newGenericParameters.Length; i++)
			{
				var newGenericParameter = newGenericParameters[i];
				var originalGenericArgument = originalGenericArguments[i];
				CopyGenericConstraintsAndAttributes(originalGenericArguments, newGenericParameters, declaringType, newGenericParameter, originalGenericArgument);
				name2GenericType[argumentNames[i]] = newGenericParameter;
			}
		}

		public static Type ExtractCorrectType(Type paramType, Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
		{
			if (paramType.IsArray)
			{
				var rank = paramType.GetArrayRank();

				var underlyingType = paramType.GetElementType();

				if (underlyingType.IsGenericParameter)
				{
					GenericTypeParameterBuilder genericType;
					if (name2GenericType.TryGetValue(underlyingType.Name, out genericType) == false)
					{
						return paramType;
					}

					if (rank == 1)
					{
						return genericType.MakeArrayType();
					}
					return genericType.MakeArrayType(rank);
				}
				if (rank == 1)
				{
					return underlyingType.MakeArrayType();
				}
				return underlyingType.MakeArrayType(rank);
			}

			if (paramType.IsGenericParameter)
			{
				GenericTypeParameterBuilder value;
				if (name2GenericType.TryGetValue(paramType.Name, out value))
				{
					return value;
				}
			}

			return paramType;
		}

		public static Type[] ExtractParametersTypes(ParameterInfo[] baseMethodParameters, Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
		{
			var newParameters = new Type[baseMethodParameters.Length];

			for (var i = 0; i < baseMethodParameters.Length; i++)
			{
				var param = baseMethodParameters[i];
				var paramType = param.ParameterType;

				newParameters[i] = ExtractCorrectType(paramType, name2GenericType);
			}

			return newParameters;
		}

		public static Dictionary<string, GenericTypeParameterBuilder> GetGenericArgumentsMap(AbstractTypeEmitter parentEmitter)
		{
			if (parentEmitter.GenericTypeParams == null || parentEmitter.GenericTypeParams.Length == 0)
			{
				return new Dictionary<string, GenericTypeParameterBuilder>(0);
			}

			var name2GenericType = new Dictionary<string, GenericTypeParameterBuilder>(parentEmitter.GenericTypeParams.Length);
			foreach (var genType in parentEmitter.GenericTypeParams)
			{
				name2GenericType.Add(genType.Name, genType);
			}
			return name2GenericType;
		}

		private static Type AdjustConstraintToNewGenericParameters(Type constraint, Type[] originalGenericParameters, GenericTypeParameterBuilder[] newGenericParameters, Type declaringType)
		{
			if (constraint.IsGenericType)
			{
				var genericArgumentsOfConstraint = constraint.GetGenericArguments();

				for (var i = 0; i < genericArgumentsOfConstraint.Length; ++i)
				{
					genericArgumentsOfConstraint[i] = AdjustConstraintToNewGenericParameters(genericArgumentsOfConstraint[i], originalGenericParameters, newGenericParameters, declaringType);
				}
				return constraint.GetGenericTypeDefinition().MakeGenericType(genericArgumentsOfConstraint);
			}
			if (constraint.IsGenericParameter)
			{
				// Determine the source of the parameter
				if (constraint.DeclaringMethod != null)
				{
					// constraint comes from the method
					var index = Array.IndexOf(originalGenericParameters, constraint);
					Trace.Assert(index != -1, "When a generic method parameter has a constraint on another method parameter, both parameters must be declared on the same method.");
					return newGenericParameters[index];
				}
				else // parameter from surrounding type
				{
					Trace.Assert(constraint.DeclaringType.IsGenericTypeDefinition);
					Trace.Assert(declaringType.IsGenericType
					             && constraint.DeclaringType == declaringType.GetGenericTypeDefinition(),
					             "When a generic method parameter has a constraint on a generic type parameter, the generic type must be the declaring typer of the method.");

					var index = Array.IndexOf(constraint.DeclaringType.GetGenericArguments(), constraint);
					Trace.Assert(index != -1, "The generic parameter comes from the given type.");
					return declaringType.GetGenericArguments()[index]; // these are the actual, concrete types
				}
			}
			return constraint;
		}

		private static Type[] AdjustGenericConstraints(Type declaringType, GenericTypeParameterBuilder[] newGenericParameters, Type[] originalGenericArguments, Type[] constraints)
		{
			for (var i = 0; i < constraints.Length; i++)
			{
				constraints[i] = AdjustConstraintToNewGenericParameters(constraints[i], originalGenericArguments, newGenericParameters, declaringType);
			}
			return constraints;
		}

		private static GenericTypeParameterBuilder[] CopyGenericArguments(Dictionary<String, GenericTypeParameterBuilder> name2GenericType, ApplyGenArgs genericParameterGenerator, Type[] genericArguments,
		                                                                  Type declaringType)
		{
			if (genericArguments.Length == 0)
			{
				return null;
			}

			var argumentNames = genericArguments.ConvertAll(t => t.Name);
			var newGenericParameters = genericParameterGenerator(argumentNames);

			CopyGenericConstraintsAndAttributes(name2GenericType, argumentNames, genericArguments, newGenericParameters, declaringType);

			return newGenericParameters;
		}

		private static void CopyGenericConstraintsAndAttributes(Type[] originalGenericArguments, GenericTypeParameterBuilder[] newGenericParameters, Type declaringType,
		                                                        GenericTypeParameterBuilder newGenericParameter, Type originalGenericArgument)
		{
			SetGenericParameterAttributes(newGenericParameter, originalGenericArgument);
			var types = AdjustGenericConstraints(declaringType, newGenericParameters, originalGenericArguments, originalGenericArgument.GetGenericParameterConstraints());
			newGenericParameter.SetInterfaceConstraints(types);
			CopyNonInheritableAttributes(newGenericParameter, originalGenericArgument);
		}

		private static void CopyNonInheritableAttributes(GenericTypeParameterBuilder newGenericParameter, Type originalGenericArgument)
		{
			foreach (var attribute in originalGenericArgument.GetNonInheritableAttributes())
			{
				newGenericParameter.SetCustomAttribute(attribute);
			}
		}

		private static void SetGenericParameterAttributes(GenericTypeParameterBuilder newGenericParameter, Type originalGenericArgument)
		{
			var attributes = originalGenericArgument.GenericParameterAttributes;
			// we reset variance flags - they are only allowed on delegates and interfaces and we never generate any of those...
			attributes &= ~(GenericParameterAttributes.Contravariant | GenericParameterAttributes.Covariant);
			newGenericParameter.SetGenericParameterAttributes(attributes);
		}
	}
}
