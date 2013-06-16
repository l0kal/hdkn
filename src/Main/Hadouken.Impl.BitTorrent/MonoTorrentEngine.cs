using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

using Hadouken.BitTorrent;
using Hadouken.Data;
using MonoTorrent.Client;
using Hadouken.Data.Models;

using HdknTorrentState = Hadouken.BitTorrent.TorrentState;
using Hadouken.IO;
using Hadouken.Configuration;
using MonoTorrent;
using EncryptionTypes = MonoTorrent.Client.Encryption.EncryptionTypes;
using MtTorrent = MonoTorrent.Common.Torrent;
using Hadouken.Events.BitTorrent;
using Hadouken.Events.Configuration;
using MonoTorrent.BEncoding;

namespace Hadouken.Impl.BitTorrent
{
    [Component]
    public class MonoTorrentEngine : IBitTorrentEngine
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IKeyValueStore _keyValueStore;
        private readonly IDataRepository _repository;
        private readonly IFileSystem _fileSystem;

        private readonly ITorrentEventPublisher _torrentPublisher;
        private readonly IConfigurationEventListener _configListener;

        private readonly string _torrentFileSavePath;

        private ClientEngine _clientEngine;
        private readonly IDictionary<string, ITorrentManager> _torrents = new Dictionary<string, ITorrentManager>();

        public MonoTorrentEngine(IFileSystem fs, ITorrentEventPublisher torrentPublisher, IConfigurationEventListener configListener, IDataRepository data, IKeyValueStore kvs)
        {
            _keyValueStore = kvs;
            _repository = data;
            _fileSystem = fs;

            _torrentPublisher = torrentPublisher;
            _configListener = configListener;

            _configListener.OnChanged<string>(SettingChanged);

            _torrentFileSavePath = Path.Combine(HdknConfig.GetPath("Paths.Data"), "Torrents");
        }

        public void Load()
        {
            Logger.Info("Loading BitTorrent engine");

            if(!_fileSystem.DirectoryExists(_torrentFileSavePath))
                _fileSystem.CreateDirectory(_torrentFileSavePath);

            LoadEngine();
            LoadState();
        }

        private void SettingChanged(string key)
        {
            if(_clientEngine == null)
                return;

            var setting = _keyValueStore.Get(key);

            switch(key)
            {
                case "bandwidth.globalMaxConnections":
                    _clientEngine.Settings.GlobalMaxConnections = Convert.ToInt32(setting);
                    break;

                case "bt.listenPort":
                    var newPort = Convert.ToInt32(setting);
                    _clientEngine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, newPort));
                    break;

                case "paths.defaultSavePath":
                    _clientEngine.Settings.SavePath = setting.ToString();
                    break;
            }
        }

        private void LoadEngine()
        {
            string defaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string savePath = _keyValueStore.Get<string>("paths.defaultSavePath", defaultSavePath);
            int listenPort = _keyValueStore.Get<int>("bt.listenPort", 6998);

            var settings = new EngineSettings
                               {
                                   AllowedEncryption = EncryptionTypes.All,
                                   GlobalMaxConnections = _keyValueStore.Get("bandwidth.globalMaxConnections", 200),
                                   GlobalMaxDownloadSpeed = _keyValueStore.Get("bandwidth.globalMaxDlSpeed", 0),
                                   GlobalMaxHalfOpenConnections = _keyValueStore.Get("bandwidth.globalMaxHalfConnections", 100),
                                   GlobalMaxUploadSpeed = _keyValueStore.Get("bandwidth.globalMaxUpSpeed", 0),
                                   //HaveSupressionEnabled
                                   MaxOpenFiles = _keyValueStore.Get("diskio.maxOpenFiles", 0),
                                   MaxReadRate = _keyValueStore.Get("diskio.maxReadRate", 0),
                                   MaxWriteRate = _keyValueStore.Get("diskio.maxWriteRate", 0),
                                   PreferEncryption = _keyValueStore.Get("bt.preferEncryption", true),
                                   //ReportedAddress = _kvs.Get<string>("bt.reportedAddress")
                                   SavePath = savePath
                               };

            _clientEngine = new ClientEngine(settings);
            _clientEngine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, listenPort));
        }

        private void LoadState()
        {
            Logger.Info("Loading torrent state");

            var datFile = Path.Combine(HdknConfig.GetPath("Paths.Data"), "state.dat");
            var oldDatFile = datFile + ".old";

            if (!_fileSystem.FileExists(datFile))
                return;

            BEncodedDictionary dat = null;

            try
            {
                Logger.Trace("Loading dat file {0} to BEncoded dictionary", datFile);
                using (var s = _fileSystem.OpenRead(datFile))
                {
                    dat = BEncodedValue.Decode<BEncodedDictionary>(s);
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("Could not load dat file " + datFile, e);
            }

            if (dat != null)
            {
                var torrentsPath = Path.Combine(HdknConfig.GetPath("Paths.Data"), "Torrents");

                foreach (var key in dat.Keys)
                {
                    var torrentFile = Path.Combine(torrentsPath, key.Text);

                    if (!_fileSystem.FileExists(torrentFile))
                    {
                        Logger.Error("Could not find torrent {0}", torrentFile);
                        continue;
                    }

                    var d = dat[key] as BEncodedDictionary;
                    var savePath = ((BEncodedString) d["path"]).Text;

                    MtTorrent t;

                    Logger.Trace("Loading state for torrent {0}", torrentFile);

                    if (MtTorrent.TryLoad(torrentFile, out t))
                    {
                        var manager = RegisterTorrentManager(new TorrentManager(t, savePath, new TorrentSettings()));
                        manager.SetStateData(d);
                    }
                }
            }

            // Move state file after loading state
            if (_fileSystem.FileExists(oldDatFile))
                _fileSystem.DeleteFile(oldDatFile);

            _fileSystem.MoveFile(datFile, oldDatFile);
        }

        private void SaveState()
        {
            Logger.Info("Saving torrent state");

            var datFile = Path.Combine(HdknConfig.GetPath("Paths.Data"), "state.dat");
            var dat = new BEncodedDictionary();

            foreach (var manager in _torrents.Values.Cast<HdknTorrentManager>())
            {
                var d = manager.GetStateData();
                dat.Add(manager.Torrent.Name + ".torrent", d);
            }

            _fileSystem.WriteAllBytes(datFile, dat.Encode());
        }

        public void Unload()
        {
            SaveState();

            _clientEngine.StopAll();
            _clientEngine.Dispose();
        }

        public IDictionary<string, ITorrentManager> Managers
        {
            get { return _torrents; }
        }

        public void StartAll()
        {
            foreach (var manager in _torrents.Values)
                manager.Start();
        }

        public void StopAll()
        {
            foreach (var manager in _torrents.Values)
                manager.Stop();
        }

        public ITorrentManager AddMagnetLink(string url)
        {
            return AddMagnetLink(url, _clientEngine.Settings.SavePath);
        }

        public ITorrentManager AddMagnetLink(string url, string savePath)
        {
            var ml = new MagnetLink(url);
            return RegisterTorrentManager(new TorrentManager(ml, savePath, new TorrentSettings(), _torrentFileSavePath));
        }

        public ITorrentManager AddTorrent(byte[] data)
        {
            return AddTorrent(data, _clientEngine.Settings.SavePath);
        }

        public ITorrentManager AddTorrent(byte[] data, string savePath)
        {
            if (data == null || data.Length == 0)
                return null;

            MtTorrent t;

            if (MtTorrent.TryLoad(data, out t))
            {
                // Save the file to torrents path if it does not already exist.
                var torrentFile = Path.Combine(HdknConfig.GetPath("Paths.Data"), "Torrents", t.Name + ".torrent");

                if (!_fileSystem.FileExists(torrentFile))
                    _fileSystem.WriteAllBytes(torrentFile, data);

                if (String.IsNullOrEmpty(savePath))
                    savePath = _clientEngine.Settings.SavePath;

                var manager = RegisterTorrentManager(new TorrentManager(t, savePath, new TorrentSettings())); ;

                SaveState();

                return manager;
            }

            return null;
        }

        private HdknTorrentManager RegisterTorrentManager(TorrentManager manager)
        {
            // register with engine
            if (_clientEngine.Contains(manager))
                return null;

            _clientEngine.Register(manager);

            // add to dictionary
            var hdknManager = new HdknTorrentManager(manager, _keyValueStore, _fileSystem, _torrentPublisher);
            hdknManager.Load();

            _torrents.Add(hdknManager.InfoHash, hdknManager);

            _torrentPublisher.PublishTorrentAdded(hdknManager);

            return hdknManager;
        }

        public void RemoveTorrent(ITorrentManager manager)
        {
            var hdknManager = manager as HdknTorrentManager;

            if (hdknManager != null)
            {
                // Stop torrent

                hdknManager.Stop();

                while(hdknManager.State != HdknTorrentState.Stopped)
                    Thread.Sleep(100);

                hdknManager.Unload();

                _clientEngine.Unregister(hdknManager.Manager);

                _torrents.Remove(hdknManager.InfoHash);

                // Publish 'removed'-event. Only the InfoHash is needed since the data is invalid.
                _torrentPublisher.PublishTorrentRemoved(new {hdknManager.InfoHash});
            }
        }
    }
}
