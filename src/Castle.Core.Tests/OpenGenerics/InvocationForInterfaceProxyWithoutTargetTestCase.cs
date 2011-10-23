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

	public class InvocationForInterfaceProxyWithoutTargetSimpleMethodTestCase : BasePEVerifyTestCase
	{
		private IInvocation invocation;

		[Test]
		public void Concrete_method_invocation_target_is_null()
		{
			Assert.IsNull(invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void Concrete_method_is_on_closed_Type()
		{
			invocation.GetConcreteMethod().MustBe<ISimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void GenericArguments_has_arguments_of_the_method()
		{
			CollectionAssert.AreEqual(new[] { typeof(int) }, invocation.GenericArguments);
		}

		[Test]
		public void Has_no_arguments()
		{
			Assert.IsEmpty(invocation.Arguments);
		}

		[Test]
		public void Invocation_target_is_null()
		{
			Assert.IsNull(invocation.InvocationTarget);
		}

		[Test]
		public void MethodInvocationTarget_is_null()
		{
			Assert.IsNull(invocation.MethodInvocationTarget);
		}

		[Test(Description = "Not too sure if perhaps that should be the open version of the method?")]
		public void Method_is_open_generic()
		{
			invocation.Method.MustBe<ISimpleGeneric<object>>(g => g.Method<int>());
		}

		[Test]
		public void TargetType_is_closed()
		{
			Assert.IsNull(invocation.TargetType);
		}

		protected override void AfterInit()
		{
			var interceptor = new KeepDataInterceptor();

			var one = generator.CreateInterfaceProxyWithoutTarget<ISimpleGeneric<object>>(interceptor);

			one.Method<int>();

			invocation = interceptor.Invocation;
		}
	}
}