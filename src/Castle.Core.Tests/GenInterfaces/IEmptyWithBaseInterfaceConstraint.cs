namespace CastleTests.GenInterfaces
{
	using Castle.DynamicProxy.Tests.Interfaces;

	public interface IEmptyWithBaseInterfaceConstraint<T> where T:IEmpty
	{
	}
}