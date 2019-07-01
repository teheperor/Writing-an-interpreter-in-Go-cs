using System.Collections.Generic;
using System.Linq;

namespace Monkey.Objects
{
    public class Array : IObject
    {
        public IList<IObject> Elements { get; set; }

        public ObjectType Type => ObjectType.Array;

        public string Inspect()
        {
            var elements = Elements.Select(x => x.Inspect());
            return $"[{string.Join(", ", elements)}]";
        }
    }
}
