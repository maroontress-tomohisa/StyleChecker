namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection;

    public sealed class Okay
    {
        public void StringClass()
        {
            var s = "Hello, World.";
            var array = new char[s.Length];
            s.CopyTo(0, array, 0, s.Length);
        }

        public void TypeClass()
        {
            var s = "Hello, World.";
            var t = s.GetType();
            var b = Type.DefaultBinder;
            t.InvokeMember(
                "ToString",
                BindingFlags.InvokeMethod,
                b,
                t,
                new object[] {});
        }

        public void ImmutableArrayClass()
        {
            var a = ImmutableArray.Create("a");
            var b = a.Add("b");
            _ = b.Length;
        }

        public void ImmutableDictionaryClass()
        {
            var a = ImmutableDictionary.Create<string, string>();
            var b = a.Add("k", "v");
            _ = b.Count;
        }

        public void ImmutableHashSetClass()
        {
            var a = ImmutableHashSet.Create("a");
            var b = a.Add("b");
            _ = b.Count;
        }

        public void ImmutableListClass()
        {
            var a = ImmutableList.Create("a");
            var b = a.Add("b");
            _ = b.Count;
        }

        public void ImmutableQueueClass()
        {
            var a = ImmutableQueue.Create("a");
            var b = a.Enqueue("b");
            _ = b.IsEmpty;
        }

        public void ImmutableSortedDictionaryClass()
        {
            var a = ImmutableSortedDictionary.Create<string, string>();
            var b = a.Add("k", "v");
            _ = b.Count;
        }

        public void ImmutableSortedSetClass()
        {
            var a = ImmutableSortedSet.Create("a");
            var b = a.Add("b");
            _ = b.Count;
        }

        public void ImmutableStackClass()
        {
            var a = ImmutableStack.Create("a");
            var b = a.Push("b");
            _ = b.IsEmpty;
        }
    }
}
