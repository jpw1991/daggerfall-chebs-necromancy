using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonSkeletonEffect : ChebEffect
    {
        protected override string effectKey => "Summon Skeleton";
        protected override string effectDescription => "Summons a skeleton to follow and guard you.";

        public new static int CostA { get; set; }
        public new static int CostB { get; set; }
        public new static int CostOffset { get; set; }

        protected override void DoEffect()
        {
            base.DoEffect();

            var spawner = new GameObject("MinionSpawner");
            spawner.SetActive(false);
            var minionSpawner = spawner.AddComponent<MinionSpawner>();
            minionSpawner.foeType = MobileTypes.SkeletalWarrior;
            spawner.SetActive(true);
        }
    }
}