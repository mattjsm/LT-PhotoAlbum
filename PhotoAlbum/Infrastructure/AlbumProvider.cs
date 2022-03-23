using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using PhotoAlbum.Model;

namespace PhotoAlbum.Infrastructure
{
    public class AlbumProvider : IAlbumProvider
    {
        private const string AlbumPath = "https://jsonplaceholder.typicode.com";
        private const string PhotosPath = "photos";
        private const string AlbumIdParam = "albumId";

        private readonly IHttpClient client;

        private bool disposed;

        public AlbumProvider(IHttpClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Album>> GetAlbums(IEnumerable<uint> albumNumbers)
        {
            try
            {
                var tasks = new List<Task<Album>>(albumNumbers.Count());
                foreach (var num in albumNumbers)
                {
                    tasks.Add(GetAlbum(num));
                }

                await Task.WhenAll(tasks);
                if (tasks.All(t => t.Result != null)) //if something failed, fail it all
                {
                    return tasks.Select(t => t.Result).ToList();
                }
            }
            catch (HttpRequestException) { }
            return null;
        }

        private async Task<Album> GetAlbum(uint albumNum)
        {
            var builder = new UriBuilder(AlbumProvider.AlbumPath);
            builder.Path = AlbumProvider.PhotosPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            query[AlbumProvider.AlbumIdParam] = albumNum.ToString();
            builder.Query = query.ToString();

            try
            {
                var httpResult = await this.client.GetAsync<List<AlbumImage>>(builder.ToString());
                var album = new Album()
                {
                    Id = albumNum
                };

                if (httpResult.IsSuccess)
                {
                    album.Images = httpResult.Content.Any() ? httpResult.Content : null;
                }

                return album;
            }
            catch (HttpRequestException)
            {
                return null;
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
                this.client.Dispose();
            }

            this.disposed = true;
        }
    }
}
