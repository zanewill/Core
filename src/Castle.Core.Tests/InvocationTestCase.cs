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

namespace Castle.DynamicProxy.Tests
{
	using Castle.DynamicProxy.Tests.Classes;
	using Castle.DynamicProxy.Tests.InterClasses;
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class InvocationTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void InvocationForConcreteClassProxy()
		{
			var interceptor = new KeepDataInterceptor();

			var proxy = generator.CreateClassProxy(typeof (ServiceClass), interceptor);

			var instance = (ServiceClass)proxy;

			instance.Sum(20, 25);
			var invocation = interceptor.Invocation;

			Assert.IsNotNull(invocation);

			Assert.IsNotNull(invocation.Arguments);
			Assert.AreEqual(2, invocation.Arguments.Length);
			Assert.AreEqual(20, invocation.Arguments[0]);
			Assert.AreEqual(25, invocation.Arguments[1]);
			Assert.AreEqual(20, invocation.GetArgumentValue(0));
			Assert.AreEqual(25, invocation.GetArgumentValue(1));
			Assert.AreEqual(45, invocation.ReturnValue);

			Assert.IsEmpty(invocation.GenericArguments);

			Assert.IsNotNull(invocation.Proxy);
			Assert.IsInstanceOf(typeof (ServiceClass), invocation.Proxy);

			Assert.IsNotNull(invocation.InvocationTarget);
			Assert.IsInstanceOf(typeof (ServiceClass), invocation.InvocationTarget);
			Assert.IsNotNull(invocation.TargetType);
			Assert.AreSame(typeof (ServiceClass), invocation.TargetType);

			Assert.IsNotNull(invocation.Method);
			Assert.IsNotNull(invocation.MethodInvocationTarget);
			Assert.AreSame(invocation.Method, invocation.MethodInvocationTarget.GetBaseDefinition());
		}

		[Test]
		public void InvocationForInterfaceProxyWithTarget()
		{
			var interceptor = new KeepDataInterceptor();

			var proxy = generator.CreateInterfaceProxyWithTarget(
				typeof (IService), new ServiceImpl(), interceptor);

			var instance = (IService)proxy;

			instance.Sum(20, 25);

			var invocation = interceptor.Invocation;
			Assert.IsNotNull(invocation);

			Assert.IsNotNull(invocation.Arguments);
			Assert.AreEqual(2, invocation.Arguments.Length);
			Assert.AreEqual(20, invocation.Arguments[0]);
			Assert.AreEqual(25, invocation.Arguments[1]);
			Assert.AreEqual(20, invocation.GetArgumentValue(0));
			Assert.AreEqual(25, invocation.GetArgumentValue(1));
			Assert.AreEqual(45, invocation.ReturnValue);

			Assert.IsEmpty(invocation.GenericArguments);

			Assert.IsNotNull(invocation.Proxy);
			Assert.IsNotInstanceOf(typeof (ServiceImpl), invocation.Proxy);

			Assert.IsNotNull(invocation.InvocationTarget);
			Assert.IsInstanceOf(typeof (ServiceImpl), invocation.InvocationTarget);
			Assert.IsNotNull(invocation.TargetType);
			Assert.AreSame(typeof (ServiceImpl), invocation.TargetType);

			Assert.IsNotNull(invocation.Method);
			Assert.IsNotNull(invocation.MethodInvocationTarget);
			Assert.AreNotSame(invocation.Method, invocation.MethodInvocationTarget);
		}
	}
}