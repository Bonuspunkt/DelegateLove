using System.Runtime.CompilerServices;

namespace DelegateLove.Mock.Generator.Test;

internal static class ModuleInitializer {
    [ModuleInitializer]
    public static void Init() {
        VerifySourceGenerators.Initialize();
    }
}
