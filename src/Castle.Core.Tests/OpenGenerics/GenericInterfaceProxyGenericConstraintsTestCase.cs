namespace CastleTests.OpenGenerics
{
	using Castle.DynamicProxy.Tests.Interceptors;

	using CastleTests.GenInterfaces;

	using NUnit.Framework;

	[TestFixture(Description = "No assertions - just PeVerify'ing types get generated correctly")]
	public class InterfaceProxyWithoutTargetGenericConstraintsTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Method_argument_is_constrained_to_type_argument()
		{
			var one = ProxyFor<IConstraint_MethodIsType<object>>();
			one.Method<int>();
		}
		[Test]
		public void Method_argument_is_constrained_to_type_argument_type_argument_is_reference_type()
		{
			var one = ProxyFor<IConstraint_MethodIsType_TypeIsClass<object>>();
			one.Method<int>();
		}

		private T ProxyFor<T>() where T : class
		{
			return generator.CreateInterfaceProxyWithoutTarget<T>(new DoNothingInterceptor());
		}
	}
}