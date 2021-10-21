using Microsoft.VisualStudio.TestTools.UnitTesting;
using SE.Serialization.Attributes;
using System.IO;

namespace SE.Serializer.Tests
{
    [TestClass]
    public class BasicTests
    {
        [AssemblyInitialize()]
        public static void MyTestInitialize(TestContext testContext)
        {
            // Warmup serializer - in other words - put classes we will use into the serializers' internal caches.
            BasicTestClass testClass = new BasicTestClass();
            byte[] bytes = Core.Serializer.Serialize(testClass);

            File.WriteAllBytes("C:\\Users\\admin\\Desktop\\test.txt", bytes);

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes);
        }

        [TestMethod]
        public void BasicSerializeDeserialize_ShouldBeCorrectData()
        {
            BasicTestClass testClass = new BasicTestClass();
            byte[] bytes = Core.Serializer.Serialize(testClass);

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes);

            Assert.IsTrue(newClass.byte1 == 55, $"Should be 55, but is {newClass.byte1}");
            Assert.IsTrue(newClass.ushort1 == 1, $"Should be 1, but is {newClass.ushort1}");
            Assert.IsTrue(newClass.int1 == 2, $"Should be 2, but is {newClass.int1}");
            Assert.IsTrue(newClass.ulong1 == 10, $"Should be 10, but is {newClass.ulong1}");
            Assert.IsTrue(newClass.float1 == 5.0f, $"Should be 5.0f, but is {newClass.float1}");
            Assert.IsTrue(newClass.double1 == 99.0f, $"Should be 99.0f, but is {newClass.double1}");
            Assert.IsTrue(newClass.bool1 == false, $"Should be false, but is {newClass.bool1}");
            Assert.IsTrue(newClass.str == "STRING VALUE", $"Should be \"STRING VALUE\", but is {newClass.str}");
        }

        [TestMethod]
        public void ModifySerializeDeserialize_ShouldBeCorrectData()
        {
            BasicTestClass testClass = new BasicTestClass {
                byte1 = 10,
                ushort1 = 99,
                int1 = 5,
                ulong1 = 20,
                float1 = 42.0f,
                double1 = 69.5f,
                bool1 = true,
                str = "ドナルド・\n \" fff \"トランプ"
            };

            byte[] bytes = Core.Serializer.Serialize(testClass);

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes);

            Assert.IsTrue(newClass.byte1 == 10, $"Should be 10, but is {newClass.byte1}");
            Assert.IsTrue(newClass.ushort1 == 99, $"Should be 99, but is {newClass.ushort1}");
            Assert.IsTrue(newClass.int1 == 5, $"Should be 5, but is {newClass.int1}");
            Assert.IsTrue(newClass.ulong1 == 20, $"Should be 20, but is {newClass.ulong1}");
            Assert.IsTrue(newClass.float1 == 42.0f, $"Should be 42.0f, but is {newClass.float1}");
            Assert.IsTrue(newClass.double1 == 69.5f, $"Should be 69.5f, but is {newClass.double1}");
            Assert.IsTrue(newClass.bool1 == true, $"Should be true, but is {newClass.bool1}");
            Assert.IsTrue(newClass.str == "ドナルド・\n \" fff \"トランプ", $"Should be, but is {newClass.str}");
        }

        // TODO: Testing null/default values, advanced classes, polymorphic classes, etc.

        [SerializeObject(ObjectSerialization.Fields)]
        public class BasicTestClass
        {
            public byte byte1 = 55;
            public ushort ushort1 = 1;
            public int int1 = 2;
            public ulong ulong1 = 10;
            public float float1 = 5.0f;
            public double double1 = 99.0f;
            public bool bool1 = false;
            public string str = "STRING \"lol\" \nVALUE";
        }
    }
}
