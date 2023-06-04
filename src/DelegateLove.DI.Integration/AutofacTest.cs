using Autofac;
using FluentAssertions;

namespace DelegateLove.DI.Integration;

public partial class AutofacTest
{
    private delegate void Void();

    private static Void VoidFactory()
    {
        return () => { };
    }

    [Register(nameof(VoidFactory))]
    static partial void DelegateRegistration(ContainerBuilder builder);

    [Fact]
    public void Test1()
    {
        var builder = new ContainerBuilder();
        DelegateRegistration(builder);
        var container = builder.Build();
        var @void = container.Resolve<Void>();
        @void.Should().NotBeNull();
    }
}
