﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DelegateLove.Mock;

internal static class Templates
{
    public static string GenerateMock(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol delegateSymbol,
        Compilation compilation)
    {
        var @namespace = classDeclaration.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString();
        var typeDeclarations = classDeclaration.AncestorsAndSelf()
            .OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToImmutableArray();

        var invokeMethod = delegateSymbol.DelegateInvokeMethod!;

        var builder = new IndentedStringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
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

        var className = typeDeclarations.Last().Identifier;

        builder.AppendLine($"private readonly {delegateSymbol.Name} _fn;");
        builder.AppendLine();
        builder.AppendLine($"public {className}({delegateSymbol.Name} fn)");
        builder.AppendLine("{").IncrementIndent();
        builder.AppendLine("_fn = fn;");
        builder.DecrementIndent().AppendLine("}");
        builder.AppendLine();

        var parametersWithType = string.Join(", ", invokeMethod.Parameters.Select(p => $"{p.Type} {p.Name}"));

        builder.AppendLine($"public record Parameters({parametersWithType});");
        builder.AppendLine();
        builder.AppendLine("private readonly List<Parameters> _callParameters = new();");
        builder.AppendLine("public int Called => _callParameters.Count;");
        builder.AppendLine("public IReadOnlyCollection<Parameters> CallParameters => _callParameters;");
        builder.AppendLine();

        var returnInfo = MethodReturnInfo.Create(invokeMethod.ReturnType, compilation);

        builder.AppendLine($"public {returnInfo.Async}{invokeMethod.ReturnType} Instance({parametersWithType})");
        builder.AppendLine("{").IncrementIndent();

        var parameterNames = string.Join(", ", invokeMethod.Parameters.Select(p => p.Name));

        builder.AppendLine($"_callParameters.Add(new Parameters({parameterNames}));");

        builder.AppendLine($"{returnInfo.Return}_fn({parameterNames});");
        builder.DecrementIndent().AppendLine("}");


        foreach (var _ in typeDeclarations)
        {
            builder.DecrementIndent().AppendLine("}");
        }

        return builder.ToString();
    }
}