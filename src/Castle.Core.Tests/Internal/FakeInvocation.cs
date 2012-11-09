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

namespace CastleTests.Internal
{
	using System;
	using System.Reflection;

	using Castle.DynamicProxy;

	public class FakeInvocation : IInvocation
	{
		private readonly MethodInfo concreteMethod;
		private readonly MethodInfo concreteMethodInvocationTarget;

		public FakeInvocation(MethodInfo concreteMethodInvocationTarget, MethodInfo concreteMethod, object[] arguments,
		                      Type[] genericArguments, object invocationTarget, MethodInfo method,
		                      MethodInfo methodInvocationTarget, object proxy, object returnValue, Type targetType)
		{
			this.concreteMethodInvocationTarget = concreteMethodInvocationTarget;
			this.concreteMethod = concreteMethod;
			Arguments = arguments;
			GenericArguments = genericArguments;
			InvocationTarget = invocationTarget;
			Method = method;
			MethodInvocationTarget = methodInvocationTarget;
			Proxy = proxy;
			ReturnValue = returnValue;
			TargetType = targetType;
		}

		public object[] Arguments { get; private set; }
		public Type[] GenericArguments { get; private set; }
		public object InvocationTarget { get; private set; }
		public MethodInfo Method { get; private set; }
		public MethodInfo MethodInvocationTarget { get; private set; }
		public object Proxy { get; private set; }
		public object ReturnValue { get; set; }
		public Type TargetType { get; private set; }

		public object GetArgumentValue(int index)
		{
			throw new NotImplementedException();
		}

		public MethodInfo GetConcreteMethod()
		{
			return concreteMethod;
		}

		public MethodInfo GetConcreteMethodInvocationTarget()
		{
			return concreteMethodInvocationTarget;
		}

		public void Proceed()
		{
			throw new NotImplementedException();
		}

		public void SetArgumentValue(int index, object value)
		{
			throw new NotImplementedException();
		}
	}
}