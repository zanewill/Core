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

namespace CastleTests.DynamicProxy.Tests
{
	using System.Collections.Generic;

	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Tests.Classes;
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class ClassProxyWithTargetMixinsAndAdditionalInterfacesTestCase : BasePEVerifyTestCase
	{
		private LogInvocationInterceptor interceptor;

		protected IInvocation Invocation
		{
			get { return Invocations[0]; }
		}

		protected List<IInvocation> Invocations
		{
			get { return interceptor.Invocations; }
		}

		protected override void AfterInit()
		{
			interceptor = new LogInvocationInterceptor();
		}

		[Test]
		public void Can_create_proxy_with_additional_interface_implemented_by_proxied_class()
		{
			var target = new AbstractSimpleImpl();
			var proxy = (ISimple)generator.CreateClassProxyWithTarget(typeof(AbstractSimple), new[] { typeof(ISimple) }, target, interceptor);
			proxy.Method();
			Assert.AreSame(target, Invocation.InvocationTarget);
		}

		[Test]
		public void Can_create_proxy_with_additional_interface_implemented_by_proxied_class_explicitly()
		{
			var target = new ExplicitSimple();
			var proxy = (ISimple)generator.CreateClassProxyWithTarget(typeof(ExplicitSimple), new[] { typeof(ISimple) }, target, interceptor);
			proxy.Method();
			Assert.AreSame(target, Invocation.InvocationTarget);
		}

		[Test]
		[Bug("DYNPROXY-180")]
		public void Can_create_proxy_with_additional_interface_implemented_by_target_class()
		{
			var target = new SimpleClassWithSimpleInterface();
			var proxy = (ISimple)generator.CreateClassProxyWithTarget(typeof(SimpleClass), new[] { typeof(ISimple) }, target, interceptor);
			proxy.Method();
			Assert.AreSame(target, Invocation.InvocationTarget);
		}

		[Test]
		public void Can_create_proxy_with_additional_interface_not_implemented_by_target()
		{
			interceptor.Proceed = false;
			var target = new InheritsAbstractClassWithMethod();
			var proxy = (ISimple)generator.CreateClassProxyWithTarget(typeof(AbstractClassWithMethod), new[] { typeof(ISimple) }, target, interceptor);
			proxy.Method();
			Assert.IsNull(Invocation.InvocationTarget);
		}
	}
}