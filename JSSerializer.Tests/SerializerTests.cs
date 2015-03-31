using System;
using System.Collections.Generic;
using Xunit;

namespace JSSerializer.Tests
{
    public class SerializerTests
    {

        [Fact]
        public void Test_Serialize_Int32()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            int value = 128;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128", result);
        }

        [Fact]
        public void Test_Serialize_Int64()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            long value = 128;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128", result);
        }

        [Fact]
        public void Test_Serialize_Byte()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            byte value = 128;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128", result);
        }

        [Fact]
        public void Test_Serialize_Single()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            float value = 128.25f;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128.25", result);
        }

        [Fact]
        public void Test_Serialize_Double()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            double value = 128.25d;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128.25", result);
        }

        [Fact]
        public void Test_Serialize_Decimal()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            decimal value = 128.25m;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("128.25", result);
        }

        [Fact]
        public void Test_Serialize_Bool_True()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            bool value = true;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("true", result);
        }

        [Fact]
        public void Test_Serialize_Bool_False()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            bool value = false;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("false", result);
        }

        [Fact]
        public void Test_Serialize_Guid()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            Guid value = Guid.NewGuid();

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal(string.Format("\"{0}\"", value.ToString()), result);
        }

        [Fact]
        public void Test_Serialize_String()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            string value = "test string";

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("\"test string\"", result);
        }

        [Fact]
        public void Test_Serialize_DateTime()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            DateTime value = new DateTime(2015, 3, 15, 11, 12, 13);

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("\"2015-03-15T11:12:13.000Z\"", result);
        }

        [Fact]
        public void Test_Serialize_TimeSpan()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            TimeSpan value = TimeSpan.FromMinutes(144);

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal(string.Format("\"{0}\"", value.ToString()), result);
        }

        [Fact]
        public void Test_Serialize_ArrayOfInt()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = new int[] { 2323, 65464, 23, 1, 7654, 237, 33, 9 };

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("[2323,65464,23,1,7654,237,33,9]", result);
        }

        [Fact]
        public void Test_Serialize_ArrayOfInt_Null()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            int[] value = null;

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("null", result);
        }

        [Fact]
        public void Test_Serialize_HashSet()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = new HashSet<string> { "first", "second", "third"};

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("[\"first\",\"second\",\"third\"]", result);
        }

        [Fact]
        public void Test_Serialize_Dictionary()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = new Dictionary<int, string> {
                { 1, "first" },
                { 2, "second" },
                { 3, "third" }
            };

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("{\"1\":\"first\",\"2\":\"second\",\"3\":\"third\"}", result);
        }

        [Fact]
        public void Test_Serialize_Dictionary_2()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = new Dictionary<DateTime, string> {
                { new DateTime(2012, 1, 1), "first" },
                { new DateTime(2013, 2, 1), "second" },
                { new DateTime(2011, 11, 11), "third" }
            };

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("{\"2012-01-01T00:00:00.000Z\":\"first\",\"2013-02-01T00:00:00.000Z\":\"second\",\"2011-11-11T00:00:00.000Z\":\"third\"}", result);
        }

        [Fact]
        public void Test_Serialize_Type()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = typeof(DateTime);

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("\"" + typeof(DateTime).AssemblyQualifiedName + "\"", result);
        }

        [Fact]
        public void Test_Serialize_Object()
        {
            //Arrange
            ISerializer serializer = new Serializer();
            var value = new TestClass1();

            //Act
            var result = serializer.Serialize(value);

            //Assert
            Assert.Equal("{\"DateTimeProperty\":\"2001-05-06T07:08:09.000Z\",\"DoubleProperty\":3.14159265358979,\"StringField\":\"String field\",\"TestClass2Field\":{\"PointProperty\":{\"IsEmpty\":false,\"X\":99,\"Y\":11},\"IntField\":13}}", result);
        }
    }
}
