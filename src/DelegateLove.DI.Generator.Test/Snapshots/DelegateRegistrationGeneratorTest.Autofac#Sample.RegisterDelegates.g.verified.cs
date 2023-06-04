﻿//HintName: Sample.RegisterDelegates.g.cs
// <auto-generated />
using Autofac;
partial class Sample
{
    static partial void RegisterDelegates(Autofac.ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            return Sample.AddFactory();
        });
        builder.Register(ctx =>
        {
            var add = ctx.Resolve<Add>();
            return Sample.MultiplyFactory(add);
        });
    }
}
