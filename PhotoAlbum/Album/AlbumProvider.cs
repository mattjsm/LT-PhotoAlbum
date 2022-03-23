using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Net.Http.Json;
using System.Text.Json;

namespace PhotoAlbum.Album
{
    public class AlbumProvider : IDisposable
    {
        private const string AlbumPath = "https://jsonplaceholder.typicode.com";
        private const string PhotosPath = "photos";
        private const string AlbumIdParam = "albumId";

        private readonly HttpClient client;
        private readonly JsonSerializerOptions jsonOptions;

        private bool disposed;

        public AlbumProvider()
        {
            this.client = new HttpClient();
            this.jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Album>> GetAlbums(IEnumerable<uint> albumNumbers)
        {
            try
            {
                var tasks = new List<Task<Album>>(albumNumbers.Count());
                foreach (var album in albumNumbers)
                {
                    tasks.Add(GetAlbum(album));
                }

                await Task.WhenAll(tasks);
                if (tasks.All(t => t.Result != null)) //if something failed, fail it all
                {
                    return tasks.Select(t => t.Result).ToList();
                }
            }
            catch (HttpRequestException) {}

            return null;
        }

        private async Task<Album> GetAlbum(uint albumNum)
        {
            var builder = new UriBuilder(AlbumProvider.AlbumPath);
            builder.Path = AlbumProvider.PhotosPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            query[AlbumProvider.AlbumIdParam] = albumNum.ToString();
            builder.Query = query.ToString();
            using (var result = await this.client.GetAsync(builder.ToString()))
            {
                try
                {
                    var album = new Album()
                    {
                        Id = albumNum
                    };

                    if (result.IsSuccessStatusCode)
                    {
                        var parsed = await result.Content.ReadFromJsonAsync<List<AlbumImage>>(this.jsonOptions);
                        album.Images = parsed.Any() ? parsed : null;
                    }

                    return album;
                }
                catch(HttpRequestException) { }
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
