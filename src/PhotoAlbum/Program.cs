using System;
using System.Collections.Generic;
using Autofac;
using PhotoAlbum.Infrastructure;
using PhotoAlbum.Integration;

namespace PhotoAlbum
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Invalid number of args; Provide comma separated Album numbers");
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

            var container = SetupDI();
            using(var cont = container.Resolve<AlbumController>())
            {
                cont.ShowAlbums(albumNumbers);
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }

        private static IContainer SetupDI()
        {
            // There are ways to automate this using conventions, attributes, etc. Seemed like overkill for this.
            // Leveraging it for the sake of testing
            var builder = new ContainerBuilder();
            builder.RegisterType<HttpClient>().As<IHttpClient>();
            builder.RegisterType<AlbumProvider>().As<IAlbumProvider>();
            builder.RegisterType<ConsoleWriter>().As<IConsoleWriter>();
            builder.RegisterType<AlbumController>().AsSelf();
            return builder.Build();
        }
    }
}
