using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhotoAlbum.Infrastructure
{
    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient client;
        private readonly JsonSerializerOptions jsonOptions;

        private bool disposed;

        public HttpClient()
        {
            this.client = new System.Net.Http.HttpClient();
            this.jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<HttpClientResponse<T>> GetAsync<T>(string url)
        {
            try
            {
                using (var response = await this.client.GetAsync(url))
                {
                    var result = new HttpClientResponse<T>();
                    if (response.IsSuccessStatusCode)
                    {
                        result.IsSuccess = true;
                        result.Content = await response.Content.ReadFromJsonAsync<T>(this.jsonOptions);
                    }
                    else
                    {
                        result.IsSuccess = false;
                    }

                    return result;
                }
            }
            catch (HttpRequestException)
            {
                throw;
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
