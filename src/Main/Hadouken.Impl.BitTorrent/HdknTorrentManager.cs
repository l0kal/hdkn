using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hadouken.BitTorrent;
using Hadouken.Configuration;
using MonoTorrent.Client;
using System.IO;
using MonoTorrent.BEncoding;
using Hadouken.IO;
using System.Threading;
using Hadouken.Events.BitTorrent;

namespace Hadouken.Impl.BitTorrent
{
    public class HdknTorrentManager : ITorrentManager
    {
        private readonly object _lock = new object();
        private readonly object _peersLock = new object();

        private readonly TorrentManager _manager;

        private readonly HdknTorrent _torrent;
        private readonly List<IPeer> _peers = new List<IPeer>();
        private readonly List<ITracker> _trackers = new List<ITracker>();
        private ITorrentSettings _settings;

        private readonly IKeyValueStore _keyValueStore;
        private readonly IFileSystem _fileSystem;
        private readonly ITorrentEventPublisher _eventPublisher;

        private long _dlBytes;
        private long _ulBytes;
        private int _initialHashFails;
        private DateTime _addedTime;
        private DateTime _startTime;
        private DateTime? _completedTime;

        private double _progress;

        internal HdknTorrentManager(TorrentManager manager, IKeyValueStore kvs, IFileSystem fileSystem, ITorrentEventPublisher eventPublisher)
        {
            _manager = manager;
            _settings = new HdknTorrentSettings(manager.Settings);
            _torrent = new HdknTorrent(manager.Torrent);

            foreach (var t in _manager.TrackerManager.TrackerTiers)
            {
                foreach (var t2 in t.GetTrackers())
                {
                    _trackers.Add(new HdknTracker(t2));
                }
            }

            _keyValueStore = kvs;
            _fileSystem = fileSystem;
            _eventPublisher = eventPublisher;
            _startTime = DateTime.Now;
        }

        internal TorrentManager Manager { get { return _manager; } }

        internal DateTime AddedTime
        {
            get { return _addedTime; }
            set { _addedTime = value; }
        }

        internal void Load()
        {
            _manager.PeerConnected += PeerConnected;
            _manager.PeerDisconnected += PeerDisconnected;
            _manager.PieceHashed += PieceHashed;
            _manager.TorrentStateChanged += TorrentStateChanged;
        }

        internal void Unload()
        {
            _manager.PeerConnected -= PeerConnected;
            _manager.PeerDisconnected -= PeerDisconnected;
            _manager.PieceHashed -= PieceHashed;
            _manager.TorrentStateChanged -= TorrentStateChanged;
        }

        private void PeerConnected(object sender, PeerConnectionEventArgs e)
        {
            HdknPeer p = new HdknPeer(e.PeerID);

            lock (_peersLock)
            {
                _peers.Add(p);
            }
        }

        private void PeerDisconnected(object sender, PeerConnectionEventArgs e)
        {
            lock (_peersLock)
            {
                var p = (HdknPeer)_peers.FirstOrDefault(i => i.PeerId == e.PeerID.PeerID);

                if (p != null)
                    _peers.Remove(p);
            }
        }

        private void PieceHashed(object sender, PieceHashedEventArgs e)
        {
            int pieceIndex = e.PieceIndex;
            int totalPieces = e.TorrentManager.Torrent.Pieces.Count;

            lock (_lock)
            {
                _progress = (double)pieceIndex / totalPieces * 100.0;
            }
        }

        private void TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            if (e.NewState == MonoTorrent.Common.TorrentState.Error)
            {
                _eventPublisher.PublishTorrentError(this);
            }

            if (e.OldState == MonoTorrent.Common.TorrentState.Downloading && e.NewState == MonoTorrent.Common.TorrentState.Seeding)
            {
                if (_completedTime == null)
                    _completedTime = DateTime.Now;

                var oldSavePath = String.Copy(SavePath);
                BasicMove(oldSavePath);

                _eventPublisher.PublishTorrentCompleted(this);
            }
        }

        private void BasicMove(string oldSavePath)
        {
            if (!oldSavePath.Equals(SavePath))
                return;

            var moveCompleted =
                _keyValueStore.Get<bool>(
                    "paths.shouldMoveCompleted");

            var newPath =
                _keyValueStore.Get<string>(
                    "paths.completedPath");

            if (!moveCompleted || String.IsNullOrEmpty(newPath)) return;

            var appendLabel = _keyValueStore.Get<bool>("paths.appendLabelOnMoveCompleted");

            if (appendLabel && !String.IsNullOrEmpty(Label))
                newPath = Path.Combine(newPath, Label);

            Move(newPath);
        }

        public IBitField BitField
        {
            get { return new HdknBitField(_manager.Bitfield); }
        }

        public bool CanUseDht
        {
            get { return _manager.CanUseDht; }
        }

        public bool Complete
        {
            get { return CalculateCompleted(); }
        }

        private bool CalculateCompleted()
        {
            if (_manager.Complete)
                return true;

            double progress = 0;
            int count = 0;
            foreach (var file in Torrent.Files)
            {
                if (file.Priority != Priority.DoNotDownload)
                {
                    progress += file.BitField.PercentComplete; // This is 0 -> 100.
                    count++;
                }
            }
            progress /= count; 

            return (progress == 100.0);
        }

        public bool HashChecked
        {
            get { return _manager.HashChecked; }
        }

        public int HashFails
        {
            get { return _manager.HashFails + _initialHashFails; }
        }

        public bool HasMetadata
        {
            get { return _manager.HasMetadata; }
        }

        public List<Uri> InactivePeerList
        {
            get { return _manager.InactivePeerList; }
        }

        public int InactivePeers
        {
            get { return _manager.InactivePeers; }
        }

        public string InfoHash
        {
            get { return _manager.InfoHash.ToString().Replace("-", ""); }
        }

        public bool IsInEndGame
        {
            get { return _manager.IsInEndGame; }
        }

        public bool IsInitialSeeding
        {
            get { return _manager.IsInitialSeeding; }
        }

        public int OpenConnections
        {
            get { return _manager.OpenConnections; }
        }

        public int PeerReviewRoundsComplete
        {
            get { return _manager.PeerReviewRoundsComplete; }
        }

        public double Progress
        {
            get 
            {
                if (State == TorrentState.Hashing)
                    return _progress;

                return _manager.Progress;
            }
        }

        public string SavePath
        {
            get { return _manager.SavePath; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            internal set { _startTime = value; }
        }

        public DateTime? CompletedTime
        {
            get { return _completedTime; }
            internal set { _completedTime = value; }
        }

        public ITorrentSettings Settings
        {
            get { return _settings; }
        }

        public TorrentState State
        {
            get { return (TorrentState)(int)_manager.State; }
        }

        public int UploadingTo
        {
            get { return _manager.UploadingTo; }
        }

        public TimeSpan ETA
        {
            get { return CalculateETA(); }
        }

        private TimeSpan CalculateETA()
        {
            if (DownloadSpeed == 0 || Complete)
                return TimeSpan.FromSeconds(-1);

            long downloadedBytes = (long)((double)Torrent.Size * (Progress / 100));

            var s = (Torrent.Size - downloadedBytes) / DownloadSpeed;
            var finishDate = DateTime.Now.AddSeconds(s);

            return finishDate - DateTime.Now;
        }

        public string Label { get; set; }

        public long DownloadedBytes
        {
            get { return _manager.Monitor.DataBytesDownloaded + _dlBytes; }
            internal set { _dlBytes = value; }
        }

        public long RemainingBytes
        {
            get { return CalculateRemaning(); }
        }

        private long CalculateRemaning()
        {
            var files = _manager.Torrent.Files;

            var total = files.Where(f => f.Priority != MonoTorrent.Common.Priority.DoNotDownload).Sum(f => f.Length);
            var dled = files.Where(f => f.Priority != MonoTorrent.Common.Priority.DoNotDownload).Sum(f => f.BytesDownloaded);

            return total - dled;
        }

        public long UploadedBytes
        {
            get { return _manager.Monitor.DataBytesUploaded + _ulBytes; }
            internal set { _ulBytes = value; }
        }

        public long DownloadSpeed
        {
            get { return _manager.Monitor.DownloadSpeed; }
        }

        public long UploadSpeed
        {
            get { return _manager.Monitor.UploadSpeed; }
        }

        public ITorrent Torrent
        {
            get { return _torrent; }
        }

        public byte[] TorrentData { get; internal set; }

        public IPeer[] Peers
        {
            get { return _peers.ToArray(); }
        }

        public ITracker[] Trackers
        {
            get { return _trackers.ToArray(); }
        }

        public void Start()
        {
            _manager.Start();
        }

        public void Stop()
        {
            _manager.Stop();
        }

        public void Pause()
        {
            _manager.Pause();
        }

        public void Move(string newLocation)
        {
            /* Move torrent
             * Examples:
             * C:\Downloads\movie.mkv - newLocation=D:\Movies - D:\Movies\movie.mkv
             * C:\Downloads\Folder    - newLocation=D:\Misc   - D:\Misc\Folder
             * 
            */

            var isRunning = (State == TorrentState.Seeding || State == TorrentState.Downloading ||
                             State == TorrentState.Metadata || State == TorrentState.Paused ||
                             State == TorrentState.Hashing);

            if (isRunning)
                Stop();

            while(State != TorrentState.Stopped)
                Thread.Sleep(100);

            var oldLocation = SavePath;

            if (Torrent.Files.Length == 1)
                oldLocation = Path.Combine(oldLocation, Torrent.Name);

            var isDirectory = File.GetAttributes(oldLocation).HasFlag(FileAttributes.Directory);

            if (isDirectory)
            {
                newLocation = Path.Combine(newLocation, Torrent.Name);
                DuplicateStructure(oldLocation, newLocation);
            }

            if(!_fileSystem.DirectoryExists(newLocation))
                _fileSystem.CreateDirectory(newLocation);

            _manager.MoveFiles(newLocation, true);

            if (isDirectory)
            {
                _fileSystem.EmptyDirectory(oldLocation);
                _fileSystem.DeleteDirectory(oldLocation);
            }

            if (isRunning)
                Start();

            _eventPublisher.PublishTorrentMoved(this);
        }

        private void DuplicateStructure(string source, string target)
        {
            if (!_fileSystem.DirectoryExists(target))
                _fileSystem.CreateDirectory(target);

            foreach (string subDirectory in _fileSystem.GetDirectories(source))
            {
                var name = new DirectoryInfo(subDirectory).Name;
                DuplicateStructure(Path.Combine(source, name), Path.Combine(target, name));
            }
        }

        public void HashCheck(bool autoStart)
        {
            if (State == TorrentState.Stopped)
                _manager.HashCheck(autoStart);
        }

        public byte[] SaveFastResume()
        {
            if (!HashChecked)
                HashCheck(false);

            using (var ms = new MemoryStream())
            {
                _manager.SaveFastResume().Encode(ms);
                return ms.ToArray();
            }
        }

        public void LoadFastResume(byte[] data)
        {
            var d = BEncodedValue.Decode<BEncodedDictionary>(data);
            var f = new FastResume(d);

            _manager.LoadFastResume(f);
        }

        internal BEncodedDictionary GetStateData()
        {
            var d = new BEncodedDictionary();

            d["added_on"] = new BEncodedNumber(-1);

            if (CompletedTime.HasValue)
                d["completed_on"] = new BEncodedNumber(CompletedTime.Value.ToUnixTime());

            d["downloaded"] = new BEncodedNumber(DownloadedBytes);

            if (_manager.HashChecked)
                d["fast_resume"] = _manager.SaveFastResume().Encode();

            d["hashfails"] = new BEncodedNumber(HashFails);
            // have
            d["label"] = new BEncodedString(Label ?? "");
            d["max_connections"] = new BEncodedNumber(_manager.Settings.MaxConnections);
            d["moved"] = new BEncodedNumber(0);
            d["state"] = new BEncodedNumber((long)_manager.State);

            if (_manager.Torrent.Files.Length > 1)
            {
                d["path"] = new BEncodedString(Directory.GetParent(_manager.SavePath).FullName);
            }
            else
            {
                d["path"] = new BEncodedString(_manager.SavePath);
            }
            

            d["seedtime"] = new BEncodedNumber(0);
            d["superseed"] = new BEncodedNumber(_manager.Settings.InitialSeedingEnabled ? 1 : 0);

            var tiers = new BEncodedList();

            foreach (var tier in _manager.TrackerManager.TrackerTiers)
            {
                tiers.Add(new BEncodedList(tier.Select(t => new BEncodedString(t.Uri.ToString()))));
            }

            d["trackers"] = tiers;
            d["ulslots"] = new BEncodedNumber(_manager.Settings.UploadSlots);
            d["uploaded"] = new BEncodedNumber(UploadedBytes);

            return d;
        }

        internal void SetStateData(BEncodedDictionary d)
        {
            _addedTime = FromUnixTime(((BEncodedNumber) d["added_on"]).Number);

            if (d.ContainsKey("completed_on"))
                _completedTime = FromUnixTime(((BEncodedNumber) d["completed_on"]).Number);

            _dlBytes = ((BEncodedNumber)d["downloaded"]).Number;
            _ulBytes = ((BEncodedNumber)d["uploaded"]).Number;

            if (d.ContainsKey("fast_resume"))
                _manager.LoadFastResume(new FastResume(d["fast_resume"] as BEncodedDictionary));

            _initialHashFails = (int)((BEncodedNumber) d["hashfails"]).Number;
            Label = ((BEncodedString) d["label"]).Text;

            _manager.Settings.MaxConnections = Convert.ToInt32(((BEncodedNumber) d["max_connections"]).Number);
            _manager.Settings.InitialSeedingEnabled = (((BEncodedNumber) d["superseed"]).Number == 1);
            _manager.Settings.UploadSlots = (int)((BEncodedNumber) d["ulslots"]).Number;

            // Parse tracker and tiers

            // Bring torrent to state
            var state = (MonoTorrent.Common.TorrentState)(int)((BEncodedNumber)d["state"]).Number;

            switch (state)
            {
                case MonoTorrent.Common.TorrentState.Downloading:
                case MonoTorrent.Common.TorrentState.Seeding:
                    Start();
                    break;

                case MonoTorrent.Common.TorrentState.Paused:
                    Start();
                    Pause();
                    break;

                case MonoTorrent.Common.TorrentState.Stopped:
                case MonoTorrent.Common.TorrentState.Stopping:
                    Stop();
                    break;
            }
        }

        private static DateTime FromUnixTime(long ticks)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ticks).ToLocalTime();
        }
    }
}
