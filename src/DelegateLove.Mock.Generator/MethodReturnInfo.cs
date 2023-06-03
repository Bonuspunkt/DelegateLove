using Microsoft.CodeAnalysis;

namespace DelegateLove.Mock;

internal class MethodReturnInfo
{
    public static MethodReturnInfo Create(ITypeSymbol typeSymbol, Compilation compilation)
    {
        var voidSymbol = compilation.GetTypeByMetadataName(typeof(void).FullName);
        var taskSymbol = compilation.GetTypeByMetadataName(typeof(Task).FullName);

        if (SymbolEqualityComparer.Default.Equals(taskSymbol, typeSymbol))
            return new MethodReturnInfo(true, true);
        if (SymbolEqualityComparer.Default.Equals(taskSymbol, typeSymbol.BaseType))
            return new MethodReturnInfo(true, false);
        if (SymbolEqualityComparer.Default.Equals(voidSymbol, typeSymbol))
            return new MethodReturnInfo(false, true);
        return new MethodReturnInfo(false, false);
    }

    private MethodReturnInfo(bool isTask, bool isVoid)
    {
        IsTask = isTask;
        IsVoid = isVoid;
    }

    public bool IsTask { get; }
    public bool IsVoid { get; }

    public string Async => IsTask ? "async " : string.Empty;

    public string Return =>
        IsTask
            ? IsVoid
                ? "await "
                : "return await "
            : IsVoid
                ? string.Empty
                : "return ";
}
