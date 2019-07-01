using System;

namespace Monkey.Objects
{
    public struct HashKey: IEquatable<HashKey>
    {
        public ObjectType Type { get; private set; }
        public ulong Value { get; private set; }

        public HashKey(ObjectType type, ulong value)
        {
            Type = type;
            Value = value;
        }

        public override bool Equals(object obj) => obj is HashKey hk ? Equals(hk) : false;

        public bool Equals(HashKey hk) => Type == hk.Type && Value == hk.Value;

        public override int GetHashCode() => (int)Type * Value.GetHashCode();

        public static bool operator ==(HashKey lhs, HashKey rhs) => lhs.Equals(rhs);
        public static bool operator !=(HashKey lhs, HashKey rhs) => !lhs.Equals(rhs);
   }
}
