#pragma warning disable CS0219
#pragma warning disable CS8321

namespace Application;

using System;

public sealed class Code
{
    public void LocalVarDecl()
    {
        var underscore = 0;
        var alpha = 1;
        int fooBar = 2;
        int fooBarBaz = 3;
    }

    public void Param(string[] args)
    {
        void outString(out string s)
        {
            s = "hello world";
        }
        outString(out string hello);
        Action<int> action
            = n => Console.WriteLine($"{n}");
        Action<int, int> action2
            = (n, m) => Console.WriteLine($"{n},{m}");
        void localFunc(int v)
        {
            Console.WriteLine($"{v}");
        }
        if (this is object o)
        {
            Console.WriteLine($"{o}");
        }
        Func<int, int> identity = (underscore) => underscore;
        Func<int, int> toZero = (underscore) => 0;
        Func<int, int, int> toSecond = (underscore, a) => a;
        Func<int, int, int, int> toThird = (_, _, a) => a;
    }

    public void Catch()
    {
        try
        {
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e}");
        }
    }

    public void ForEach()
    {
        foreach (var item in new[] { "a", "b", "c" })
        {
        }
    }
}
