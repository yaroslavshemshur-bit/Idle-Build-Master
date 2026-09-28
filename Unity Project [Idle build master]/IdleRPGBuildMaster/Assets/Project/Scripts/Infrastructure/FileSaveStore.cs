using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IBM.Application;

namespace IBM.Infrastructure
{
    // Immutable generation files avoid overwriting the current valid save during publication.
    public sealed class FileSaveStore : ISaveStore
    {
        private readonly string _directory;
        public FileSaveStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Save directory is required.", nameof(directory));
            _directory = directory;
        }

        public IReadOnlyList<byte[]> ReadCandidatesNewestFirst()
        {
            if (!Directory.Exists(_directory)) return Array.Empty<byte[]>();
            var candidates = new List<byte[]>();
            foreach (var file in Generations().OrderByDescending(x => x.generation))
            {
                try { candidates.Add(File.ReadAllBytes(file.path)); }
                catch (IOException) { /* A corrupt/unreadable generation is skipped; an older one may survive. */ }
            }
            return candidates;
        }

        public void Commit(byte[] snapshotBytes)
        {
            if (snapshotBytes == null || snapshotBytes.Length == 0) throw new ArgumentException("Snapshot is empty.", nameof(snapshotBytes));
            Directory.CreateDirectory(_directory);
            var existing = Generations();
            long generation = existing.Count == 0 ? 1 : checked(existing.Max(x => x.generation) + 1);
            string finalPath = Path.Combine(_directory, "save-" + generation.ToString("D20") + ".bin");
            string temporary = finalPath + ".tmp-" + Guid.NewGuid().ToString("N");
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(snapshotBytes, 0, snapshotBytes.Length);
                stream.Flush(true);
            }
            if (!File.ReadAllBytes(temporary).SequenceEqual(snapshotBytes)) throw new IOException("Save candidate verification failed.");
            File.Move(temporary, finalPath);
            if (!File.ReadAllBytes(finalPath).SequenceEqual(snapshotBytes)) throw new IOException("Published save verification failed.");
            // Retain two previous generations as recovery evidence.
            foreach (var old in Generations().OrderByDescending(x => x.generation).Skip(3))
                File.Delete(old.path);
        }

        private List<(long generation, string path)> Generations()
        {
            var result = new List<(long generation, string path)>();
            if (!Directory.Exists(_directory)) return result;
            foreach (var path in Directory.GetFiles(_directory, "save-*.bin", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("save-", StringComparison.Ordinal) && long.TryParse(name.Substring(5), out long generation) && generation > 0)
                    result.Add((generation, path));
            }
            return result;
        }
    }
}
