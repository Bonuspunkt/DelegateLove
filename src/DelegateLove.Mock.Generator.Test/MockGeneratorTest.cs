namespace DelegateLove.Mock.Generator.Test;

[UsesVerify]
public class MockGeneratorTest
{
    [Fact]
    public Task NoNamespace()
    {
        var source = """
using DelegateLove.Mock;

public delegate int Add(int a, int b);

[MockDelegate<Add>]
public partial class AddMock {}
""";

        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task WithNamespace()
    {
        var source = """
using DelegateLove.Mock;

namespace Sample;

public delegate int Add(int a, int b);

[MockDelegate<Add>]
public partial class AddMock {}
""";

        return TestHelper.Compile(source).Validate(Check.Validators);
    }

}
