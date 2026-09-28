using System;
using System.IO;
using System.Security.Cryptography;
using IBM.Application;
using IBM.Authoring;
using IBM.Domain;
using IBM.Infrastructure;
using UnityEngine;

namespace IBM.Presentation
{
    // Unity owns lifecycle and presentation references; the session owns gameplay state.
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private GameContentAsset contentAsset;
        [SerializeField] private BalanceTuningAsset balanceAsset;
        [SerializeField] private InterfaceTuningAsset interfaceAsset;

        private SaveCoordinator _saves;
        public GameSession Session { get; private set; }
        public ContentCatalog Content { get; private set; }
        public BalanceCatalog Balance { get; private set; }
        public InterfaceTuningAsset Interface => interfaceAsset;
        public event Action<SessionView> ViewChanged;

        private void Awake()
        {
            if (contentAsset == null || balanceAsset == null || interfaceAsset == null)
                throw new InvalidOperationException("GameRoot requires content, balance and interface ScriptableObjects.");
            Balance = balanceAsset.Compile();
            interfaceAsset.Validate();
            Content = contentAsset.Compile(balance: Balance);
            var firstStage = Content.StartingStageId;
            var versions = new VersionStamp(GameSnapshotDto.CurrentSchemaVersion, 1, 1, Pcg32.AlgorithmVersion,
                contentAsset.contentVersion);
            var store = new FileSaveStore(Path.Combine(UnityEngine.Application.persistentDataPath, "saves"));
            _saves = new SaveCoordinator(new UnityJsonSaveCodec(), store, Content, versions);
            var state = store.ReadCandidatesNewestFirst().Count == 0
                ? NewGame(firstStage, versions)
                : _saves.RestoreOrThrow();
            Session = new GameSession(state, Content);
            DontDestroyOnLoad(gameObject);
            ViewChanged?.Invoke(Session.ReadView());
        }

        public CommandResult Execute(GameCommand command)
        {
            if (Session == null) throw new InvalidOperationException("GameRoot is not initialized.");
            var result = Session.Execute(command);
            if (result.Status == CommandStatus.Applied) ViewChanged?.Invoke(Session.ReadView());
            return result;
        }

        public void Save() => _saves?.Save(Session);

        private void OnApplicationPause(bool paused)
        {
            if (paused && Session != null) Save();
        }

        private void OnApplicationQuit()
        {
            if (Session != null) Save();
        }

        private static GameState NewGame(ContentId firstStage, VersionStamp versions)
        {
            var bytes = new byte[8];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            ulong seed = BitConverter.ToUInt64(bytes, 0);
            return new GameState(new AccountState(Guid.NewGuid().ToString("N")),
                new RunState(Guid.NewGuid().ToString("N"), firstStage), new CombatState(), versions,
                SessionRandomState.Create(seed), new OfflineAccountingState
                {
                    AccountedThroughUtc = DateTime.UtcNow.ToString("O")
                });
        }
    }
}
