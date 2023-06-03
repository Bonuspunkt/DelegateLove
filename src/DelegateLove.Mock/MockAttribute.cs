namespace DelegateLove.Mock;

[AttributeUsage(AttributeTargets.Class)]
public class MockDelegateAttribute<T> : Attribute where T: Delegate
{

}
