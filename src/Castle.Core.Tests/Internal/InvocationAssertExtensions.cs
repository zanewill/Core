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

namespace CastleTests.Internal
{
	using Castle.DynamicProxy;

	using NUnit.Framework;

	public static class InvocationAssertExtensions
	{
		public static void AssertIsEqual(this IInvocation invocation, FakeInvocation expected)
		{
			Assert.AreEqual(expected.GetConcreteMethod(), invocation.GetConcreteMethod(),
			                "GetConcreteMethod() result doesn't match");
			Assert.AreEqual(expected.GetConcreteMethodInvocationTarget(), invocation.GetConcreteMethodInvocationTarget(),
			                "GetConcreteMethodInvocationTarget() result doesn't match");
			CollectionAssert.AreEquivalent(expected.Arguments, invocation.Arguments, "Arguments don't match");
			CollectionAssert.AreEquivalent(expected.GenericArguments, invocation.GenericArguments, "GenericArguments don't match");
			Assert.AreSame(expected.InvocationTarget, invocation.InvocationTarget, "InvocationTarget don't match");
			Assert.AreSame(expected.Method, invocation.Method, "Method don't match");
			Assert.AreSame(expected.InvocationTarget, invocation.InvocationTarget, "InvocationTarget don't match");
			Assert.AreSame(expected.MethodInvocationTarget, invocation.MethodInvocationTarget,
			               "MethodInvocationTarget don't match");
			Assert.AreSame(expected.Proxy, invocation.Proxy, "Proxy don't match");
			Assert.AreEqual(expected.ReturnValue, invocation.ReturnValue, "ReturnValue don't match");
			Assert.AreSame(expected.TargetType, invocation.TargetType, "TargetType don't match");
		}
	}
}