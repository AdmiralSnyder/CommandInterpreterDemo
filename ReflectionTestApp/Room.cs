namespace ReflectionTestApp
{
    public class Room : IInDb
    {
        public static uint NextID { get; internal set; }
        public int Floor { get; set; }
        public Subjects Subject { get; set; }

        public uint ID { get; set; }
    }
}

public enum Subjects
{
    Maths,
    English,
    SingAndDance
}
