namespace Hadouken.Engines.BitTorrent
{
    public abstract class BitTorrentTransferManager : ItemTransferManager
    {
        public abstract long UploadSpeed { get; }
        public abstract long UploadedBytes { get; }

        public abstract void HashCheck(bool autoStart);

        public abstract void LoadFastResume(byte[] data);

        public abstract byte[] SaveFastResume();
    }
}
