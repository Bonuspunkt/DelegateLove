using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DelegateLove.DI;

[Generator]
public class DelegateRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var buildInfos = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(info => info.HasValue)
            .Select((info, _) => info!.Value);

        var compilation = context.CompilationProvider.Combine(buildInfos.Collect());

        context.RegisterSourceOutput(compilation,
            static (context, source) => Generate(context, source.Left, source.Right));

    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
    {
        return node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private const string RegisterAttribute = "DelegateLove.DI.RegisterAttribute";

    private static BuildInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var factories = methodDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Where(attribute =>
            {
                var symbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, attribute).Symbol as IMethodSymbol;
                return symbol?.ContainingType.ToDisplayString() == RegisterAttribute;
            })
            .Select(attribute => attribute.ArgumentList?.Arguments.First().Expression)
            .OfType<InvocationExpressionSyntax>()
            .Select(expression => expression is
            {
                Expression: IdentifierNameSyntax
                {
                    Identifier.Text: "nameof",
                },
                ArgumentList.Arguments.Count: 1
            } ? expression.ArgumentList.Arguments.First().Expression : null)
            .OfType<IdentifierNameSyntax>()
            .SelectMany(identifierName => ModelExtensions.GetSymbolInfo(context.SemanticModel, identifierName).CandidateSymbols)
            .OfType<IMethodSymbol>()
            .ToImmutableArray();

        if (factories.IsEmpty) return null;

        if (methodDeclaration.ParameterList.Parameters.Count != 1)
        {
            // TODO: warning
            return null;
        }

        var parameter = methodDeclaration.ParameterList.Parameters[0];
        var symbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, parameter.Type).Symbol;
        if (symbol == null) return null;

        return new BuildInfo
        {
            Method = methodDeclaration,
            IocSymbolType = symbol,
            Factories = factories
        };
    }


    private static void Generate(SourceProductionContext context, Compilation compilation, ImmutableArray<BuildInfo> buildInfos)
    {
        foreach (var info in buildInfos)
        {
            var source = Templates.GenerateRegistration(info.Method, info.IocSymbolType, info.Factories);

            var typeDeclarations = info.Method.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Select(type => type.Identifier)
                .Reverse()
                .ToImmutableArray();

            var fileName = string.Join(".", typeDeclarations) + $".{info.Method.Identifier}.g.cs";

            context.AddSource(fileName, source);
        }
    }
}

internal record struct BuildInfo(
    MethodDeclarationSyntax Method,
    ISymbol IocSymbolType,
    ImmutableArray<IMethodSymbol> Factories
);
