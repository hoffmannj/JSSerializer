using System.Drawing;

namespace JSSerializer.Tests
{
    public class TestClass2
    {
        public int IntField;

        public Point PointProperty { get; set; }

        public TestClass2()
        {
            IntField = 13;
            PointProperty = new Point(99, 11);
        }
    }
}
