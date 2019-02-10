namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Code
    {
        public void One()
        {
            var all = new[] { "a", };
        }

        public void Two()
        {
            var all = new[] { "a", "b", };
        }

        public void Three()
        {
            var all = new[] { "a", "b", "c", };
        }

        public void StringAndNull()
        {
            var all = new[] { "a", null, };
        }

        public void NullCastedToString()
        {
            var all = new[] { (string)null, };
        }

        public void ObjectAndInt32()
        {
            var all = new[] { (object)"a", 1, };
        }

        public void StringInt32AndObject()
        {
            var all = new[] { "a", 1, (object)2, };
        }

        public T[] TypeParam<T>(T first)
        {
            return new[] { first };
        }

        public void TwoDimension()
        {
            var all = new[,] { { "a", "b", }, { "c", "d", }, };
        }
    }
}