﻿//HintName: AddMock.g.cs
// <auto-generated />
using System.Collections.Generic;

public partial class AddMock
{
    private readonly Add _fn;

    public AddMock(Add fn)
    {
        _fn = fn;
    }

    public record Parameters(int a, int b);

    private readonly List<Parameters> _callParameters = new();
    public int Called => _callParameters.Count;
    public IReadOnlyCollection<Parameters> CallParameters => _callParameters;

    public int Instance(int a, int b)
    {
        _callParameters.Add(new Parameters(a, b));
        return _fn(a, b);
    }
}