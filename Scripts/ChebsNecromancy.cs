using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
using Wenzil.Console;

namespace ChebsNecromancyMod
{
    public class ChebsNecromancy : MonoBehaviour
    {
        private static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<ChebsNecromancy>();

            ConsoleCommandsDatabase.RegisterCommand(
                SpawnCommand.name, SpawnCommand.description,
                SpawnCommand.usage, SpawnCommand.Execute);

            ConsoleCommandsDatabase.RegisterCommand(
                RecallMinionsCommand.name, RecallMinionsCommand.description,
                RecallMinionsCommand.usage, RecallMinionsCommand.Execute);

            mod.LoadSettingsCallback = LoadSettings;

            SaveLoadManager.OnLoad += RegisterExistingMinions;

            mod.LoadSettings();

            mod.IsReady = true;
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            var spellEffects = new List<ChebEffect>()
            {
                new RecallMinionsEffect(),
                new SummonSkeletonEffect(),
                new SummonGhostEffect(),
                new SummonLichEffect(),
                new SummonMummyEffect(),
                new SummonVampireEffect(),
                new SummonAncientLichEffect(),
                new SummonAncientVampireEffect(),
                new SummonZombieEffect()
            };

            var effectBroker = GameManager.Instance.EntityEffectBroker;

            foreach (var baseEntityEffect in spellEffects)
            {
                baseEntityEffect.CostA = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost A");
                baseEntityEffect.CostB = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost B");
                baseEntityEffect.CostOffset = modSettings.GetValue<int>(baseEntityEffect.Key, "Chance Cost Offset");

                if (effectBroker.HasEffectTemplate(baseEntityEffect.Key))
                {
                    Debug.Log($"Updating existing {baseEntityEffect.Key}.");
                    var existingTemplate = effectBroker.GetEffectTemplate(baseEntityEffect.Key);
                    existingTemplate.Settings = baseEntityEffect.Settings;
                }
                else
                {
                    Debug.Log($"Registering new {baseEntityEffect.Key}");
                    effectBroker.RegisterEffectTemplate(baseEntityEffect);
                }
            }
        }

        private static void RegisterExistingMinions(SaveData_v1 saveDataV1)
        {
            // When the scene loads, existing skeletons from the last session won't have the UndeadMinion script
            // attached to them. Find them and attach it here, so that they follow the player etc.
            var daggerfallEnemies = FindObjectsOfType<DaggerfallEnemy>();
            foreach (var daggerfallEnemy in daggerfallEnemies)
            {
                if (daggerfallEnemy.MobileUnit.Enemy.Team == MobileTeams.PlayerAlly)
                {
                    daggerfallEnemy.gameObject.AddComponent<UndeadMinion>();
                }
            }
        }

        private void OnDestroy()
        {
            SaveLoadManager.OnLoad -= RegisterExistingMinions;
        }
    }
}
