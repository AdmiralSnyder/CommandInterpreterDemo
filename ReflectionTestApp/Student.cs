using System;

namespace ReflectionTestApp
{
    public class Student : IInDb
    {
        public static uint NextID { get; internal set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }

        public int ShoeSize { get; set; }

        public uint ID { get; set; }
    }
}
