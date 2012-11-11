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
	using Castle.DynamicProxy.Tests;
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.Internal;

	using NUnit.Framework;

	public class Inteceptor_selector_values_TestCase : BasePEVerifyTestCase
	{
		private ProxyGenerationOptions options;
		private LoggingInterceptorSelector selector;

		[Test]
		public void Class_proxy_abstract_method()
		{
			var proxyWithSelector = generator.CreateClassProxy<AbstractClass>(options, new DoNothingInterceptor());
			proxyWithSelector.Foo();

			Assert.AreEqual(1, selector.Entries.Count);
			Assert.AreEqual(Method<AbstractClass>(c => c.Foo()), selector.Entries[0].Method);
		}

		protected override void AfterInit()
		{
			selector = new LoggingInterceptorSelector();
			options = new ProxyGenerationOptions { Selector = selector };
		}
	}
}