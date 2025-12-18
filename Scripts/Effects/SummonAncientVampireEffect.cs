using System.Collections.Generic;
using ChebsNecromancyMod.MinionSpawners;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonAncientVampireEffect : ChebEffect
    {
        public const string EffectKey = "Summon Ancient Vampire";
        protected override string effectKey => EffectKey;
        protected override string effectDescription => "Summons an ancient vampire to follow and guard you.";
        protected override List<KeyValuePair<ItemGroups, int>> reagentsItems
        {
            get { return new List<KeyValuePair<ItemGroups, int>>
            {
                new KeyValuePair<ItemGroups, int>(CustomCorpseItem.TemplateItemGroup, (int)CustomCorpseItem.CustomTemplateIndex),
                new KeyValuePair<ItemGroups, int>(ItemGroups.PlantIngredients2, (int)PlantIngredients2.Black_rose)
            }; }
        }

        public override void SetProperties()
        {
            base.SetProperties();

            // if we wanna make stuff harder, we can add durations to the spell etc. but we all know Cheb hates that
            // stuff.
            properties.SupportChance = true;
            properties.SupportDuration = false;
            properties.SupportMagnitude = true;
        }

        public override void LoadModSettings(ModSettings modSettings)
        {
            base.LoadModSettings(modSettings);

            var chanceCostA = modSettings.GetValue<int>(effectKey, "Chance Cost A");
            var chanceCostB = modSettings.GetValue<int>(effectKey, "Chance Cost B");
            var chanceCostOffset = modSettings.GetValue<int>(effectKey, "Chance Cost Offset");
            var chanceCosts = new EffectCosts
            {
                CostA = chanceCostA,
                CostB = chanceCostB,
                OffsetGold = chanceCostOffset
            };

            var magnitudeCostA = modSettings.GetValue<int>(effectKey, "Magnitude Cost A");
            var magnitudeCostB = modSettings.GetValue<int>(effectKey, "Magnitude Cost B");
            var magnitudeCostOffset = modSettings.GetValue<int>(effectKey, "Magnitude Cost Offset");
            var magnitudeCosts = new EffectCosts
            {
                CostA = magnitudeCostA,
                CostB = magnitudeCostB,
                OffsetGold = magnitudeCostOffset
            };

            properties.ChanceCosts = chanceCosts;
            properties.MagnitudeCosts = magnitudeCosts;
        }

        public override bool ChanceSuccess => base.ChanceSuccess && (!ChebsNecromancy.CorpseItemEnabled || HasReagents());

        protected override void DoEffect()
        {
            base.DoEffect();

            Spawn(GetMagnitude(), caster.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Mysticism),
                caster.Entity.Stats.LiveIntelligence, caster.Entity.Stats.LiveWillpower, true);

            ConsumeReagents();
        }

        public static void Spawn(int magnitude, int mysticismLevel, int intelligence, int willpower, bool showHUDMessage)
        {
            var spawner = new GameObject("MinionSpawner");
            spawner.SetActive(false);
            var minionSpawner = spawner.AddComponent<AncientVampireSpawner>();
            minionSpawner.showHUDMessage = showHUDMessage;
            minionSpawner.magnitude = magnitude;
            minionSpawner.mysticismLevel = mysticismLevel;
            minionSpawner.intelligence = intelligence;
            minionSpawner.willpower = willpower;
            spawner.SetActive(true);
        }
    }
}