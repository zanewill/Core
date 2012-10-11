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
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class ClassProxyWithInvocationTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Generic_method_arg_on_type_and_method_has_same_name()
		{
			var one = ProxyFor<SimpleGenericSameName<object>>();

			one.AssertIsOpenGenericType();

			one.Method<int>();
		}

		[Test]
		public void Generic_type_protected_generic_method()
		{
			var one = ProxyFor<SimpleGenericProtectedGenericMethod<object>>();

			one.AssertIsOpenGenericType();

			one.Method<int>();
		}

		[Test]
		public void Generic_type_protected_method()
		{
			var one = ProxyFor<SimpleGenericProtectedMethod<object>>();

			one.AssertIsOpenGenericType();

			one.Method();
		}

		[Test]
		public void Plain_method()
		{
			var one = ProxyFor<Simple<object>>();

			one.AssertIsOpenGenericType();

			one.Method();
		}

		private T ProxyFor<T>() where T : class
		{
			return generator.CreateClassProxy<T>(new DoNothingInterceptor());
		}
	}
}