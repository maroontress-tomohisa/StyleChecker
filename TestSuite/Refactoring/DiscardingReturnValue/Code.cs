namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System.Collections.Immutable;
    using System.IO;
    using StyleChecker.Annotations;

    public sealed class Code
    {
        private byte[] array = new byte[4];

        public void Read(Stream stream)
        {
            stream.Read(array, 0, array.Length);
//@         ^System.IO.Stream.${Read}
        }

        public void Read(BinaryReader reader)
        {
            reader.Read(array, 0, array.Length);
//@         ^System.IO.BinaryReader.${Read}
        }

        public void DoNotIgonore(Stream stream)
        {
            Read(stream, array);
//@         ^StyleChecker.Test.Refactoring.DiscardingReturnValue.Code.Read(System.IO.Stream, byte[])
        }

        [return: DoNotIgnore]
        public int Read(Stream stream, byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public void StringClass()
        {
            var s = "Hello, World.";
            s.IndexOf('W');
//@         ^string.IndexOf(char)
            s.ToLower();
//@         ^string.ToLower()
        }

        public void TypeClass()
        {
            var s = "Hello, World.";
            var t = s.GetType();
            t.GetMembers();
//@         ^System.Type.GetMembers()
        }

        public void ImmutableArrayClass()
        {
            ImmutableArray.Create("a");
//@         ^System.Collections.Immutable.ImmutableArray.Create<T>(T)
            var b = ImmutableArray.Create("b");
            b.Add("c");
//@         ^System.Collections.Immutable.ImmutableArray<T>.Add(T)
        }

        public void ImmutableDictionaryClass()
        {
            ImmutableDictionary.Create<string, string>();
//@         ^System.Collections.Immutable.ImmutableDictionary.Create<TKey, TValue>()
            var b = ImmutableDictionary.Create<string, string>();
            b.Add("k", "v");
//@         ^System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.Add(TKey, TValue)
        }

        public void ImmutableHashSetClass()
        {
            ImmutableHashSet.Create("a");
//@         ^System.Collections.Immutable.ImmutableHashSet.Create<T>(T)
            var b = ImmutableHashSet.Create("b");
            b.Add("c");
//@         ^System.Collections.Immutable.ImmutableHashSet<T>.Add(T)
        }

        public void ImmutableListClass()
        {
            ImmutableList.Create("a");
//@         ^System.Collections.Immutable.ImmutableList.Create<T>(T)
            var b = ImmutableList.Create("b");
            b.Add("c");
//@         ^System.Collections.Immutable.ImmutableList<T>.Add(T)
        }

        public void ImmutableQueueClass()
        {
            ImmutableQueue.Create("a");
//@         ^System.Collections.Immutable.ImmutableQueue.Create<T>(T)
            var b = ImmutableQueue.Create("b");
            b.Enqueue("c");
//@         ^System.Collections.Immutable.ImmutableQueue<T>.Enqueue(T)
        }

        public void ImmutableSortedDictionaryClass()
        {
            ImmutableSortedDictionary.Create<string, string>();
//@         ^System.Collections.Immutable.ImmutableSortedDictionary.Create<TKey, TValue>()
            var b = ImmutableSortedDictionary.Create<string, string>();
            b.Add("k", "v");
//@         ^System.Collections.Immutable.ImmutableSortedDictionary<TKey, TValue>.Add(TKey, TValue)
        }

        public void ImmutableSortedSetClass()
        {
            ImmutableSortedSet.Create("a");
//@         ^System.Collections.Immutable.ImmutableSortedSet.Create<T>(T)
            var b = ImmutableSortedSet.Create("b");
            b.Add("c");
//@         ^System.Collections.Immutable.ImmutableSortedSet<T>.Add(T)
        }

        public void ImmutableStackClass()
        {
            ImmutableStack.Create("a");
//@         ^System.Collections.Immutable.ImmutableStack.Create<T>(T)
            var b = ImmutableStack.Create("b");
            b.Push("c");
//@         ^System.Collections.Immutable.ImmutableStack<T>.Push(T)
        }
    }
}
