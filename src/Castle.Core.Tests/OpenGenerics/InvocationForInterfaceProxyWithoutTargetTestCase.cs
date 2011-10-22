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
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class InvocationForInterfaceProxyWithoutTargetTestCase : BasePEVerifyTestCase
	{
		private KeepDataInterceptor interceptor;

		[Test]
		public void Plain_method()
		{
			var one = ProxyFor<ISimple<object>>();
			one.Method();

			var invocation = GetLastInvocation();
			Assert.IsEmpty(invocation.Arguments);
			Assert.IsEmpty(invocation.GenericArguments);
			invocation.GetConcreteMethod().MustBe<ISimple<object>>(g => g.Method());
		}

		[Test]
		public void Generic_method()
		{
			var one = ProxyFor<ISimpleGeneric<object>>();

			one.Method<int>();

			var invocation = GetLastInvocation();
			Assert.IsEmpty(invocation.Arguments);
			CollectionAssert.AreEqual(invocation.GenericArguments, new[] { typeof(int) });
			invocation.GetConcreteMethod().MustBe<ISimpleGeneric<object>>(g => g.Method<int>());

		}

		protected override void AfterInit()
		{
			interceptor = new KeepDataInterceptor();
		}

		private IInvocation GetLastInvocation()
		{
			return interceptor.Invocation;
		}

		private T ProxyFor<T>() where T : class
		{
			return generator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
		}
	}
}