using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class RecallMinionsEffect : ChebEffect
    {
        protected new static string effectKey = "Recall Minions";
        protected new static string effectDescription = "Recall all minions to your location.";

        public new static int CostA { get; set; }
        public new static int CostB { get; set; }
        public new static int CostOffset { get; set; }

        protected override void DoEffect()
        {
            base.DoEffect();

            RecallMinions();
        }

        public static void RecallMinions()
        {
            if (Camera.main == null)
            {
                Debug.LogError("ChebsNecromancy.RecallMinionsEffect.RecallMinions: Camera.main is null.");
                return;
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
        }
    }
}