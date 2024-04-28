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

        public int ChanceCostA { get; set; }
        public int ChanceCostB { get; set; }
        public int ChanceCostOffset { get; set; }
        public int MagnitudeCostA { get; set; }
        public int MagnitudeCostB { get; set; }
        public int MagnitudeCostOffset { get; set; }

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
            properties.DisableReflectiveEnumeration = true;
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