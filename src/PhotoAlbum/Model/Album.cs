using System.Collections.Generic;

namespace PhotoAlbum.Model
{
    public class Album
    {
        public uint Id { get; set; }
        public IEnumerable<AlbumImage> Images { get; set; }
    }
}
