using Autofac;
using DelegateLove.DI;
using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace DelegateLove.Mock.Generator.Test;

internal static class TestHelper
{
    public static CompilationResult Compile(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        var locations = new[] {
                typeof(RegisterAttribute),
                typeof(ContainerBuilder),
                typeof(IServiceCollection)
            }
            .Select(type => type.Assembly.Location)
            .Select(location => MetadataReference.CreateFromFile(location));
        references.AddRange(locations);

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create("foo")
            .WithOptions(compilationOptions)
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        var generator = new DelegateRegistrationGenerator();

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new CompilationResult(driver, outputCompilation, diagnostics);
    }
}

internal delegate Task Validator(CompilationResult data);

internal record CompilationResult(
    GeneratorDriver Driver,
    Compilation Compilation,
    IReadOnlyCollection<Diagnostic> Diagnostics
)
{
    public async Task Validate(params Validator[] checks)
    {
        await Task.WhenAll(checks.Select(check => check(this)));
    }
}

internal static class Check {

    public static Validator[] Validators { get; } = {
        Snapshots,
        Compilation,
    };

    internal static Task Compilation(CompilationResult data) {
        data.Diagnostics.Should().BeEmpty();

        using var stream = new MemoryStream();
        var result = data.Compilation.Emit(stream);

        var filteredDiagnostics = result.Diagnostics
            .Where(diagnostic => diagnostic.Severity != DiagnosticSeverity.Hidden);

        using (new AssertionScope()) {
            result.Success.Should().Be(true);
            filteredDiagnostics.Should().BeEmpty();
        }
        return Task.CompletedTask;
    }
    internal static Task Snapshots(CompilationResult data) {
        return Verify(data.Driver)
            .UseDirectory("Snapshots");
    }
}
