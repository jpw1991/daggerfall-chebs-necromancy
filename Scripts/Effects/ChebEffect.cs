using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace ChebsNecromancyMod
{
    public class ChebEffect : BaseEntityEffect
    {
        protected virtual string effectKey => "Recall Minions";
        protected virtual string effectDescription => "Recall all minions to your location.";

        public int CostA { get; set; }
        public int CostB { get; set; }
        public int CostOffset { get; set; }

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
            properties.ChanceCosts = MakeEffectCosts(CostA, CostB, CostOffset);
            properties.SupportDuration = false;
            // properties.DurationCosts = MakeEffectCosts(8, 100, 200);
            properties.SupportMagnitude = false;
            // properties.MagnitudeCosts = MakeEffectCosts(8, 100, 200);
        }

        #region Text

        public override string GroupName => effectKey;
        public override TextFile.Token[] SpellMakerDescription => GetSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => GetSpellBookDescription();

        private TextFile.Token[] GetSpellMakerDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                effectDescription,
                "Duration: Instantaneous.",
                "Chance: % Chance summoning will succeed.",
                "Magnitude: N/A");
        }

        private TextFile.Token[] GetSpellBookDescription()
        {
            return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                TextFile.Formatting.JustifyCenter,
                GroupName,
                "Duration: Instantaneous.",
                "Chance: %bch + %ach per %clc level(s)",
                "Magnitude: N/A",
                effectDescription);
        }

        #endregion

        public override void MagicRound()
        {
            base.MagicRound();

            var entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            DoEffect();
        }

        protected virtual void DoEffect()
        {
            // implement in subclass
        }
    }
}