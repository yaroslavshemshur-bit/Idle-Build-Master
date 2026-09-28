using IBM.Authoring;
using UnityEditor;
using UnityEngine;

namespace IBM.Editor
{
    public static class TuningAssetCreator
    {
        private const string Folder = "Assets/Project/Content/Authoring/Tuning";

        // Safe to run again: existing authored profiles are never replaced.
        [MenuItem("Idle Build Master/Tuning/Create Missing Profiles")]
        public static void CreateMissing()
        {
            EnsureFolder("Assets/Project", "Content");
            EnsureFolder("Assets/Project/Content", "Authoring");
            EnsureFolder("Assets/Project/Content/Authoring", "Tuning");
            Ensure<BalanceTuningAsset>(Folder + "/BalanceTuning.asset");
            Ensure<InterfaceTuningAsset>(Folder + "/InterfaceTuning.asset");
            Ensure<GameContentAsset>("Assets/Project/Content/Authoring/GameContent.asset");
            EnsureOfflineBaseline();
            EnsureCombatBaseline();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
        }

        private static void Ensure<T>(string path) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return;
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                throw new System.InvalidOperationException("Unexpected asset at " + path);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
        }

        private static void EnsureOfflineBaseline()
        {
            var asset = AssetDatabase.LoadAssetAtPath<BalanceTuningAsset>(Folder + "/BalanceTuning.asset");
            var serialized = new SerializedObject(asset);
            var entries = serialized.FindProperty("parameters");
            AddIfMissing(entries, "offline.max_seconds", IBM.Domain.BalanceValueKind.DurationSeconds, "21600", "0");
            AddIfMissing(entries, "offline.base_efficiency", IBM.Domain.BalanceValueKind.Probability, "5", "-1");
            AddIfMissing(entries, "offline.min_encounter_seconds", IBM.Domain.BalanceValueKind.DurationSeconds, "60", "0");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureCombatBaseline()
        {
            var asset = AssetDatabase.LoadAssetAtPath<BalanceTuningAsset>(Folder + "/BalanceTuning.asset");
            var serialized = new SerializedObject(asset);
            var entries = serialized.FindProperty("parameters");
            AddNumber(entries, "combat.min_damage.base", "5", "0");
            AddNumber(entries, "combat.min_damage.dex_factor", "5", "-1");
            AddNumber(entries, "combat.max_damage.str_factor", "2", "0");
            AddNumber(entries, "combat.max_hp.base", "20", "0");
            AddNumber(entries, "combat.max_hp.vit_factor", "100", "0");
            AddNumber(entries, "combat.block.str_factor", "1", "-1");
            AddNumber(entries, "combat.block.vit_factor", "5", "-1");
            AddNumber(entries, "combat.block.reference", "100", "0");
            AddNumber(entries, "combat.accuracy.base", "100", "0");
            AddNumber(entries, "combat.evasion.base", "100", "0");
            AddNumber(entries, "combat.attack_speed.base_rating", "1", "0");
            AddNumber(entries, "combat.attack_speed.agi_factor", "1", "-2");
            AddNumber(entries, "combat.attack_speed.rating_scale", "2", "0");
            AddNumber(entries, "combat.regen.base", "1", "0");
            AddNumber(entries, "combat.regen.vit_factor", "1", "-1");
            AddNumber(entries, "combat.hit.min", "5", "-2");
            AddNumber(entries, "combat.hit.max", "95", "-2");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddNumber(SerializedProperty entries, string id, string coefficient, string exponent) =>
            AddIfMissing(entries, id, IBM.Domain.BalanceValueKind.Number, coefficient, exponent);

        private static void AddIfMissing(SerializedProperty entries, string id, IBM.Domain.BalanceValueKind kind, string coefficient, string exponent)
        {
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue == id) return;
            int index = entries.arraySize;
            entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("id").stringValue = id;
            entry.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            entry.FindPropertyRelative("coefficient").stringValue = coefficient;
            entry.FindPropertyRelative("exponent").stringValue = exponent;
        }
    }
}
