using System;
using System.Collections.Generic;
using PhotoAlbum.Album;

namespace PhotoAlbum
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Invalid number of args; Comma separated Album numbers");
                return;
            }

            var splitNumbers = args[0].Split(',');
            var albumNumbers = new List<uint>();
            var hasInvalid = false;
            foreach (var str in splitNumbers)
            {
                if (uint.TryParse(str, out uint num))
                {
                    albumNumbers.Add(num);
                }
                else
                {
                    Console.WriteLine($"Invalid album number: {str}");
                    hasInvalid = true;
                }
            }

            if (hasInvalid) return;

            new AlbumController().ShowAlbums(albumNumbers);
            Console.WriteLine();
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }
    }
}
