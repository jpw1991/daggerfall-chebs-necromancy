using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using Wenzil.Console;

namespace ChebsNecromancyMod
{
    public class ChebsNecromancy : MonoBehaviour
    {
        private static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<ChebsNecromancy>();

            ConsoleCommandsDatabase.RegisterCommand(
                SpawnSkeletonCommand.name, SpawnSkeletonCommand.description,
                SpawnSkeletonCommand.usage, SpawnSkeletonCommand.Execute);

            mod.IsReady = true;
        }
    }
}
