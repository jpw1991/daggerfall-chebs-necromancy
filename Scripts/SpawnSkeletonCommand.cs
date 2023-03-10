using UnityEngine;

namespace ChebsNecromancyMod
{
    public static class SpawnSkeletonCommand
    {
        public static readonly string name = "spawnskeleton";
        public static readonly string description = "Spawns a skeleton ally.";
        public static readonly string usage = "spawnskeleton";

        public static string Execute(params string[] args)
        {
            var spawner = new GameObject("SkeletonSpawner").AddComponent<MinionSpawner>();
            return "";
        }
    }
}


