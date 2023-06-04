using System.Runtime.CompilerServices;

namespace DelegateLove.DI.Generator.Test;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
