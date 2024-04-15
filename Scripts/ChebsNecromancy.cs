using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.MagicAndEffects;
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
            RegisterSpells();

            mod.IsReady = true;
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            RecallMinionsEffect.CostA = mod.GetSettings().GetValue<int>("Recall Minions", "Chance Cost A");
            RecallMinionsEffect.CostB = mod.GetSettings().GetValue<int>("Recall Minions", "Chance Cost B");
            RecallMinionsEffect.CostOffset = mod.GetSettings().GetValue<int>("Recall Minions", "Chance Cost Offset");

            SummonAncientLichEffect.CostA = mod.GetSettings().GetValue<int>("Summon Ancient Lich", "Chance Cost A");
            SummonAncientLichEffect.CostB = mod.GetSettings().GetValue<int>("Summon Ancient Lich", "Chance Cost B");
            SummonAncientLichEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Ancient Lich", "Chance Cost Offset");

            SummonAncientVampireEffect.CostA = mod.GetSettings().GetValue<int>("Summon Ancient Vampire", "Chance Cost A");
            SummonAncientVampireEffect.CostB = mod.GetSettings().GetValue<int>("Summon Ancient Vampire", "Chance Cost B");
            SummonAncientVampireEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Ancient Vampire", "Chance Cost Offset");

            SummonGhostEffect.CostA = mod.GetSettings().GetValue<int>("Summon Ghost", "Chance Cost A");
            SummonGhostEffect.CostB = mod.GetSettings().GetValue<int>("Summon Ghost", "Chance Cost B");
            SummonGhostEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Ghost", "Chance Cost Offset");

            SummonLichEffect.CostA = mod.GetSettings().GetValue<int>("Summon Lich", "Chance Cost A");
            SummonLichEffect.CostB = mod.GetSettings().GetValue<int>("Summon Lich", "Chance Cost B");
            SummonLichEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Lich", "Chance Cost Offset");

            SummonMummyEffect.CostA = mod.GetSettings().GetValue<int>("Summon Mummy", "Chance Cost A");
            SummonMummyEffect.CostB = mod.GetSettings().GetValue<int>("Summon Mummy", "Chance Cost B");
            SummonMummyEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Mummy", "Chance Cost Offset");

            SummonSkeletonEffect.CostA = mod.GetSettings().GetValue<int>("Summon Skeleton", "Chance Cost A");
            SummonSkeletonEffect.CostB = mod.GetSettings().GetValue<int>("Summon Skeleton", "Chance Cost B");
            SummonSkeletonEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Skeleton", "Chance Cost Offset");

            SummonVampireEffect.CostA = mod.GetSettings().GetValue<int>("Summon Vampire", "Chance Cost A");
            SummonVampireEffect.CostB = mod.GetSettings().GetValue<int>("Summon Vampire", "Chance Cost B");
            SummonVampireEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Vampire", "Chance Cost Offset");

            SummonZombieEffect.CostA = mod.GetSettings().GetValue<int>("Summon Zombie", "Chance Cost A");
            SummonZombieEffect.CostB = mod.GetSettings().GetValue<int>("Summon Zombie", "Chance Cost B");
            SummonZombieEffect.CostOffset = mod.GetSettings().GetValue<int>("Summon Zombie", "Chance Cost Offset");

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

        private static void RegisterSpells()
        {
            var effectBroker = GameManager.Instance.EntityEffectBroker;

            var spellEffects = new List<BaseEntityEffect>()
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

            foreach (var baseEntityEffect in spellEffects)
            {
                if (effectBroker.HasEffectTemplate(baseEntityEffect.Key))
                {
                    var existingTemplate = effectBroker.GetEffectTemplate(baseEntityEffect.Key);
                    existingTemplate.Settings = baseEntityEffect.Settings;
                }
                else
                {
                    effectBroker.RegisterEffectTemplate(baseEntityEffect);
                }
            }

        }
    }
}
