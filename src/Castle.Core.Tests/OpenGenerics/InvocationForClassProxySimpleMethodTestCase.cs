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
	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class InvocationForClassProxySimpleMethodTestCase : BasePEVerifyTestCase
	{
		private IInvocation invocation;

		[Test]
		public void Arguments_is_empty()
		{
			CollectionAssert.IsEmpty(invocation.Arguments);
		}

		[Test]
		public void Concrete_method_invocation_target_is_on_closed_type()
		{
			invocation.GetConcreteMethodInvocationTarget().MustBe<SimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void Concrete_method_is_closed_and_on_closed_Type()
		{
			invocation.GetConcreteMethod().MustBe<SimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void GenericArguments_has_arguments_for_the_method_only()
		{
			CollectionAssert.AreEquivalent(invocation.GenericArguments, new[] { typeof (int) });
		}

		[Test]
		public void Has_no_arguments()
		{
			Assert.IsEmpty(invocation.Arguments);
		}

		[Test]
		public void Invocation_target_is_not_null()
		{
			Assert.IsNotNull(invocation.InvocationTarget);
			Assert.IsInstanceOf<SimpleGeneric<object>>(invocation.InvocationTarget);
		}

		[Test(Description = "Not sure if it should be closed... but that's how it used to be so let's stick to it")]
		public void MethodInvocationTarget_is_closed()
		{
			invocation.MethodInvocationTarget.MustBe<SimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void Method_is_closed_generic()
		{
			invocation.Method.MustBe<SimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void TargetType_is_closed()
		{
			Assert.AreEqual(typeof (SimpleGeneric<object>), invocation.TargetType);
		}

		protected override void AfterInit()
		{
			var interceptor = new KeepDataInterceptor();

			var one = generator.CreateClassProxy<SimpleGeneric<object>>(interceptor);

			one.Method<int>();

			invocation = interceptor.Invocation;
		}
	}
}