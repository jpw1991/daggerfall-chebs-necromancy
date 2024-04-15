using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonVampireEffect : ChebEffect
    {
        protected override string effectKey => "Summon Vampire";
        protected override string effectDescription => "Summons a vampire to follow and guard you.";

        public new static int CostA { get; set; }
        public new static int CostB { get; set; }
        public new static int CostOffset { get; set; }

        protected override void DoEffect()
        {
            base.DoEffect();

            var spawner = new GameObject("MinionSpawner");
            spawner.SetActive(false);
            var minionSpawner = spawner.AddComponent<MinionSpawner>();
            minionSpawner.foeType = MobileTypes.Vampire;
            spawner.SetActive(true);
        }
    }
}