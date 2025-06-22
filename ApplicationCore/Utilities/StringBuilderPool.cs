using System.Text;

namespace ApplicationCore.Utilities;

public static class StringBuilderPool
{
    private static readonly Queue<StringBuilder> Pool;

    static StringBuilderPool()
    {
        Pool = new Queue<StringBuilder>();
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
    }
    
    public static StringBuilder Rent()
    {
        return Pool.Count > 0 ? Pool.Dequeue() : new StringBuilder();
    }

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        Pool.Enqueue(sb);
    }
}