using ChebsNecromancyMod.MinionSpawners;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonMummyEffect : ChebEffect
    {
        protected override string effectKey => "Summon Mummy";
        protected override string effectDescription => "Summons a mummy to follow and guard you.";

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
                ChebsNecromancy.ChebError("SummonMummyEffect.HasReagents: caster is null");
                return false;
            }

            var corpseItem = caster.Entity.Items
                .GetItem(CustomCorpseItem.TemplateItemGroup, CustomCorpseItem.TemplateIndex);
            if (corpseItem == null)
            {
                DaggerfallUI.AddHUDText("No corpse item available.");
                return false;
            }

            var bandage = caster.Entity.Items
                .GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Bandage);
            var oil = caster.Entity.Items
                .GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Oil);
            if (oil == null && bandage == null)
            {
                DaggerfallUI.AddHUDText("Bandages or oil required.");
                return false;
            }

            return true;
        }

        protected void ConsumeReagents()
        {
            if (caster == null)
            {
                ChebsNecromancy.ChebError("SummonMummyEffect.ConsumeReagents: caster is null");
                return;
            }

            var foundCorpseItem =
                caster.Entity.Items.GetItem(CustomCorpseItem.TemplateItemGroup, CustomCorpseItem.TemplateIndex);
            if (foundCorpseItem == null)
            {
                ChebsNecromancy.ChebError("Failed to consume reagents: foundCorpseItem is null");
                return;
            }

            // consume oil before bandages (oil seems more useless)
            var bandage = caster.Entity.Items
                .GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Bandage);
            var oil = caster.Entity.Items
                .GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Oil);
            if (oil == null && bandage == null)
            {
                ChebsNecromancy.ChebError("Failed to consume reagents: oil and bandages is null");
                return;
            }

            if (oil != null)
            {
                ChebsNecromancy.ChebLog("Consuming oil");
                caster.Entity.Items.RemoveItem(oil);
            }
            else
            {
                ChebsNecromancy.ChebLog("Consuming bandage");
                caster.Entity.Items.RemoveItem(bandage);
            }

            caster.Entity.Items.RemoveItem(foundCorpseItem);
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
            var minionSpawner = spawner.AddComponent<MummySpawner>();
            minionSpawner.showHUDMessage = showHUDMessage;
            minionSpawner.magnitude = magnitude;
            minionSpawner.mysticismLevel = mysticismLevel;
            minionSpawner.intelligence = intelligence;
            minionSpawner.willpower = willpower;
            spawner.SetActive(true);
        }
    }
}