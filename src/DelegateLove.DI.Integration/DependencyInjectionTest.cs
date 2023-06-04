using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DelegateLove.DI.Integration;

public partial class DependencyInjectionTest
{
    private delegate void Void();

    private static Void VoidFactory()
    {
        return () => { };
    }

    [Register(nameof(VoidFactory))]
    static partial void DelegateRegistration(IServiceCollection services);

    [Fact]
    public void Fact()
    {
        var services = new ServiceCollection();
        DelegateRegistration(services);

        var provider = services.BuildServiceProvider();
        var @void = provider.GetRequiredService<Void>();
        @void.Should().NotBeNull();
    }
}
