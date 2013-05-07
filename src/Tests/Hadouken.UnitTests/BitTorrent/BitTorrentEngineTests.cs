﻿using System.Collections.Generic;
using Hadouken.Common.Data;
using Hadouken.Common.IO;
using Hadouken.Common.Messaging;
using NUnit.Framework;
using Hadouken.Impl.BitTorrent;
using Moq;
using Hadouken.Data.Models;
using Hadouken.Configuration;

namespace Hadouken.UnitTests.BitTorrent
{
    [TestFixture]
    public class BitTorrentEngineTests
    {
        private readonly Mock<IFileSystem> fs = new Mock<IFileSystem>();    
        private readonly Mock<IMessageBus> bus = new Mock<IMessageBus>();
        private readonly Mock<IMessageBusFactory> _factory = new Mock<IMessageBusFactory>();
        private readonly Mock<IDataRepository> repo = new Mock<IDataRepository>();
        private readonly Mock<IKeyValueStore> kvs = new Mock<IKeyValueStore>();
        private MonoTorrentEngine engine;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            HdknConfig.ConfigManager = new MemoryConfigManager();
            _factory.Setup(f => f.Create(It.IsAny<string>())).Returns(bus.Object);
        }

        [SetUp]
        public void SetUp()
        {
            engine = new MonoTorrentEngine(fs.Object, _factory.Object, repo.Object, kvs.Object);
            repo.Setup(r => r.List<TorrentInfo>()).Returns(new List<TorrentInfo>());
        }

        [TearDown]
        public void TearDown()
        {
            engine = null;
        }

        [Test]
        public void Does_not_care_about_null_data()
        {
            engine.Load();
            var t = engine.AddTorrent(null);

            Assert.IsNull(t);
        }

        [Test]
        public void Does_not_care_about_garbage_data()
        {
            engine.Load();
            var t = engine.AddTorrent(new byte[] { 6, 7, 1, 4, 5, 3, 3 }); // garbage data :)

            Assert.IsNull(t);
        }

        [Test]
        public void Can_load_and_unload_torrent()
        {
            byte[] torrent = TestHelper.LoadResource("Hadouken.UnitTests.Resources.ubuntu.torrent").ToArray();

            engine.Load();
            var manager = engine.AddTorrent(torrent, "/temp");

            Assert.IsNotNull(manager);
            Assert.IsTrue(engine.Managers.ContainsKey(manager.InfoHash));

            engine.RemoveTorrent(manager);

            Assert.IsFalse(engine.Managers.ContainsKey(manager.InfoHash));
        }
    }
}
