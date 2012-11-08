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

namespace CastleTests.OpenGenerics
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

	using NUnit.Framework;

	public static class OpenGenericAssertionExtensions
	{
		public static void AssertIsClosedType(this object proxy)
		{
			Assert.False(proxy.GetType().IsGenericType,
			             string.Format("Expected proxy type ({0}) to NOT be generic", proxy.GetType()));
		}

		public static void AssertIsOpenGenericType(this object proxy)
		{
			Assert.True(proxy.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", proxy.GetType()));
		}

		public static void MustBe<TType>(this MethodInfo method, Expression<Action<TType>> methodCall)
		{
			MustBe(method, ((MethodCallExpression)methodCall.Body).Method);
		}

		public static void MustBe(this MethodInfo method, MethodInfo expectedMethod)
		{
			Assert.AreEqual(expectedMethod.DeclaringType, method.DeclaringType, "Methods declaring types must be same");
			Assert.AreEqual(expectedMethod, method);
		}

		public static void MustBeOpen<TType>(this MethodInfo method, Expression<Action<TType>> methodCall)
		{
			var callExpression = (MethodCallExpression)methodCall.Body;
			Assert.AreEqual(callExpression.Method.DeclaringType, method.DeclaringType, "Methods declaring types must be same");
			Assert.True(callExpression.Method.IsGenericMethod, "Method must be generic");
			Assert.AreEqual(callExpression.Method.GetGenericMethodDefinition(), method);
		}
	}
}