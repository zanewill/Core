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

namespace CastleTests
{
	using System;
	using System.Collections.Generic;

	using Castle.DynamicProxy.Tests.Interfaces;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class OpenGenericInterfaceProxyWithoutTargetTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Can_generate_generic_and_non_generic_proxy_for_interfaces_with_same_name_one_generic_one_not()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();
			var two = generator.CreateInterfaceProxyWithoutTarget<IEmpty>();

			Assert.AreNotEqual(one.GetType(), two.GetType());
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
			Assert.False(two.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be non-generic", two.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_two_generic_interfaces_with_same_name_different_arity()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();
			var two = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string, int>>();

			Assert.AreNotEqual(one.GetType(), two.GetType());
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
			Assert.True(two.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", two.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_twice_closed_over_different_types()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();
			var two = generator.CreateInterfaceProxyWithoutTarget<IEmpty<int>>();

			Assert.AreEqual(one.GetType().GetGenericTypeDefinition(), two.GetType().GetGenericTypeDefinition());
		}

		[Test]
		public void Can_generate_generic_proxy_twice_closed_over_same_type()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();
			var two = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();

			Assert.AreEqual(one.GetType(), two.GetType());
		}

		[Test]
		public void Simple_proxy_for_open_generic_interface_is_itself_open_generic_type()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmpty<string>>();

			Assert.True(proxy.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", proxy.GetType()));
		}

		[Test]
		public void Simple_proxy_for_open_generic_interface_is_itself_open_generic_type_nested_generics()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmpty<IDictionary<Func<string>, ICollection<Predicate<int>>>>>();

			Assert.True(proxy.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", proxy.GetType()));
		}
	}
}