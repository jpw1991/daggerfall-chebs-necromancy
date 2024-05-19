using ChebsNecromancyMod.MinionSpawners;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonAncientVampireEffect : ChebEffect
    {
        protected override string effectKey => "Summon Ancient Vampire";
        protected override string effectDescription => "Summons an ancient vampire to follow and guard you.";

        public new int ChanceCostA { get; set; }
        public new int ChanceCostB { get; set; }
        public new int ChanceCostOffset { get; set; }
        public new int MagnitudeCostA { get; set; }
        public new int MagnitudeCostB { get; set; }
        public new int MagnitudeCostOffset { get; set; }

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
            properties.DisableReflectiveEnumeration = true;

            // if we wanna make stuff harder, we can add durations to the spell etc. but we all know Cheb hates that
            // stuff.
            properties.SupportChance = true;
            properties.ChanceCosts = MakeEffectCosts(ChanceCostA, ChanceCostB, ChanceCostOffset);
            properties.SupportDuration = false;
            // properties.DurationCosts = MakeEffectCosts(8, 100, 200);
            properties.SupportMagnitude = true;
            properties.MagnitudeCosts = MakeEffectCosts(MagnitudeCostA, MagnitudeCostB, MagnitudeCostOffset);
        }

        public override bool ChanceSuccess => base.ChanceSuccess && (!ChebsNecromancy.CorpseItemEnabled || HasReagents());

        protected bool HasReagents()
        {
            if (caster == null)
            {
                ChebsNecromancy.ChebError("HasReagents: caster is null");
                return false;
            }

            var corpseItem = caster.Entity.Items
                .GetItem(CustomCorpseItem.TemplateItemGroup, CustomCorpseItem.TemplateIndex);
            if (corpseItem == null)
            {
                DaggerfallUI.AddHUDText("No corpse item available.");
                return false;
            }

            var blackRoseItem = caster.Entity.Items
                .GetItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.Black_rose);
            var whiteRoseItem = caster.Entity.Items
                .GetItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.White_rose);
            if (blackRoseItem == null && whiteRoseItem == null)
            {
                DaggerfallUI.AddHUDText("Black/White rose required.");
                return false;
            }

            return true;
        }

        protected void ConsumeReagents()
        {
            if (caster == null)
            {
                ChebsNecromancy.ChebError("ConsumeReagents: caster is null");
                return;
            }

            var corpseItem = caster.Entity.Items
                .GetItem(CustomCorpseItem.TemplateItemGroup, CustomCorpseItem.TemplateIndex);
            if (corpseItem == null)
            {
                ChebsNecromancy.ChebError("Failed to consume reagents: corpseItem is null");
                return;
            }
            caster.Entity.Items.RemoveOne(corpseItem);

            var blackRose = caster.Entity.Items
                .GetItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.Black_rose);
            var whiteRose = caster.Entity.Items
                .GetItem(ItemGroups.PlantIngredients2, (int)PlantIngredients2.White_rose);
            if (blackRose == null && whiteRose == null)
            {
                ChebsNecromancy.ChebError("Failed to consume reagents: black and white rose is null");
                return;
            }

            if (blackRose != null)
            {
                ChebsNecromancy.ChebLog("Consuming black rose");
                caster.Entity.Items.RemoveOne(blackRose);
            }
            else
            {
                ChebsNecromancy.ChebLog("Consuming white rose");
                caster.Entity.Items.RemoveOne(whiteRose);
            }
        }

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