using System.Text;

namespace DelegateLove;

internal class IndentedStringBuilder
{
    private readonly StringBuilder _builder = new();

    private int _indent = 0;
    private const int DefaultIndent = 4;
    private bool _indentWritten = false;

    public IndentedStringBuilder IncrementIndent()
    {
        _indent++;
        return this;
    }
    public IndentedStringBuilder DecrementIndent()
    {
        _indent--;
        return this;
    }

    private void WriteIndent()
    {
        if (_indentWritten) return;
        _builder.Append(new string(' ', _indent * DefaultIndent));
        _indentWritten = true;
    }

    public IndentedStringBuilder Append(string text)
    {
        WriteIndent();
        _builder.Append(text);
        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        _builder.AppendLine();
        _indentWritten = false;
        return this;
    }

    public IndentedStringBuilder AppendLine(string text)
    {
        WriteIndent();
        _builder.AppendLine(text);
        _indentWritten = false;
        return this;
    }

    public override string ToString() => _builder.ToString();
}
