namespace DelegateLove.DI;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RegisterAttribute : Attribute
{
#pragma warning disable IDE0060
    public RegisterAttribute(string nameOfFactory) { }
#pragma warning restore IDE0060
}

