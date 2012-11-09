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
	using System;

	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Tests.Classes;
	using Castle.DynamicProxy.Tests.InterClasses;
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;
	using CastleTests.Internal;

	using NUnit.Framework;

	[TestFixture]
	public class InvocationTestCase : BasePEVerifyTestCase
	{
		private KeepDataInterceptor interceptor;

		protected override void AfterInit()
		{
			interceptor = new KeepDataInterceptor();
		}

		private IInvocation Invocation
		{
			get { return interceptor.Invocation; }
		}

		[Test]
		public void Invocation_for_class_proxy_public_method()
		{
			var proxy = generator.CreateClassProxy<ServiceClass>(interceptor);

			proxy.Sum(20, 25);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         concreteMethodInvocationTarget: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         method: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         concreteMethod: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         arguments: new object[] { 20, 25 },
				                         genericArguments: Type.EmptyTypes,
				                         invocationTarget: proxy,
				                         proxy: proxy,
				                         returnValue: 45,
				                         targetType: typeof (ServiceClass)));
		}

		[Test]
		public void Invocation_for_class_proxy_public_method_generic()
		{
			var proxy = generator.CreateClassProxy<Generic<string>>(interceptor);

			proxy.Method(45);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethodInvocationTarget: Method<Generic<string>>(s => s.Method(0)),
				                         method: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethod: Method<Generic<string>>(s => s.Method(0)),
				                         arguments: new object[] { 45 },
				                         genericArguments: new[] { typeof (int) },
				                         invocationTarget: proxy,
				                         proxy: proxy,
				                         returnValue: default(string),
				                         targetType: typeof (Generic<string>)));
		}

		[Test]
		public void Invocation_for_class_proxy_with_taget_public_method()
		{
			var target = new ServiceClass();
			var proxy = generator.CreateClassProxyWithTarget(target, interceptor);

			proxy.Sum(20, 25);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         concreteMethodInvocationTarget: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         method: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         concreteMethod: Method<ServiceClass>(s => s.Sum(0, 0)),
				                         arguments: new object[] { 20, 25 },
				                         genericArguments: Type.EmptyTypes,
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: 45,
				                         targetType: typeof (ServiceClass)));
		}

		[Test]
		public void Invocation_for_class_proxy_with_taget_public_method_generic()
		{
			var target = new Generic<string>("foo");
			var proxy = generator.CreateClassProxyWithTarget(target, interceptor);

			proxy.Method(45);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethodInvocationTarget: Method<Generic<string>>(s => s.Method(0)),
				                         method: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethod: Method<Generic<string>>(s => s.Method(0)),
				                         arguments: new object[] { 45 },
				                         genericArguments: new[] { typeof (int) },
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: "foo",
				                         targetType: typeof (Generic<string>)));
		}

		[Test]
		public void Invocation_for_interface_proxy_with_target()
		{
			var target = new ServiceImpl();
			var proxy = generator.CreateInterfaceProxyWithTarget<IService>(target, interceptor);

			proxy.Sum(20, 25);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: Method<ServiceImpl>(s => s.Sum(0, 0)),
				                         concreteMethodInvocationTarget: Method<ServiceImpl>(s => s.Sum(0, 0)),
				                         method: Method<IService>(s => s.Sum(0, 0)),
				                         concreteMethod: Method<IService>(s => s.Sum(0, 0)),
				                         arguments: new object[] { 20, 25 },
				                         genericArguments: Type.EmptyTypes,
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: 45,
				                         targetType: typeof (ServiceImpl)));
		}

		[Test]
		public void Invocation_for_interface_proxy_with_target_generic()
		{
			var target = new Generic<string>("foo");
			var proxy = generator.CreateInterfaceProxyWithTarget<IGeneric<string>>(target, interceptor);

			proxy.Method(45);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethodInvocationTarget: Method<Generic<string>>(s => s.Method(0)),
				                         method: MethodOpen<IGeneric<string>>(s => s.Method(0)),
				                         concreteMethod: Method<IGeneric<string>>(s => s.Method(0)),
				                         arguments: new object[] { 45 },
				                         genericArguments: new[] { typeof (int) },
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: "foo",
				                         targetType: typeof (Generic<string>)));
		}

		[Test]
		public void Invocation_for_interface_proxy_with_target_interface()
		{
			var target = new ServiceImpl();
			var proxy = generator.CreateInterfaceProxyWithTargetInterface<IService>(target, interceptor);

			proxy.Sum(20, 25);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: Method<ServiceImpl>(s => s.Sum(0, 0)),
				                         concreteMethodInvocationTarget: Method<ServiceImpl>(s => s.Sum(0, 0)),
				                         method: Method<IService>(s => s.Sum(0, 0)),
				                         concreteMethod: Method<IService>(s => s.Sum(0, 0)),
				                         arguments: new object[] { 20, 25 },
				                         genericArguments: Type.EmptyTypes,
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: 45,
				                         targetType: typeof (ServiceImpl)));
		}

		[Test]
		public void Invocation_for_interface_proxy_with_target_interface_generic()
		{
			var target = new Generic<string>("foo");
			var proxy = generator.CreateInterfaceProxyWithTargetInterface<IGeneric<string>>(target, interceptor);

			proxy.Method(45);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: MethodOpen<Generic<string>>(s => s.Method(0)),
				                         concreteMethodInvocationTarget: Method<Generic<string>>(s => s.Method(0)),
				                         method: MethodOpen<IGeneric<string>>(s => s.Method(0)),
				                         concreteMethod: Method<IGeneric<string>>(s => s.Method(0)),
				                         arguments: new object[] { 45 },
				                         genericArguments: new[] { typeof (int) },
				                         invocationTarget: target,
				                         proxy: proxy,
				                         returnValue: "foo",
				                         targetType: typeof (Generic<string>)));
		}

		[Test]
		public void Invocation_for_interface_proxy_without_target()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IService>(interceptor);

			proxy.Sum(20, 25);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: null,
				                         concreteMethodInvocationTarget: null,
				                         method: Method<IService>(s => s.Sum(0, 0)),
				                         concreteMethod: Method<IService>(s => s.Sum(0, 0)),
				                         arguments: new object[] { 20, 25 },
				                         genericArguments: Type.EmptyTypes,
				                         invocationTarget: null,
				                         proxy: proxy,
				                         returnValue: 0,
				                         targetType: null));
		}

		[Test]
		public void Invocation_for_interface_proxy_without_target_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGeneric<string>>(interceptor);

			proxy.Method(45);
			Invocation.AssertIsEqual(new FakeInvocation(
				                         methodInvocationTarget: null,
				                         concreteMethodInvocationTarget: null,
				                         method: MethodOpen<IGeneric<string>>(s => s.Method(0)),
				                         concreteMethod: Method<IGeneric<string>>(s => s.Method(0)),
				                         arguments: new object[] { 45 },
				                         genericArguments: new[] { typeof (int) },
				                         invocationTarget: null,
				                         proxy: proxy,
				                         returnValue: default(string),
				                         targetType: null));
		}
	}
}