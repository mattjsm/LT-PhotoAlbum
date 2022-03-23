using System;
namespace PhotoAlbum.Integration
{
    public class ConsoleWriter : IConsoleWriter
    {
        public void Write(string str)
        {
            Console.Write(str);
        }

        public void WriteLine(string str)
        {
            Console.WriteLine(str);
        }
    }
}
