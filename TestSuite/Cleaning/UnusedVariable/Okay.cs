namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using StyleChecker.Annotations;

    public sealed class Okay
    {
        private int used;

        public Okay(int used)
        {
            this.used = used;
        }

        public int GetValue()
        {
            var s = used;
            return s;
        }

        public void SetValue(int used)
        {
            this.used = used;
        }

        public void PatternMatching(object o)
        {
            if (o is string s)
            {
                used = int.Parse(s);
            }
        }

        public void OutVar(Dictionary<string, string> map)
        {
            if (map.TryGetValue("key", out var v))
            {
                used = int.Parse(v);
            }
        }

        public abstract class Abstract
        {
            public abstract void Method(int unused);

            public virtual void CustomizePoint(int usedBySubclass)
            {
            }

            public abstract int this[int unused] { get; }

            public virtual int this[int unused, int ignored]
            {
                set { }
            }

            [DllImport("User32.dll")]
            public static extern void ExternMethod(int unused);
        }

        public partial class Partial
        {
            partial void PartialMethod(int unused);
        }

        public interface IInterface
        {
            void Method(int unused);

            int this[int unused] { get; }
        }

        public sealed class Concrete : Abstract
        {
            public override int this[int value] => value;

            public override void Method(int value)
            {
                CustomizePoint(value);
            }

            public override void CustomizePoint([Unused] int ignored)
            {
            }
        }

        public void StartWithAt(int @baz, Dictionary<string, string> map)
        {
            this.used = @baz;
            var @foo = "string";
            if (@foo is string @stringFoo)
            {
                this.used = int.Parse(@stringFoo);
            }
            if (map.TryGetValue("key", out var @bar))
            {
                this.used = int.Parse(@bar);
            }
        }

        public void ForEach()
        {
            var all = new[] { "a", "b", "c", };
            foreach (var e in all)
            {
                Console.WriteLine(e);
            }
        }

        public void Catch()
        {
            try
            {
                Console.WriteLine("a");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                Console.WriteLine("a");
            }
            catch (Exception)
            {
                Console.WriteLine("Exception thrown");
            }
        }

        public void Lambda()
        {
            void Print(string s, Func<string, string> func)
            {
                Console.WriteLine(func(s));
            }
            Print("foo", s => "bar");
        }
    }

    public class IndexerOkay
    {
        private int[] arr = new int[10];

        public int this[int i] => arr[i];

        public int this[int i, int j]
        {
            get => arr[i + j];
            set => arr[i + j] = value;
        }

        public string this[[Unused] string ignored] => "";
    }

    public class PrimaryBase(int x)
    {
        public int X => x;
    }

    public class PrimaryOkay(int a, int b, int c, [Unused] int ignored) : PrimaryBase(a)
    {
        private int f = b;
        public int Prop { get; } = c;
    }

    public struct PrimaryStructOkay(int a)
    {
        public int A => a;
    }

    public record RecordOkay(int a, int b);

    public record struct RecordStructOkay(int a, int b);
}
