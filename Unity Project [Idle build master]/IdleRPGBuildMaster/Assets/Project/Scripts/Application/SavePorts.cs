using System;
using System.Collections.Generic;
using IBM.Domain;

namespace IBM.Application
{
    public sealed class IncompatibleSaveException : InvalidOperationException
    {
        public IncompatibleSaveException(string message) : base(message) { }
    }
    public interface ISaveCodec
    {
        byte[] Encode(GameState snapshot, ContentCatalog catalog);
        GameState Decode(byte[] bytes, ContentCatalog catalog, VersionStamp expectedVersions);
    }

    public interface ISaveStore
    {
        IReadOnlyList<byte[]> ReadCandidatesNewestFirst();
        void Commit(byte[] snapshotBytes);
    }

    // A release that changes saved balance or mechanics must explicitly supply one of these.
    public interface ISaveCompatibilityHandler
    {
        bool CanTransform(GameSnapshotDto saved, VersionStamp target, string targetMechanicalHash);
        GameSnapshotDto Transform(GameSnapshotDto saved, VersionStamp target, string targetMechanicalHash);
    }

    public sealed class SaveCoordinator
    {
        private readonly ISaveCodec _codec;
        private readonly ISaveStore _store;
        private readonly ContentCatalog _catalog;
        private readonly VersionStamp _versions;

        public SaveCoordinator(ISaveCodec codec, ISaveStore store, ContentCatalog catalog, VersionStamp versions)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _versions = versions;
        }

        public void Save(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            byte[] bytes = _codec.Encode(session.CaptureAtSafePoint(), _catalog);
            // Decode before writing so unsupported state cannot replace a valid generation.
            _codec.Decode(bytes, _catalog, _versions);
            _store.Commit(bytes);
        }

        public GameState RestoreOrThrow()
        {
            var candidates = _store.ReadCandidatesNewestFirst();
            if (candidates.Count == 0) throw new InvalidOperationException("No save generation exists.");
            var failures = new List<string>();
            foreach (var candidate in candidates)
            {
                try { return _codec.Decode(candidate, _catalog, _versions); }
                catch (Exception error) when (!(error is IncompatibleSaveException) &&
                    (error is ArgumentException || error is InvalidOperationException || error is FormatException || error is OverflowException))
                { failures.Add(error.Message); }
            }
            throw new InvalidOperationException("No compatible valid save generation: " + string.Join(" | ", failures));
        }
    }
}
