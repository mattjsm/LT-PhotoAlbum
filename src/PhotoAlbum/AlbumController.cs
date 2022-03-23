using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhotoAlbum.Infrastructure;
using PhotoAlbum.Integration;
using PhotoAlbum.Model;

namespace PhotoAlbum
{
    public class AlbumController : IDisposable
    {
        private readonly IAlbumProvider albumProvider;
        private readonly IConsoleWriter consoleWriter;

        private bool disposed;

        public AlbumController(IAlbumProvider prov, IConsoleWriter writer)
        {
            this.albumProvider = prov;
            this.consoleWriter = writer;
        }

        public void ShowAlbums(IEnumerable<uint> albumNums)
        {
            this.consoleWriter.WriteLine("Retrieving album(s)...");

            var albumTask = this.albumProvider.GetAlbums(albumNums.Distinct());
            albumTask.Wait();

            var albums = albumTask.Result;
            if (albums == null)
            {
                this.consoleWriter.WriteLine("An error occurred while retrieving photo album data.");
                return;
            }

            this.DumpAlbums(albumNums, albums.ToDictionary(a => a.Id));
        }

        private void DumpAlbums(IEnumerable<uint> albumNums, Dictionary<uint, Album> albumsLookup)
        {
            foreach (var albumNum in albumNums)
            {
                var album = albumsLookup[albumNum];
                var sb = new StringBuilder().AppendLine();
                sb.Append($"Album: {album.Id}");

                if (album.Images == null)
                {
                    sb.Append(" (Not found)").AppendLine();
                }
                else
                {
                    sb.AppendLine().AppendLine("Contents:");
                    foreach (var image in album.Images)
                    {
                        sb.AppendLine($"[{image.Id}] {image.Title}");
                    }
                }

                this.consoleWriter.WriteLine(sb.ToString());
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;

            if (disposing)
            {
                this.albumProvider.Dispose();
            }

            this.disposed = true;
        }
    }
}
