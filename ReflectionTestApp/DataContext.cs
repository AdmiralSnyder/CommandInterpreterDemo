using System;
using System.Collections.Generic;

namespace ReflectionTestApp
{
    public class DataContext
    {
        public Dictionary<uint, Student> Students = new Dictionary<uint, Student>();
        public Dictionary<uint, Room> Rooms = new Dictionary<uint, Room>();

        private readonly Dictionary<Type, Func<uint, bool>> ExistsDict = new Dictionary<Type, Func<uint, bool>>();
        private readonly Dictionary<Type, Func<uint, object>> LookupDict = new Dictionary<Type, Func<uint, object>>();
        private readonly Dictionary<Type, Func<uint, bool>> DeleteDict = new Dictionary<Type, Func<uint, bool>>();

        public DataContext()
        {
            ExistsDict[typeof(Student)] = Students.ContainsKey;
            ExistsDict[typeof(Room)] = Rooms.ContainsKey;

            LookupDict[typeof(Student)] = id => Students[id];
            LookupDict[typeof(Room)] = id => Rooms[id];

            DeleteDict[typeof(Student)] = Students.Remove;
            DeleteDict[typeof(Room)] = Rooms.Remove;
        }

        public static DataContext Current { get; } = new DataContext();

        internal string AddInstance(object newInstance)
        {
            if (newInstance is Student newStudent)
            {
                var newid = newStudent.ID = Student.NextID++;
                Students.Add(newid, newStudent);
                return newid.ToString();
            }
            else if (newInstance is Room newRoom)
            {
                var newid = newRoom.ID = Room.NextID++;
                Rooms.Add(newid, newRoom);
                return newid.ToString();
            }
            throw new Exception("Unknown object type");
        }

        internal bool Exists(Type dataType, uint id) => ExistsDict[dataType](id);
        internal object Get(Type dataType, uint id) => LookupDict[dataType](id);
        internal void Delete(Type dataType, uint id) => DeleteDict[dataType](id);
    }
}
