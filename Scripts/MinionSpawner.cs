using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ChebsNecromancyMod
{
    public class MinionSpawner : MonoBehaviour
    {
        public MobileTypes foeType = MobileTypes.SkeletalWarrior;
        public int spawnCount = 3;
        public float maxDistance = 3f;
        public bool alliedToPlayer = true;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            GameObjectHelper.CreateFoeGameObjects(Vector3.zero, foeType, spawnCount, alliedToPlayer: alliedToPlayer)
                .ToList()
                .ForEach(m =>
                {
                    PositionMinion(m);
                    FinalizeMinion(m);
                });

            Destroy(this);
        }

        void PositionMinion(GameObject minion)
        {
            var center = Camera.main.transform.position;
            var randomPos = Random.insideUnitSphere.normalized * maxDistance + center;
            randomPos.y = center.y;
            minion.transform.position = randomPos;
        }

        void FinalizeMinion(GameObject minion)
        {
            var mobileUnit = minion.GetComponentInChildren<MobileUnit>();
            if (mobileUnit)
            {
                if (mobileUnit.Enemy.Behaviour != MobileBehaviour.Flying)
                    GameObjectHelper.AlignControllerToGround(minion.GetComponent<CharacterController>());
                else
                    minion.transform.localPosition += Vector3.up * 1.5f;
            }
            else
            {
                GameObjectHelper.AlignControllerToGround(minion.GetComponent<CharacterController>());
            }

            minion.AddComponent<UndeadMinion>();
            minion.SetActive(true);
        }
    }
}