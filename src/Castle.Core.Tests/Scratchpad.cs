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
	using System.Linq;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	public class Scratchpad
	{
		[Test]
		public void Type_with_value_type_and_base_class_constrain()
		{
			var type = typeof(IConstraint_MethodIsTypeAndStruct<object>);
			var method = type.GetMethods().Single();
			var genericArgument = method.GetGenericArguments().Single();
			var constraints = genericArgument.GetGenericParameterConstraints();

			// one is System.ValueType, another is the actual TType
			Assert.AreEqual(2,constraints.Length);
		}
	}
}