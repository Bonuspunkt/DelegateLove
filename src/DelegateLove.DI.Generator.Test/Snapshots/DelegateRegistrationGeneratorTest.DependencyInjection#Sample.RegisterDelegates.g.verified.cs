//HintName: Sample.RegisterDelegates.g.cs
// <auto-generated />
using Microsoft.Extensions.DependencyInjection;
partial class Sample
{
    static partial void RegisterDelegates(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        services.AddTransient(provider =>
        {
            return Sample.AddFactory();
        });
        services.AddTransient(provider =>
        {
            var add = provider.GetRequiredService<Add>();
            return Sample.MultiplyFactory(add);
        });
    }
}
