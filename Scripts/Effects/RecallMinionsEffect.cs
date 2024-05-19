using DaggerfallConnect;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class RecallMinionsEffect : ChebEffect
    {
        public const string EffectKey = "Recall Minions";
        protected override string effectKey => EffectKey;
        protected override string effectDescription => "Recall all minions to your location.";

        public new int ChanceCostA { get; set; }
        public new int ChanceCostB { get; set; }
        public new int ChanceCostOffset { get; set; }

        public override void SetProperties()
        {
            properties.Key = effectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
            properties.DisableReflectiveEnumeration = true;

            properties.SupportChance = true;
            properties.ChanceCosts = MakeEffectCosts(ChanceCostA, ChanceCostB, ChanceCostOffset);
            // what purpose could duration/magnitude have on recall?
            properties.SupportDuration = false;
            // properties.DurationCosts = MakeEffectCosts(8, 100, 200);
            properties.SupportMagnitude = false;
            //properties.MagnitudeCosts = MakeEffectCosts(MagnitudeCostA, MagnitudeCostB, MagnitudeCostOffset);
        }

        protected override void DoEffect()
        {
            base.DoEffect();

            RecallMinions();
        }

        public static void RecallMinions(float maxDistance = 3f)
        {
            if (Camera.main == null)
            {
                ChebsNecromancy.ChebError("RecallMinionsEffect.RecallMinions: Camera.main is null.");
                return;
            }

            var cameraTransform = Camera.main.transform;
            var center = cameraTransform.position + cameraTransform.forward * 2;

            var activeMinions = UndeadMinion.GetActiveMinions();
            activeMinions.ForEach(minion =>
            {

                var randomPos = Random.insideUnitSphere.normalized * maxDistance + center;
                randomPos.y = center.y;
                minion.transform.position = randomPos;
            });
        }
    }
}