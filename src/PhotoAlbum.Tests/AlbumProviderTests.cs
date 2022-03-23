using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PhotoAlbum.Infrastructure;
using PhotoAlbum.Integration;
using PhotoAlbum.Model;
using FluentAssertions;
using System.Net.Http;

namespace PhotoAlbum.Tests
{
    public class AlbumProviderTests
    {
        public readonly AlbumProvider SUT;
        public readonly Mock<IHttpClient> HttpClient;
        public readonly Fixture Fixture;

        public AlbumProviderTests()
        {
            this.Fixture = new Fixture();
            this.HttpClient = new Mock<IHttpClient>();
            this.SUT = new AlbumProvider(this.HttpClient.Object);
        }
    }

    [TestClass]
    public class WhenEmptyAlbumReceived: AlbumProviderTests
    {
        private uint albumId;

        [TestInitialize]
        public void Setup()
        {
            this.albumId = this.Fixture.Create<uint>();
            this.HttpClient
                .Setup(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new HttpClientResponse<List<AlbumImage>>()
                {
                    IsSuccess = true,
                    Content = new List<AlbumImage>()
                }));
        }

        private IEnumerable<Album> Execute()
        {
            var task = this.SUT.GetAlbums(new []{ this.albumId });
            task.Wait();
            return task.Result;
        }

        [TestMethod]
        public void AnEmptyAlbumIsReturned()
        {
            var albums = Execute();
            albums.Should().HaveCount(1);
            albums.First().Images.Should().BeNull();
        }
    }

    [TestClass]
    public class WhenAnExceptionIsThrown : AlbumProviderTests
    {
        [TestInitialize]
        public void Setup()
        {
            this.HttpClient
                .Setup(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()))
                .Throws(new HttpRequestException("test"));
        }

        private IEnumerable<Album> Execute()
        {
            var task = this.SUT.GetAlbums(this.Fixture.CreateMany<uint>());
            task.Wait();
            return task.Result;
        }

        [TestMethod]
        public void NullIsReturned()
        {
            var albums = Execute();
            albums.Should().BeNull();
        }
    }

    [TestClass]
    public class WhenDuplicateIDsAreSubmitted : AlbumProviderTests
    {
        private IEnumerable<Album> testItems;

        [TestInitialize]
        public void Setup()
        {
            var album = this.Fixture.Create<Album>();
            this.testItems = new[] { album, album };

            var albumIds = this.testItems.Select(a => a.Id);

            this.HttpClient
                .Setup(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()))
                .Returns<string>(url =>
                {
                    var id = int.Parse(url.Substring(url.LastIndexOf("=") + 1));
                    var images = testItems.First(a => a.Id == id).Images.ToList();
                    return Task.FromResult(new HttpClientResponse<List<AlbumImage>>()
                    {
                        IsSuccess = true,
                        Content = images
                    });
                });
        }

        private IEnumerable<Album> Execute()
        {
            var args = this.testItems.Select(a => a.Id);
            var task = this.SUT.GetAlbums(args);
            task.Wait();
            return task.Result;
        }

        [TestMethod]
        public void HttpClientIsCalledTwice()
        {
            Execute();
            this.HttpClient.Verify(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TwoAlbumsAreReturned()
        {
            var albums = Execute();
            albums.Should().HaveCount(this.testItems.Count());
        }
    }

    [TestClass]
    public class WhenValidResultsReceived : AlbumProviderTests
    {
        private IEnumerable<Album> testItems;

        [TestInitialize]
        public void Setup()
        {
            this.testItems = this.Fixture.CreateMany<Album>();

            var albumIds = this.testItems.Select(a => a.Id);

            this.HttpClient
                .Setup(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()))
                .Returns<string>(url =>
                    {
                        var id = int.Parse(url.Substring(url.LastIndexOf("=")+1));
                        var images = testItems.First(a => a.Id == id).Images.ToList();
                        return Task.FromResult(new HttpClientResponse<List<AlbumImage>>()
                        {
                            IsSuccess = true,
                            Content = images
                        });
                    });
        }

        private IEnumerable<Album> Execute()
        {
            var args = this.testItems.Select(a => a.Id);
            var task = this.SUT.GetAlbums(args);
            task.Wait();
            return task.Result;
        }

        [TestMethod]
        public void HttpClientIsCalledOncePerAlbum()
        {
            Execute();
            this.HttpClient.Verify(c => c.GetAsync<List<AlbumImage>>(It.IsAny<string>()), Times.Exactly(this.testItems.Count()));
        }

        [TestMethod]
        public void AllAlbumsAreReturned()
        {
            var albums = Execute();

            albums.Should().HaveCount(this.testItems.Count());
            foreach (var source in this.testItems)
            {
                var testAlbum = this.testItems.FirstOrDefault(a => a.Id == source.Id);
                testAlbum.Should().NotBeNull();
            }
        }

        [TestMethod]
        public void AllAlbumsContainAllImages()
        {
            var albums = Execute();

            foreach (var testAlbum in this.testItems)
            {
                var album = albums.FirstOrDefault(a => a.Id == testAlbum.Id);
                album.Should().NotBeNull();
                foreach (var img in testAlbum.Images)
                {
                    album.Images.Should().Contain(img);
                }
            }
        }
    }
}
