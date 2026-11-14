namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation;

using System;

public sealed class MethodReference
{
    public void FromCSharp10()
    {
        var all = new[]
        {
            One,
            Two,
        };
    }

    public void NewAndNull()
    {
        int? Foo() => 42;

        var all = new[]
        {
            Foo,
            new(() => null),
            null,
        };
    }

    public void Delegate()
    {
        var all = new[]
        {
            (Action)One,
            Two,
        };
    }

    public void Null()
    {
        var all = new[]
        {
            One,
            (Action)null,
        };
    }

    public void LambdaFromCSharp10()
    {
        var all = new[]
        {
            () => 1,
            () => 2,
        };
    }

    private void One()
    {
    }

    private void Two()
    {
    }
}
