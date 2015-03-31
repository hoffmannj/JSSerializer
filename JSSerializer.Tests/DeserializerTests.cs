using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JSSerializer.Tests
{
    public class DeserializerTests
    {
        [Fact]
        public void Test_Deserialize_String()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"Some string\"";

            //Act
            var value = deserializer.Deserialize<string>(json);

            //Assert
            Assert.Equal("Some string", value);
        }

        [Fact]
        public void Test_Deserialize_Char()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"S\"";

            //Act
            var value = deserializer.Deserialize<char>(json);

            //Assert
            Assert.Equal('S', value);
        }

        [Fact]
        public void Test_Deserialize_DateTime()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"2015-03-28T00:00:00.000Z\"";

            //Act
            var value = deserializer.Deserialize<DateTime>(json);

            //Assert
            Assert.Equal(new DateTime(2015, 3, 28).Ticks, value.Ticks);
        }

        [Fact]
        public void Test_Deserialize_TimeSpan()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"08:12:45\"";

            //Act
            var value = deserializer.Deserialize<TimeSpan>(json);

            //Assert
            Assert.Equal(new TimeSpan(8, 12, 45).Ticks, value.Ticks);
        }

        [Fact]
        public void Test_Deserialize_Guid()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"b9505260-bed9-4be1-9e71-bb680dae8e1e\"";

            //Act
            var value = deserializer.Deserialize<Guid>(json);

            //Assert
            Assert.Equal(new Guid("b9505260-bed9-4be1-9e71-bb680dae8e1e").ToString(), value.ToString());
        }

        [Fact]
        public void Test_Deserialize_Int32()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "1278";

            //Act
            var value = deserializer.Deserialize<int>(json);

            //Assert
            Assert.Equal(1278, value);
        }

        [Fact]
        public void Test_Deserialize_Double()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "1278.72364";

            //Act
            var value = deserializer.Deserialize<double>(json);

            //Assert
            Assert.Equal(1278.72364d, value);
        }

        [Fact]
        public void Test_Deserialize_Decimal()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "1278.72364";

            //Act
            var value = deserializer.Deserialize<decimal>(json);

            //Assert
            Assert.Equal(1278.72364m, value);
        }

        [Fact]
        public void Test_Deserialize_Bool_1()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "true";

            //Act
            var value = deserializer.Deserialize<bool>(json);

            //Assert
            Assert.Equal(true, value);
        }

        [Fact]
        public void Test_Deserialize_Bool_2()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "false";

            //Act
            var value = deserializer.Deserialize<bool>(json);

            //Assert
            Assert.Equal(false, value);
        }

        [Fact]
        public void Test_Deserialize_Type()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "\"" + typeof(decimal).AssemblyQualifiedName + "\"";

            //Act
            var value = deserializer.Deserialize<Type>(json);

            //Assert
            Assert.Equal(typeof(decimal), value);
        }

        [Fact]
        public void Test_Deserialize_ArrayOfInts_List()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "[ 1, 634, 23, 8568 ]";

            //Act
            var value = deserializer.Deserialize<List<int>>(json);

            //Assert
            Assert.Equal<int>(new List<int> { 1, 634, 23, 8568 }, value);
        }

        [Fact]
        public void Test_Deserialize_ArrayOfInts_Array()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "[ 1, 634, 23, 8568 ]";

            //Act
            var value = deserializer.Deserialize<int[]>(json);

            //Assert
            Assert.Equal<int>(new int[] { 1, 634, 23, 8568 }, value);
        }

        [Fact]
        public void Test_Deserialize_Dictionary()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "{\"first\" : 1, \"second\" : 634, \"third\" : 23, \"fourth\" : 8568 }";

            //Act
            var value = deserializer.Deserialize<Dictionary<string, int>>(json);

            //Assert
            Assert.Equal(1, value["first"]);
            Assert.Equal(634, value["second"]);
            Assert.Equal(23, value["third"]);
            Assert.Equal(8568, value["fourth"]);
        }

        [Fact]
        public void Test_Deserialize_Object()
        {
            //Arrange
            var deserializer = new Deserializer();
            var json = "{\"IntField\" : 1, \"PointProperty\" : { \"IsEmpty\": false, \"X\": 13, \"Y\" : 99} }";
            var obj = new TestClass2
            {
                IntField = 1,
                PointProperty = new System.Drawing.Point
                {
                    X = 13,
                    Y = 99
                }
            };

            //Act
            var value = deserializer.Deserialize<TestClass2>(json);

            //Assert
            Assert.Equal(1, value.IntField);
            Assert.Equal(false, value.PointProperty.IsEmpty);
            Assert.Equal(13, value.PointProperty.X);
            Assert.Equal(99, value.PointProperty.Y);
        }
    }
}
