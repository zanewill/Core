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

namespace CastleTests.OpenGenerics
{
	using System;

	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	[TestFixture(Description = "No assertions - just PeVerify'ing types get generated correctly")]
	public class InterfaceProxyWithTargetGenericConstraintsTestCase : BasePEVerifyTestCase
	{
		private T ProxyFor<T>(T target) where T : class
		{
			return generator.CreateInterfaceProxyWithTarget(target, new DoNothingInterceptor());
		}

		[Test]
		public void Method_argument_is_constrained_to_default_ctor_class_type_argument_is_contravariant()
		{
			var one =
				ProxyFor<IConstraint_MethodIsClassNew_Type_is_contravariant<object>>(
					new Constraint_MethodIsClassNew_Type_is_contravariant<object>());
			one.Method<InterfaceProxyWithoutTargetGenericConstraintsTestCase>();
		}

		[Test]
		public void Method_argument_is_constrained_to_type_and_being_value_type()
		{
			var one = ProxyFor<IConstraint_MethodIsTypeAndStruct<object>>(new Constraint_MethodIsTypeAndStruct<object>());
			one.Method<int>();
		}

		[Test]
		public void Method_argument_is_constrained_to_type_and_reference_type_argument()
		{
			var one = ProxyFor<IConstraint_MethodIsTypeAndClass<object>>(new Constraint_MethodIsTypeAndClass<object>());
			one.Method<string>();
		}

		[Test]
		public void Method_argument_is_constrained_to_type_and_value_type_argument_type_argument_is_reference_type()
		{
			var one =
				ProxyFor<IConstraint_MethodIsTypeAndStruct_TypeIsClass<object>>(
					new Constraint_MethodIsTypeAndStruct_TypeIsClass<object>());
			one.Method<int>();
		}

		[Test]
		public void Method_argument_is_constrained_to_type_argument()
		{
			var one = ProxyFor<IConstraint_MethodIsType<object>>(new Constraint_MethodIsType<object>());
			one.Method<int>();
		}

		[Test]
		public void Method_argument_is_constrained_to_type_argument_type_argument_is_reference_type()
		{
			var one = ProxyFor<IConstraint_MethodIsType_TypeIsClass<object>>(new Constraint_MethodIsType_TypeIsClass<object>());
			one.Method<int>();
		}

		[Test]
		public void Method_argument_one_is_constrained_to_type_secod_argument_and_being_value_type()
		{
			var one =
				ProxyFor<IConstraint_Method1IsTypeStructAndMethod2<object>>(new Constraint_Method1IsTypeStructAndMethod2<object>());
			one.Method<DayOfWeek, Enum>();
		}
	}
}