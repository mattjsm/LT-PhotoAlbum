using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhotoAlbum.Model;

namespace PhotoAlbum.Infrastructure
{
    public interface IAlbumProvider : IDisposable
    {
        Task<IEnumerable<Album>> GetAlbums(IEnumerable<uint> albumNumbers);
    }
}