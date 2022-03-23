using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoAlbum.Infrastructure;
using PhotoAlbum.Model;

namespace PhotoAlbum
{
    public class AlbumController : IDisposable
    {
        private readonly IAlbumProvider albumProvider;

        private bool disposed;

        public AlbumController(IAlbumProvider prov)
        {
            this.albumProvider = prov;
        }

        public void ShowAlbums(IEnumerable<uint> albumNums)
        {
            Console.WriteLine("Retrieving albums...");
            Task<IEnumerable<Album>> albumTask = null;
            albumTask = this.albumProvider.GetAlbums(albumNums.Distinct());
            albumTask.Wait();

            var albums = albumTask.Result;

            if (albums == null)
            {
                Console.WriteLine("An error occurred while retrieving photo album data.");
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

                Console.WriteLine(sb.ToString());
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
