using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonZombieEffect : ChebEffect
    {
        protected new static string effectKey = "Summon Zombie";
        protected new static string effectDescription = "Summons a zombie to follow and guard you.";

        public new static int CostA { get; set; }
        public new static int CostB { get; set; }
        public new static int CostOffset { get; set; }

        protected override void DoEffect()
        {
            base.DoEffect();

            var spawner = new GameObject("MinionSpawner");
            spawner.SetActive(false);
            var minionSpawner = spawner.AddComponent<MinionSpawner>();
            minionSpawner.foeType = MobileTypes.Zombie;
            spawner.SetActive(true);
        }
    }
}