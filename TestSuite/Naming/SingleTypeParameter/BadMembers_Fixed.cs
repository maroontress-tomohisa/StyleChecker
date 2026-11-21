#nullable enable
namespace Application;

public sealed class BadMembers
{
    public sealed class T
    {
        public static void M<T>()
        {
        }
    }

    public sealed class LocalFunctions
    {
        public static void M()
        {
            static void Local<T>()
            {
            }
            Local<string>();
        }
    }
}
