using System;

namespace ReflectionTestApp
{
    class Program
    {
        static string ExecuteCommand(string command) => CommandHandler.HandleCommand(command);

        static void Main(string[] args)
        {
            // hacky static stuff... I know, I know. but this is just a demo
            CommandHandler.RegisterType<Student>(nameof(Student.Name), nameof(Student.Birthday), nameof(Student.ShoeSize));
            CommandHandler.RegisterType<Room>(nameof(Room.Floor), nameof(Room.Subject));

            // made up command syntax.
            string studentID = ExecuteCommand("Create Student");
            string roomID = ExecuteCommand("Create Room");
            string nameResult = ExecuteCommand($"Set Student {studentID} Name 'Fred'");
            string deleteResult = ExecuteCommand($"Delete Room {roomID}");
            string name = ExecuteCommand($"Get Student {studentID} Name");
            Console.Write(name);
            Console.ReadLine();
        }
    }
}
