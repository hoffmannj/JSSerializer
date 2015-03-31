using System;

namespace JSSerializer.Tests
{
    public class TestClass1
    {
        public string StringField;

        public DateTime DateTimeProperty { get; set; }

        private Guid PrivateGuidProperty { get; set; }

        public double DoubleProperty { get; set; }

        public TestClass2 TestClass2Field;

        public TestClass1()
        {
            StringField = "String field";
            DateTimeProperty = new DateTime(2001, 5, 6, 7, 8, 9, DateTimeKind.Utc);
            PrivateGuidProperty = new Guid("8114E50F-5303-408A-B37C-D035200E6E0B");
            DoubleProperty = Math.PI;
            TestClass2Field = new TestClass2();
        }
    }
}
