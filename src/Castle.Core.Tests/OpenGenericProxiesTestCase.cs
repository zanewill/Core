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
	using Castle.DynamicProxy.Tests.GenClasses;
	using CastleTests.DynamicProxy.Tests.Explicit;
	using CastleTests.GenInterfaces;
	using CastleTests.OpenGenerics;
	using NUnit.Framework;

	public class OpenGenericProxiesTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Interface_proxy_with_target_is_open_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithTarget<ISimpleGeneric<int>>(new SimpleGenericExplicit<int>());

			proxy.AssertIsOpenGenericType();
		}

		[Test]
		public void Interface_proxy_with_target_interface_is_open_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithTargetInterface<ISimpleGeneric<int>>(new SimpleGenericExplicit<int>());

			proxy.AssertIsOpenGenericType();
		}

		[Test]
		public void Interface_proxy_without_target_is_open_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<ISimpleGeneric<int>>();

			proxy.AssertIsOpenGenericType();
		}

		[Test]
		public void Class_proxy_is_open_generic()
		{
			var proxy = generator.CreateClassProxy<GenClassWithGenMethods<int>>();

			proxy.AssertIsOpenGenericType();
		}

		[Test]
		public void Class_proxy_with_target_is_open_generic()
		{
			var proxy = generator.CreateClassProxyWithTarget(new GenClassWithGenMethods<int>());

			proxy.AssertIsOpenGenericType();
		}
	}
}