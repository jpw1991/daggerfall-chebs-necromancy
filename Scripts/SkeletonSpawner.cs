using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility;

namespace ChebsNecromancyMod
{
    public class SkeletonSpawner : FoeSpawner
    {
        void Start()
        {
            FoeType = MobileTypes.SkeletalWarrior;
            MinDistance = 1f;
            MaxDistance = 3f;
            AlliedToPlayer = true;
            SpawnCount = 3;
        }
    }
}