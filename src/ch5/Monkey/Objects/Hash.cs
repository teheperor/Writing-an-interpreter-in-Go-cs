using System.Collections.Generic;
using System.Linq;

namespace Monkey.Objects
{
    public class Hash : IObject
    {
        public Dictionary<HashKey, HashPair> Pairs;

        public ObjectType Type => ObjectType.Hash;

        public string Inspect()
        {
            var pairs = Pairs.Values.Select(pair => $"{pair.Key.Inspect()}: {pair.Value.Inspect()}");
            return $"{{{string.Join(", ", pairs)}}}";
        }
    }
}
