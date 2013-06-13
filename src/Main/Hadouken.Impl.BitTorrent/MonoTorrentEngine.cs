using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hadouken.BitTorrent;
using Hadouken.Data;
using MonoTorrent.Client;
using Hadouken.Data.Models;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;
using System.IO;

using HdknTorrentState = Hadouken.BitTorrent.TorrentState;
using Hadouken.IO;
using System.Threading;
using Hadouken.Configuration;
using MonoTorrent;
using System.Net;
using EncryptionTypes = MonoTorrent.Client.Encryption.EncryptionTypes;
using MtTorrent = MonoTorrent.Common.Torrent;
using Hadouken.Events.BitTorrent;
using Hadouken.Events.Configuration;

namespace Hadouken.Impl.BitTorrent
{
    [Component]
    public class MonoTorrentEngine : IBitTorrentEngine
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IKeyValueStore _keyValueStore;
        private readonly IDataRepository _repository;
        private readonly IFileSystem _fileSystem;

        private readonly ITorrentEventPublisher _torrentPublisher;
        private readonly IConfigurationEventListener _configListener;

        private readonly string _torrentFileSavePath;

        private ClientEngine _clientEngine;
        private readonly Dictionary<string, ITorrentManager> _torrents = new Dictionary<string, ITorrentManager>();

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
            _logger.Info("Loading BitTorrent engine");

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
            _logger.Info("Loading torrent state");

            var infos = _repository.List<TorrentInfo>();

            if(infos == null)
                return;

            foreach (var torrentInfo in infos)
            {
                var manager = (HdknTorrentManager)AddTorrent(torrentInfo.Data, torrentInfo.SavePath);

                if (manager != null)
                {
                    manager.DownloadedBytes = torrentInfo.DownloadedBytes;
                    manager.UploadedBytes = torrentInfo.UploadedBytes;
                    manager.Label = torrentInfo.Label;
                    manager.StartTime = torrentInfo.StartTime;
                    manager.CompletedTime = torrentInfo.CompletedTime;

                    // Load settings
                    manager.Settings.ConnectionRetentionFactor = torrentInfo.ConnectionRetentionFactor;
                    manager.Settings.EnablePeerExchange = torrentInfo.EnablePeerExchange;
                    manager.Settings.InitialSeedingEnabled = torrentInfo.InitialSeedingEnabled;
                    manager.Settings.MaxConnections = torrentInfo.MaxConnections;
                    manager.Settings.MaxDownloadSpeed = torrentInfo.MaxDownloadSpeed;
                    manager.Settings.MaxUploadSpeed = torrentInfo.MaxUploadSpeed;
                    manager.Settings.MinimumTimeBetweenReviews = torrentInfo.MinimumTimeBetweenReviews;
                    manager.Settings.PercentOfMaxRateToSkipReview = torrentInfo.PercentOfMaxRateToSkipReview;
                    manager.Settings.UploadSlots = torrentInfo.UploadSlots;
                    manager.Settings.UseDht = torrentInfo.UseDht;

                    _logger.Debug("Loading FastResume data for torrent {0}", manager.Torrent.Name);

                    if (torrentInfo.FastResumeData != null)
                        manager.LoadFastResume(torrentInfo.FastResumeData);

                    BringToState(manager, torrentInfo.State);
                }
            }
        }

        private void SaveState()
        {
            var infoList = _repository.List<TorrentInfo>();

            foreach (var m in _torrents.Values)
            {
                HdknTorrentManager manager = (HdknTorrentManager)m;
                TorrentInfo info = infoList.SingleOrDefault(i => i.InfoHash == manager.InfoHash);

                if (info == null)
                    info = new TorrentInfo();

                _logger.Debug("Saving state for torrent {0}", manager.Torrent.Name);

                CreateTorrentInfo(manager, info);

                _repository.SaveOrUpdate(info);
            }
        }

        private void BringToState(HdknTorrentManager manager, HdknTorrentState state)
        {
            switch (state)
            {
                case HdknTorrentState.Downloading:
                case HdknTorrentState.Seeding:
                    manager.Start();
                    break;

                case HdknTorrentState.Hashing:
                    manager.HashCheck(false);
                    break;

                case HdknTorrentState.Paused:
                    manager.Start();
                    manager.Pause();
                    break;

                case HdknTorrentState.Stopped:
                    manager.Stop();
                    break;
            }
        }

        public void Unload()
        {
            SaveState();

            _clientEngine.StopAll();
            _clientEngine.Dispose();
        }

        public IDictionary<string, ITorrentManager> Managers
        {
            get { return (IDictionary<string, ITorrentManager>)_torrents; }
        }

        private void CreateTorrentInfo(HdknTorrentManager manager, TorrentInfo info)
        {
            info.Data = manager.TorrentData;
            info.DownloadedBytes = manager.DownloadedBytes;

            if (manager.HashChecked)
            {
                info.FastResumeData = manager.SaveFastResume();
            }
            else
            {
                info.FastResumeData = null;
            }
            
            info.InfoHash = manager.InfoHash;
            info.Label = manager.Label;
            info.SavePath = manager.SavePath;
            info.StartTime = manager.StartTime;
            info.CompletedTime = manager.CompletedTime;

            // to prevent nesting directories
            if (manager.Torrent.Files.Length > 1)
                info.SavePath = Directory.GetParent(manager.SavePath).FullName;

            info.State = manager.State;
            info.UploadedBytes = manager.UploadedBytes;

            // save torrent settings
            info.ConnectionRetentionFactor = manager.Settings.ConnectionRetentionFactor;
            info.EnablePeerExchange = manager.Settings.EnablePeerExchange;
            info.InitialSeedingEnabled = manager.Settings.InitialSeedingEnabled;
            info.MaxConnections = manager.Settings.MaxConnections;
            info.MaxDownloadSpeed = manager.Settings.MaxDownloadSpeed;
            info.MaxUploadSpeed = manager.Settings.MaxUploadSpeed;
            info.MinimumTimeBetweenReviews = manager.Settings.MinimumTimeBetweenReviews;
            info.PercentOfMaxRateToSkipReview = manager.Settings.PercentOfMaxRateToSkipReview;

            info.UploadSlots = manager.Settings.UploadSlots;
            info.UseDht = manager.Settings.UseDht;
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

            MtTorrent t = null;

            if (MtTorrent.TryLoad(data, out t))
            {
                if (String.IsNullOrEmpty(savePath))
                    savePath = _clientEngine.Settings.SavePath;

                return RegisterTorrentManager(new TorrentManager(t, savePath, new TorrentSettings()), data); ;
            }

            return null;
        }

        private HdknTorrentManager RegisterTorrentManager(TorrentManager manager, byte[] data = null)
        {
            // register with engine
            if (_clientEngine.Contains(manager))
                return null;

            _clientEngine.Register(manager);

            // add to dictionary
            var hdknManager = new HdknTorrentManager(manager, _keyValueStore, _fileSystem, _torrentPublisher) { TorrentData = data };
            hdknManager.Load();

            _torrents.Add(hdknManager.InfoHash, hdknManager);

            _torrentPublisher.PublishTorrentAdded(hdknManager);

            // Save state whenever adding torrents.
            SaveState();

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
            }
        }
    }
}
