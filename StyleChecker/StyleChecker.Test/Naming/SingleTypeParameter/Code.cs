#pragma warning disable CS0693

namespace Application
{
    public sealed class Code<Type>
    {
        public Code(Type obj)
        {
        }

        public static Type OK<Type>(Type obj)
        {
            return obj;
        }
    }
}
