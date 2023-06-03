using FluentAssertions;

namespace DelegateLove.Mock.Test;

public partial class Integration
{
    private delegate int Int2Int(int number);

    [MockDelegate<Int2Int>]
    private partial class Int2IntMock { }

    [Fact]
    public void Int2IntTest()
    {
        var mock = new Int2IntMock(x => x);
        mock.Instance(5);

        mock.Called.Should().Be(1);
        mock.CallParameters.Should().SatisfyRespectively(
            p => p.number.Should().Be(5)
        );
    }


    private delegate void Void2Void();

    [MockDelegate<Void2Void>]
    private partial class Void2VoidMock { }

    [Fact]
    public void Void2VoidTest()
    {
        var mock = new Void2VoidMock(() => { });
        mock.Instance();
        mock.Instance();

        mock.Called.Should().Be(2);
    }


    private delegate Task Void2Task();

    [MockDelegate<Void2Task>]
    private partial class Void2TaskMock{}

    [Fact]
    public async Task Void2TaskMockTest()
    {
        var mock = new Void2TaskMock(() => Task.CompletedTask);
        await mock.Instance();

        mock.Called.Should().Be(1);
    }


    private delegate Task<int> String2TaskInt(string input);

    [MockDelegate<String2TaskInt>]
    private partial class String2TaskIntMock { }

    [Fact]
    public async Task String2TaskIntMockTest()
    {
        const string input = "oh Hi";

        var mock = new String2TaskIntMock(str => Task.FromResult( str.Length));
        await mock.Instance(input);

        mock.Called.Should().Be(1);
        mock.CallParameters.Should().SatisfyRespectively(
            p => p.input.Should().Be(input)
        );
    }
}
