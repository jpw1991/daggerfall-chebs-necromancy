using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public static class RecallMinionsCommand
    {
        public static readonly string name = "recallminions";
        public static readonly string description = "Recall all your summoned minions to your position.";
        public static readonly string usage = "recallminions";

        public static string Execute(params string[] args)
        {
            if (Camera.main == null)
            {
                Debug.LogError("ChebsNecromancy.RecallMinionsCommand.Execute: Camera.main is null.");
                return "";
            }
            var recallPosition = Camera.main.transform.position + Vector3.forward;
            var daggerfallEnemies = Object.FindObjectsOfType<DaggerfallEnemy>();
            foreach (var daggerfallEnemy in daggerfallEnemies)
            {
                if (daggerfallEnemy.MobileUnit.Enemy.Team == MobileTeams.PlayerAlly)
                {
                    daggerfallEnemy.transform.position = recallPosition;
                }
            }
            return "";
        }
    }
}


