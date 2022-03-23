using System;
using System.Threading.Tasks;

namespace PhotoAlbum.Infrastructure
{
    public interface IHttpClient : IDisposable
    {
        Task<HttpClientResponse<T>> GetAsync<T>(string url);
    }

    public class HttpClientResponse<T>
    {
        public T Content { get; set; }
        public bool IsSuccess { get; set; }
    }
}
