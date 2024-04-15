using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class SummonAncientLichEffect : ChebEffect
    {
        protected new static string effectKey = "Summon Ancient Lich";
        protected new static string effectDescription = "Summons an ancient lich to follow and guard you.";

        public new static int CostA { get; set; }
        public new static int CostB { get; set; }
        public new static int CostOffset { get; set; }

        protected override void DoEffect()
        {
            base.DoEffect();

            var spawner = new GameObject("MinionSpawner");
            spawner.SetActive(false);
            var minionSpawner = spawner.AddComponent<MinionSpawner>();
            minionSpawner.foeType = MobileTypes.AncientLich;
            spawner.SetActive(true);
        }
    }
}