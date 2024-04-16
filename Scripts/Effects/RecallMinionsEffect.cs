using DaggerfallWorkshop;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class RecallMinionsEffect : ChebEffect
    {
        protected override string effectKey => "Recall Minions";
        protected override string effectDescription => "Recall all minions to your location.";

        public int CostA { get; set; }
        public int CostB { get; set; }
        public int CostOffset { get; set; }

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
                if (daggerfallEnemy.MobileUnit.Enemy.Team == MobileTeams.PlayerAlly
                    && daggerfallEnemy.TryGetComponent(out UndeadMinion _))
                {
                    daggerfallEnemy.transform.position = recallPosition;
                }
            }
        }
    }
}