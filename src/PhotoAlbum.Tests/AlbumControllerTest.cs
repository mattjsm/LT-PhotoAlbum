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
using FluentAssertions.Execution;

namespace PhotoAlbum.Tests
{
    public class AlbumControllerTests
    {
        public readonly AlbumController SUT;
        public readonly Mock<IAlbumProvider> AlbumProvider;
        public readonly Mock<IConsoleWriter> ConsoleWriter;
        public readonly Fixture Fixture;

        public AlbumControllerTests()
        {
            this.Fixture = new Fixture();
            this.AlbumProvider = new Mock<IAlbumProvider>();
            this.ConsoleWriter = new Mock<IConsoleWriter>();
            this.SUT = new AlbumController(this.AlbumProvider.Object, this.ConsoleWriter.Object);
        }
    }

    [TestClass]
    public class WhenAlbumsAreReturned : AlbumControllerTests
    {
        private IEnumerable<Album> testItems;

        [TestInitialize]
        public void Setup()
        {
            this.testItems = this.Fixture.CreateMany<Album>();
            this.AlbumProvider.Setup(p =>
                p.GetAlbums(It.Is<IEnumerable<uint>>(l => l.SequenceEqual(this.testItems.Select(i => i.Id)))))
                .Returns(Task.FromResult(this.testItems));
        }

        [TestMethod]
        public void ContentsAreOutput()
        {
            this.SUT.ShowAlbums(this.testItems.Select(i => i.Id));
            this.ConsoleWriter.Verify(w => w.WriteLine(It.Is<string>(s => !s.Contains("An error occurred"))), Times.Exactly(this.testItems.Count() + 1));
        }
    }

    [TestClass]
    public class WhenAnEmptyAlbumIsReturned : AlbumControllerTests
    {
        private uint albumId;

        [TestInitialize]
        public void Setup()
        {
            this.albumId = this.Fixture.Create<uint>();
            var result = new[] { new Album() { Id = this.albumId } };
            this.AlbumProvider.Setup(p =>
                p.GetAlbums(It.IsAny<IEnumerable<uint>>()))
                .Returns(Task.FromResult(result as IEnumerable<Album>));
        }

        [TestMethod]
        public void AlbumIsReportedAsNotFound()
        {
            this.SUT.ShowAlbums(new uint[] { this.albumId });
            this.ConsoleWriter
                .Verify(w => w.WriteLine(It.Is<string>(s => !s.Contains("An error occurred") && s.Contains("Not found"))), Times.Once());
        }
    }

    [TestClass]
    public class WhenAnErrorRetrievingItemsOccurs : AlbumControllerTests
    {
        [TestInitialize]
        public void Setup()
        {
            this.AlbumProvider.Setup(p => p.GetAlbums(It.IsAny<IEnumerable<uint>>()))
                .Returns(Task.FromResult((IEnumerable<Album>)null));
        }

        [TestMethod]
        public void AnErrorIsReported()
        {
            this.SUT.ShowAlbums(new uint[] { 123 });
            this.ConsoleWriter.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("An error occurred"))), Times.Once());
        }
    }
}
