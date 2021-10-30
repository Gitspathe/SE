using Microsoft.VisualStudio.TestTools.UnitTesting;
using SE.Serialization.Attributes;
using System.IO;
using SE.Serialization;

using static SE.Serializer.Tests.TestHelper;
using System.Threading.Tasks;

namespace SE.Serializer.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestInitialize]
        public void Initialize()
        {
            // Warmup serializer - in other words - put classes we will use into the serializers' internal caches.
            BasicTestClass testClass = new BasicTestClass();
            byte[] bytes = Core.Serializer.Serialize(testClass).ToArray();

            //File.WriteAllBytes("C:\\Users\\admin\\Desktop\\test.txt", bytes);

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes);
        }

        [TestMethod]
        public void SerializeDeserialize_BinaryOrder_ShouldBeCorrectData()
        {
            RunMethod1(Configs.BinaryOrder);
        }

        [TestMethod]
        public void SerializeDeserialize_BinaryNameOrder_ShouldBeCorrectData()
        {
            RunMethod1(Configs.BinaryNameAndOrder);
        }

        [TestMethod]
        public void SerializeDeserialize_TextOrder_ShouldBeCorrectData()
        {
            RunMethod1(Configs.TextOrder);
        }

        [TestMethod]
        public void SerializeDeserialize_TextName_ShouldBeCorrectData()
        {
            RunMethod1(Configs.TextName);
        }

        [TestMethod]
        public void SerializeDeserialize_TextNameOrder_ShouldBeCorrectData()
        {
            RunMethod1(Configs.TextNameAndOrder);
        }

        [TestMethod]
        public void ModifySerializeDeserialize_BinaryOrder_ShouldBeCorrectData()
        {
            RunMethod2(Configs.BinaryOrder);
        }

        [TestMethod]
        public void ModifySerializeDeserialize_BinaryNameOrder_ShouldBeCorrectData()
        {
            RunMethod2(Configs.BinaryNameAndOrder);
        }

        [TestMethod]
        public void ModifySerializeDeserialize_TextOrder_ShouldBeCorrectData()
        {
            RunMethod2(Configs.TextOrder);
        }

        [TestMethod]
        public void ModifySerializeDeserialize_TextName_ShouldBeCorrectData()
        {
            RunMethod2(Configs.TextName);
        }

        [TestMethod]
        public void ModifySerializeDeserialize_TextNameOrder_ShouldBeCorrectData()
        {
            RunMethod2(Configs.TextNameAndOrder);
        }

        public void RunMethod1(SerializerSettings settings)
        {
            BasicTestClass testClass = new BasicTestClass();
            byte[] bytes = Core.Serializer.Serialize(testClass, settings).ToArray();

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes, settings);

            Assert.IsTrue(newClass.byte1 == 55, GetFailMessage("55", newClass.byte1));
            Assert.IsTrue(newClass.ushort1 == 1, GetFailMessage("55", newClass.ushort1));
            Assert.IsTrue(newClass.int1 == 2, GetFailMessage("2", newClass.int1));
            Assert.IsTrue(newClass.ulong1 == 10, GetFailMessage("10", newClass.ulong1));
            Assert.IsTrue(newClass.float1 == 5.0f, GetFailMessage("5.0f", newClass.float1));
            Assert.IsTrue(newClass.double1 == 99.0f, GetFailMessage("99.0", newClass.double1));
            Assert.IsTrue(newClass.bool1 == false, GetFailMessage("false", newClass.bool1));
            Assert.IsTrue(newClass.str == "STRING VALUE", GetFailMessage("STRING VALUE", newClass.str));
        }

        public void RunMethod2(SerializerSettings settings)
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

            byte[] bytes = Core.Serializer.Serialize(testClass, settings).ToArray();

            BasicTestClass newClass = Core.Serializer.Deserialize<BasicTestClass>(bytes, settings);

            Assert.IsTrue(newClass.byte1 == 10, GetFailMessage("10", newClass.byte1));
            Assert.IsTrue(newClass.ushort1 == 99, GetFailMessage("99", newClass.ushort1));
            Assert.IsTrue(newClass.int1 == 5, GetFailMessage("5", newClass.int1));
            Assert.IsTrue(newClass.ulong1 == 20, GetFailMessage("20", newClass.ulong1));
            Assert.IsTrue(newClass.float1 == 42.0f, GetFailMessage("42.0f", newClass.float1));
            Assert.IsTrue(newClass.double1 == 69.5f, GetFailMessage("69.5", newClass.double1));
            Assert.IsTrue(newClass.bool1 == true, GetFailMessage("true", newClass.bool1));
            Assert.IsTrue(newClass.str == "ドナルド・\n \" fff \"トランプ", GetFailMessage("ドナルド・\n \" fff \"トランプ", newClass.str));
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
            public string str = "STRING VALUE";
        }
    }
}
