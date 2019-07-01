using System.Text;

namespace Monkey.Objects
{
    public class String : IObject, IHashable
    {
        public string Value { get; set; }

        public ObjectType Type => ObjectType.String;

        public string Inspect() => Value;

        public HashKey HashKey() => new HashKey(Type, Fnv1A64(Encoding.UTF8.GetBytes(Value)));

        // FNV Hash http://www.isthe.com/chongo/tech/comp/fnv/
        //private static ulong Fnv1A64(byte[] bytes)
        public static ulong Fnv1A64(byte[] bytes)
        {
            ulong fnvPrime = 1099511628211ul;
            ulong offsetBasis = 14695981039346656037ul;
            var hash = offsetBasis;
            foreach (var b in bytes)
            {
                hash ^= b;
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}
