using System.Collections.Generic;

namespace Monkey.Objects
{
    public class Environment
    {
        private IDictionary<string, IObject> store;
        private Environment outer;

        public static Environment NewEnclosedEnvironment(Environment outer) =>
            new Environment { outer = outer };

        public Environment() => store = new Dictionary<string, IObject>();

        public IObject Get(string name)
        {
            store.TryGetValue(name, out var obj);
            if (obj == null && outer != null)
                obj = outer.Get(name);
            return obj;
        }

        public IObject Set(string name, IObject val)
        {
            store[name] = val;
            return val;
        }
    }
}
