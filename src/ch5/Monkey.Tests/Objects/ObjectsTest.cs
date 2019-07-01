using Monkey.Objects;
using Xunit;

namespace Monkey.Tests.Objects
{
    public class ObjectsTest
    {
        [Fact]
        public void TestStringFnv1A64() =>
            Assert.Equal(16654208175385433931, String.Fnv1A64(System.Text.Encoding.UTF8.GetBytes("abc")));

        [Fact]
        public void TestStringHashKey()
        {
            var hello1 = new String { Value = "Hello World" };
            var hello2 = new String { Value = "Hello World" };
            var diff1 = new String { Value = "My name is Johnny" };
            var diff2 = new String { Value = "My name is Johnny" };

            Assert.Equal(hello1.HashKey(), hello2.HashKey());
            Assert.Equal(diff1.HashKey(), diff2.HashKey());
            Assert.NotEqual(hello1.HashKey(), diff1.HashKey());
        }

        [Fact]
        public void TestBooleanHashKey()
        {
            var true1 = new Boolean { Value = true };
            var true2 = new Boolean { Value = true };
            var false1 = new Boolean { Value = false };
            var false2 = new Boolean { Value = false };

            Assert.Equal(true1.HashKey(), true2.HashKey());
            Assert.Equal(false1.HashKey(), false2.HashKey());
            Assert.NotEqual(true1.HashKey(), false1.HashKey());
        }

        [Fact]
        public void TestIntegerHashKey()
        {
            var one1 = new Integer { Value = 1L };
            var one2 = new Integer { Value = 1L };
            var two1 = new Integer { Value = 2L };
            var two2 = new Integer { Value = 2L };

            Assert.Equal(one1.HashKey(), one2.HashKey());
            Assert.Equal(two1.HashKey(), two2.HashKey());
            Assert.NotEqual(one1.HashKey(), two1.HashKey());
        }
    }
}
