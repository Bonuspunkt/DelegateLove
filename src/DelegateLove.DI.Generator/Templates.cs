using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DelegateLove.DI;

internal static class Templates
{
    public static string GenerateRegistration(MethodDeclarationSyntax methodDeclaration, ISymbol ioCSymbol, ImmutableArray<IMethodSymbol> factories)
    {
        var framework = ResolveIoCFramework(ioCSymbol);

        var @namespace = methodDeclaration.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString();
        var typeDeclarations = methodDeclaration.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToImmutableArray();

        var builder = new IndentedStringBuilder();
        builder.AppendLine("// <auto-generated />");
        switch (framework)
        {
            case IoCFramework.Autofac:
                builder.AppendLine("using Autofac;");
                break;
            case IoCFramework.DependencyInjection:
                builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
                break;
        }

        if (!string.IsNullOrEmpty(@namespace))
        {
            builder.AppendLine($"namespace {@namespace};");
            builder.AppendLine();
        }

        foreach (var type in typeDeclarations)
        {
            builder.AppendLine($"{type.Modifiers} {type.Keyword} {type.Identifier}");
            builder.AppendLine("{").IncrementIndent();
        }

        var parameterName = methodDeclaration.ParameterList.Parameters[0].Identifier.ToFullString();

        var methodModifiers = string.Join(" ", methodDeclaration.Modifiers);

        builder.Append($"{methodModifiers} void {methodDeclaration.Identifier}")
            .Append($"({ioCSymbol.ToDisplayString()} {parameterName})")
            .AppendLine();
        builder.AppendLine("{").IncrementIndent();
        foreach (var factory in factories)
        {
            switch (framework)
            {
                case IoCFramework.Autofac:
                    WriteAutofacRegistration(builder, parameterName, factory);
                    break;
                case IoCFramework.DependencyInjection:
                    WriteDependencyInjectionRegistration(builder, parameterName, factory);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        builder.DecrementIndent().AppendLine("}");

        foreach (var _ in typeDeclarations)
        {
            builder.DecrementIndent().AppendLine("}");
        }

        return builder.ToString();
    }

    private static void WriteAutofacRegistration(IndentedStringBuilder builder, string parameterName, IMethodSymbol methodSymbol)
    {
        builder.AppendLine($"{parameterName}.Register(ctx =>");
        builder.AppendLine("{").IncrementIndent();
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            builder.Append($"var {parameterSymbol.Name} ");
            builder.Append($"= ctx.Resolve<{parameterSymbol.Type.ToDisplayString()}>();");
            builder.AppendLine();
        }

        var parameters = string.Join(", ", methodSymbol.Parameters.Select(parameter => parameter.Name));
        builder.Append($"return {methodSymbol.ContainingType.ToDisplayString()}");
        builder.Append($".{methodSymbol.Name}({parameters});").AppendLine();
        builder.DecrementIndent().AppendLine("});");

    }

    private static void WriteDependencyInjectionRegistration(IndentedStringBuilder builder, string parameterName, IMethodSymbol methodSymbol)
    {
        builder.AppendLine($"{parameterName}.AddTransient(provider =>");
        builder.AppendLine("{").IncrementIndent();
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            builder.Append($"var {parameterSymbol.Name} ");
            builder.Append($"= provider.GetRequiredService<{parameterSymbol.Type.ToDisplayString()}>();");
            builder.AppendLine();
        }

        var parameters = string.Join(", ", methodSymbol.Parameters.Select(parameter => parameter.Name));
        builder.Append($"return {methodSymbol.ContainingType.ToDisplayString()}");
        builder.Append($".{methodSymbol.Name}({parameters});").AppendLine();
        builder.DecrementIndent().AppendLine("});");
    }

    internal enum IoCFramework
    {
        NotSupported,
        Autofac,
        DependencyInjection,
    }


    private static IoCFramework ResolveIoCFramework(ISymbol symbol)
    {
        return symbol.ToDisplayString() switch
        {
            "Autofac.ContainerBuilder" => IoCFramework.Autofac,
            "Microsoft.Extensions.DependencyInjection.IServiceCollection" => IoCFramework.DependencyInjection,
            _ => IoCFramework.NotSupported
        };
    }
}
