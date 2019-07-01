using System;
using System.Collections.Generic;

namespace Monkey.Objects
{
    public class Builtin : IObject
    {
        public Func<IEnumerable<IObject>, IObject> Fn { get; set; }

        public ObjectType Type => ObjectType.Builtin;

        public string Inspect() => "builtin function";
    }
}
