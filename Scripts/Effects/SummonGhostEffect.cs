using ChebsNecromancyMod.MinionSpawners;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonGhostEffect : ChebEffect
    {
        protected override string effectKey => "Summon Ghost";
        protected override string effectDescription => "Summons a ghost to follow and guard you.";

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

            var ectoplasmItem = caster.Entity.Items
                .GetItem(ItemGroups.CreatureIngredients1, (int)CreatureIngredients1.Ectoplasm);
            if (ectoplasmItem == null)
            {
                DaggerfallUI.AddHUDText("Ectoplasm required.");
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

            var ectoplasmItem = caster.Entity.Items
                .GetItem(ItemGroups.CreatureIngredients1, (int)CreatureIngredients1.Ectoplasm);
            if (ectoplasmItem == null)
            {
                ChebsNecromancy.ChebError("Failed to consume reagents: ectoplasm is null");
                return;
            }
            caster.Entity.Items.RemoveOne(ectoplasmItem);
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
            var minionSpawner = spawner.AddComponent<GhostSpawner>();
            minionSpawner.showHUDMessage = showHUDMessage;
            minionSpawner.magnitude = magnitude;
            minionSpawner.mysticismLevel = mysticismLevel;
            minionSpawner.intelligence = intelligence;
            minionSpawner.willpower = willpower;
            spawner.SetActive(true);
        }
    }
}