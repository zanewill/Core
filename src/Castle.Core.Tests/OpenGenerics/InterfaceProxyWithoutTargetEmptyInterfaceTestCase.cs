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
	using System;
	using System.Collections.Generic;

	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Tests.GenClasses;
	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.Interfaces;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class InterfaceProxyWithoutTargetEmptyInterfaceTestCase : BasePEVerifyTestCase
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
		public void Can_generate_generic_proxy_for_interface_with_generic_class_constraint()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyClass<object>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_constraint_base_generic_interface()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyWithBaseGenericInterfaceConstraint<GenInterfaceImpl<int>>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_constraint_base_interface()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyWithBaseInterfaceConstraint<Empty>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_new_constraint()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyNew<int>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_struct_constraint()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyStruct<int>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_variance()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyVariant<object, string>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
			IEmptyVariant<string, object> other = one;
		}

		[Test]
		public void Can_generate_generic_proxy_for_interface_with_generic_base_interface()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget<IEmptyWithBase<int>>();

			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
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
		public void Can_generate_generic_proxy_with_additional_interface()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), new[] { typeof(IEmpty) });
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_with_additional_interface_and_mixin()
		{
			var options = new ProxyGenerationOptions();
			options.AddMixinInstance(new Empty());
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), new[] { typeof(ISimple) }, options);
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Can_generate_generic_proxy_with_mixin()
		{
			var options = new ProxyGenerationOptions();
			options.AddMixinInstance(new Empty());
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), options);
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Generic_base_interface_proxy_type_allowed_closed()
		{
			var options = new ProxyGenerationOptions { BaseTypeForInterfaceProxy = typeof(ClassWithGenArgs<int>) };
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), options);
			Assert.True(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be generic", one.GetType()));
		}

		[Test]
		public void Proxy_is_non_generic_if_has_generic_additional_interfaces()
		{
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), new[] { typeof(IEmpty<string, int>) });
			Assert.False(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be non-generic", one.GetType()));
		}

		[Test]
		public void Proxy_is_non_generic_if_has_generic_mixin()
		{
			var options = new ProxyGenerationOptions();
			options.AddMixinInstance(new GenInterfaceImpl<int>());
			var one = generator.CreateInterfaceProxyWithoutTarget(typeof(IEmpty<string>), options);
			Assert.False(one.GetType().IsGenericType, string.Format("Expected proxy type ({0}) to be non-generic", one.GetType()));
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