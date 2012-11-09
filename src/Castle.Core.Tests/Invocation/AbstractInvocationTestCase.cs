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

namespace CastleTests
{
	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.Internal;

	using NUnit.Framework;

	[TestFixture]
	public abstract class AbstractInvocationTestCase : BasePEVerifyTestCase
	{
		protected KeepDataInterceptor interceptor;
		private FakeInvocation expected;

		protected override void AfterInit()
		{
			interceptor = new KeepDataInterceptor();
			expected = SetUpExpectations();
		}

		private IInvocation Invocation
		{
			get { return interceptor.Invocation; }
		}

		protected abstract FakeInvocation SetUpExpectations();

		[Test]
		public void Arguments()
		{
			CollectionAssert.AreEquivalent(expected.Arguments, Invocation.Arguments, "Arguments don't match");
		}

		[Test]
		public void ConcreteMethod()
		{
			Assert.AreEqual(expected.GetConcreteMethod(), Invocation.GetConcreteMethod(),
			                "GetConcreteMethod() result doesn't match");
		}

		[Test]
		public void ConcreteMethodInvocationTarget()
		{
			var expectedMethod = expected.GetConcreteMethodInvocationTarget();
			var actualMethod = Invocation.GetConcreteMethodInvocationTarget();
			if (expectedMethod != null && actualMethod != null)
			{
				Assert.AreSame(expectedMethod.DeclaringType, actualMethod.DeclaringType,
				               "GetConcreteMethodInvocationTarget().DeclaringType don't match");
			}
			Assert.AreEqual(expectedMethod, actualMethod,
			                "GetConcreteMethodInvocationTarget() result doesn't match");
		}

		[Test]
		public void GenericArguments()
		{
			CollectionAssert.AreEquivalent(expected.GenericArguments, Invocation.GenericArguments, "GenericArguments don't match");
		}

		[Test]
		public void InvocationTarget()
		{
			Assert.AreSame(expected.InvocationTarget, Invocation.InvocationTarget, "InvocationTarget don't match");
		}

		[Test]
		public void Method()
		{
			Assert.AreSame(expected.Method, Invocation.Method, "Method don't match");
		}

		[Test]
		public void MethodInvocationTarget()
		{
			if (expected.MethodInvocationTarget != null && Invocation.MethodInvocationTarget != null)
			{
				Assert.AreSame(expected.MethodInvocationTarget.DeclaringType, Invocation.MethodInvocationTarget.DeclaringType,
				               "MethodInvocationTarget.DeclaringType don't match");
			}
			Assert.AreEqual(expected.MethodInvocationTarget, Invocation.MethodInvocationTarget,
			               "MethodInvocationTarget don't match");
		}

		[Test]
		public void Proxy()
		{
			Assert.AreSame(expected.Proxy, Invocation.Proxy, "Proxy don't match");
		}

		[Test]
		public void ReturnValue()
		{
			Assert.AreEqual(expected.ReturnValue, Invocation.ReturnValue, "ReturnValue don't match");
		}

		[Test]
		public void TargetType()
		{
			Assert.AreSame(expected.TargetType, Invocation.TargetType, "TargetType don't match");
		}
	}
}