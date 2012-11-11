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
	using Castle.DynamicProxy.Tests.Interfaces;
	using Castle.InterClasses;

	using NUnit.Framework;

	[TestFixture]
	public class InterfaceProxyWithTargetTestCase : BasePEVerifyTestCase
	{
		[Test]
		[Bug("DYNPROXY-182")]
		public void Can_create_proxy_with_mixin_interface_implemented_by_target_class()
		{
			var options = new ProxyGenerationOptions();
			var mixin = new One();
			options.AddMixinInstance(mixin);
			var target = new OneAndEmpty();
			var interceptor = new LogInvocationInterceptor();
			var proxy = (IOne)generator.CreateInterfaceProxyWithTarget(typeof(IEmpty), target, options, interceptor);
			proxy.OneMethod();
			Assert.AreSame(target, interceptor.Invocations[0].InvocationTarget);
		}

		[Test]
		public void Invocation_type_is_reused_among_target_types()
		{
			var interceptor = new LogInvocationInterceptor();
			var proxy1 = generator.CreateInterfaceProxyWithTarget<IOne>(new One(), interceptor);
			var proxy2 = generator.CreateInterfaceProxyWithTarget<IOne>(new OneAndEmpty(), interceptor);
			proxy1.OneMethod();
			proxy2.OneMethod();

			Assert.AreSame(interceptor.Invocations[0].GetType(), interceptor.Invocations[1].GetType());
		}

		[Test]
		[Bug("DYNPROXY-177")]
		public void Proxy_types_are_reused_across_target_types()
		{
			var proxy1 = generator.CreateInterfaceProxyWithTarget<IOne>(new One());
			var proxy2 = generator.CreateInterfaceProxyWithTarget<IOne>(new OneAndEmpty());

			Assert.AreSame(proxy1.GetType(), proxy2.GetType());
		}
	}
}