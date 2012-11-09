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
	using CastleTests.GenInterfaces;
	using CastleTests.Internal;

	public class Interface_proxy_with_target_generic_method : AbstractInvocationTestCase
	{
		protected override FakeInvocation SetUpExpectations()
		{
			var target = new Generic<string>("foo");
			var proxy = generator.CreateInterfaceProxyWithTarget<IGeneric<string>>(target, interceptor);

			proxy.Method(45);

			return new FakeInvocation(
				methodInvocationTarget: MethodOpen<Generic<string>>(s => s.Method(0)),
				concreteMethodInvocationTarget: Method<Generic<string>>(s => s.Method(0)),
				method: MethodOpen<IGeneric<string>>(s => s.Method(0)),
				concreteMethod: Method<IGeneric<string>>(s => s.Method(0)),
				arguments: new object[] { 45 },
				genericArguments: new[] { typeof (int) },
				invocationTarget: target,
				proxy: proxy,
				returnValue: "foo",
				targetType: typeof (Generic<string>));
		}
	}
}