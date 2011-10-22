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

namespace CastleTests.OpenGenerics
{
	using Castle.DynamicProxy;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class InterfaceProxyWithoutTargetNoInvocationTestCase : BasePEVerifyTestCase
	{
		private readonly ProxyGenerationOptions proxyNothing = new ProxyGenerationOptions(new ProxyNothingHook());

		[Test]
		public void Generic_method()
		{
			var one = ProxyFor<ISimpleGeneric<object>>();

			one.AssertIsOpenGenericType();

			one.Method<int>();
		}

		[Test]
		public void Generic_method_return()
		{
			var one = ProxyFor<ISimpleReturnGeneric<object>>();

			one.AssertIsOpenGenericType();

			one.Method<int>();
		}

		[Test]
		public void Generic_method_return_constraint_on_type_parameter()
		{
			var one = ProxyFor<ISimpleGenericConstraint<object>>();

			one.AssertIsOpenGenericType();

			one.Method<int>();
		}

		[Test]
		public void Plain_method()
		{
			var one = ProxyFor<ISimple<object>>();

			one.AssertIsOpenGenericType();

			one.Method();
		}

		[Test]
		public void Plain_method_using_generic_argument()
		{
			var one = ProxyFor<ISimpleArg<object>>();

			one.AssertIsOpenGenericType();

			one.Method(null);
		}

		[Test]
		public void Plain_method_using_generic_argument_return()
		{
			var one = ProxyFor<ISimpleReturn<object>>();

			one.AssertIsOpenGenericType();

			one.Method();
		}

		private T ProxyFor<T>() where T : class
		{
			return generator.CreateInterfaceProxyWithoutTarget<T>(proxyNothing);
		}
	}
}