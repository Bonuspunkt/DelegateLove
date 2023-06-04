using DelegateLove.Mock.Generator.Test;

namespace DelegateLove.DI.Generator.Test;

[UsesVerify]
public class DelegateRegistrationGeneratorTest
{
    private const string CoreSource = """
using DelegateLove.DI;

public delegate int Add(int a, int b);
public delegate int Multiply(int a, int b);

public partial class Sample
{
    private static Add AddFactory()
    {
        return (a, b) => a + b;
    }

    private static Multiply MultiplyFactory(Add add)
    {
        int Multiply(int a, int b)
        {
            var result = 0;
            for (int i = 1; i <= b; i++)
            {
                result = add(result, a);
            }
            return result;
        }
        return Multiply;
    }
}
""";

    [Fact]
    public async Task Autofac()
    {
        var source = """
using Autofac;

""" + CoreSource + """

partial class Sample
{
    [Register(nameof(AddFactory))]
    [Register(nameof(MultiplyFactory))]
    static partial void RegisterDelegates(ContainerBuilder builder);
}
""";

        await TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public async Task DependencyInjection()
    {
        var source = """
using Microsoft.Extensions.DependencyInjection;

""" + CoreSource + """

partial class Sample
{
    [Register(nameof(AddFactory))]
    [Register(nameof(MultiplyFactory))]
    static partial void RegisterDelegates(IServiceCollection services);
}
""";

        await TestHelper.Compile(source).Validate(Check.Validators);
    }
}
