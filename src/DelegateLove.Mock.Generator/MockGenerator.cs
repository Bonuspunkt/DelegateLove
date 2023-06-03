using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DelegateLove.Mock;

[Generator]
public class MockGenerator : IIncrementalGenerator
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

    private bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private const string MockAttribute = "DelegateLove.Mock.MockDelegateAttribute<T>";

    private static BuildInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context,
        CancellationToken token)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attribute in classDeclaration.AttributeLists.SelectMany(list => list.Attributes))
        {
            if (attribute.Name is not GenericNameSyntax { TypeArgumentList.Arguments: var typeList } ||
                context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol
                {
                    ContainingType: { IsGenericType: true, ConstructedFrom: var constructedFrom }
                })
            {
                continue;
            }

            if (typeList.Count == 1 &&
                constructedFrom.ToDisplayString() == MockAttribute)
            {
                return new BuildInfo(classDeclaration, typeList[0]);
            }
        }

        return null;
    }

    private record struct BuildInfo(ClassDeclarationSyntax ClassDeclaration, TypeSyntax Type);

    private static void Generate(SourceProductionContext context, Compilation compilation,
        ImmutableArray<BuildInfo> buildInfos)
    {
        foreach (var info in buildInfos)
        {
            var semanticModel = compilation.GetSemanticModel(info.ClassDeclaration.SyntaxTree);
            if (semanticModel.GetSymbolInfo(info.Type, context.CancellationToken).Symbol is not INamedTypeSymbol
                delegateSymbol || delegateSymbol?.DelegateInvokeMethod?.ReturnType == null)
            {
                continue;
            }

            context.AddSource(
                $"{info.ClassDeclaration.Identifier}.g.cs",
                Templates.GenerateMock(info.ClassDeclaration, delegateSymbol, compilation)
            );
        }
    }
}
