using System;
using System.Security.Cryptography;
using System.Text;
using IBM.Application;
using IBM.Domain;
using UnityEngine;

namespace IBM.Infrastructure
{
    [Serializable]
    internal sealed class SaveEnvelopeDto
    {
        public string formatId;
        public int schemaVersion;
        public string payloadChecksum;
        public string payloadJson;
    }

    public sealed class UnityJsonSaveCodec : ISaveCodec
    {
        private const string FormatId = "IBM.LocalSave.JSON.v1";
        private const int MaxPayloadBytes = 16 * 1024 * 1024;
        private readonly ISaveCompatibilityHandler _compatibility;

        public UnityJsonSaveCodec(ISaveCompatibilityHandler compatibility = null) => _compatibility = compatibility;

        public byte[] Encode(GameState snapshot, ContentCatalog catalog)
        {
            var dto = GameSnapshotDto.FromState(snapshot, catalog.MechanicalHash);
            string payload = JsonUtility.ToJson(dto);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            if (payloadBytes.Length > MaxPayloadBytes) throw new InvalidOperationException("Save payload exceeds size limit.");
            var envelope = new SaveEnvelopeDto { formatId = FormatId, schemaVersion = dto.schemaVersion,
                payloadChecksum = Sha256(payloadBytes), payloadJson = payload };
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(envelope));
        }

        public GameState Decode(byte[] bytes, ContentCatalog catalog, VersionStamp expectedVersions)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaxPayloadBytes * 2)
                throw new InvalidOperationException("Save envelope is empty or too large.");
            var envelope = JsonUtility.FromJson<SaveEnvelopeDto>(Encoding.UTF8.GetString(bytes));
            if (envelope == null || envelope.formatId != FormatId || string.IsNullOrEmpty(envelope.payloadJson))
                throw new InvalidOperationException("Unsupported save envelope.");
            byte[] payloadBytes = Encoding.UTF8.GetBytes(envelope.payloadJson);
            if (payloadBytes.Length > MaxPayloadBytes || !StringComparer.OrdinalIgnoreCase.Equals(Sha256(payloadBytes), envelope.payloadChecksum))
                throw new InvalidOperationException("Save payload checksum failed.");
            var dto = JsonUtility.FromJson<GameSnapshotDto>(envelope.payloadJson);
            if (dto == null || dto.schemaVersion != envelope.schemaVersion) throw new InvalidOperationException("Save schema header mismatch.");
            bool needsTransform = dto.schemaVersion != expectedVersions.Schema || dto.rulesVersion != expectedVersions.Rules ||
                dto.numericVersion != expectedVersions.Numeric || dto.rngVersion != expectedVersions.Rng ||
                dto.contentVersion != expectedVersions.Content || dto.mechanicalHash != catalog.MechanicalHash;
            if (needsTransform)
            {
                if (_compatibility == null || !_compatibility.CanTransform(dto, expectedVersions, catalog.MechanicalHash))
                    throw new IncompatibleSaveException("Save requires a content/rules/schema migration before activation.");
                dto = _compatibility.Transform(dto, expectedVersions, catalog.MechanicalHash);
            }
            return dto.ToState(catalog, expectedVersions);
        }

        private static string Sha256(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
