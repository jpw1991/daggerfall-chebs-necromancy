using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using UnityEngine;

namespace ChebsNecromancyMod
{
    public class UndeadMinion : MonoBehaviour
    {
        public float stopDistance = 5f;
        public float doorCrouchingHeight = 5f;

        private EnemyMotor enemyMotor;
        //private EnemyEntity enemyEntity;
        private EnemySenses enemySenses;
        private bool obstacleDetected;
        private bool fallDetected;
        private bool foundUpwardSlope;
        private bool foundDoor;
        private Vector3 destination;
        private CharacterController controller;
        private float originalHeight;
        private MobileUnit mobile;
        private int ignoreMaskForObstacles;

        protected void Awake()
        {
            enemyMotor = GetComponent<EnemyMotor>();
            enemySenses = enemyMotor.GetComponent<EnemySenses>();
            controller = enemyMotor.GetComponent<CharacterController>();
            mobile = enemyMotor.transform.GetComponentInChildren<MobileUnit>();

            ignoreMaskForObstacles = ~(1 << LayerMask.NameToLayer("SpellMissiles") | 1 << LayerMask.NameToLayer("Ignore Raycast"));

            enemyMotor.IsHostile = false;
        }

        private void FixedUpdate()
        {
            if (enemyMotor != null && enemySenses != null && !enemySenses.TargetInSight)
            {
                FollowPlayer();
            }
        }

        protected void FollowPlayer()
        {
            // Move the enemy twice as fast as you walk so you don't lose them
            float moveSpeed = (1 + PlayerSpeedChanger.dfWalkBase) * MeshReader.GlobalScale * 1.5f;//(enemyEntity.Stats.LiveSpeed + PlayerSpeedChanger.dfWalkBase) * MeshReader.GlobalScale * 1.5f;

            destination = Camera.main.transform.position;

            // Get direction & distance to destination.
            Vector3 direction = (destination - enemyMotor.transform.position).normalized;
            var distance = (destination - enemyMotor.transform.position).magnitude;

            if (distance >= stopDistance)
            {
                AttemptMove(direction, moveSpeed, Camera.main.transform);
            }
            else if (!enemySenses.TargetIsWithinYawAngle(22.5f, destination))
            {
                TurnToTarget(direction);
            }
        }

        void AttemptMove(Vector3 direction, float moveSpeed, Transform targetTransform)
        {
            if (!enemySenses.TargetIsWithinYawAngle(5.625f, destination))
            {
                TurnToTarget(direction);
            }

            Vector3 motion = direction * moveSpeed;

            // Check if there is something to collide with directly in movement direction, such as upward sloping ground.
            var direction2d = direction;
            direction2d.y = 0;

            ObstacleCheck(direction2d);
            FallCheck(direction2d);

            if (fallDetected || obstacleDetected)
            {
                enemyMotor.transform.position = targetTransform.position - targetTransform.forward;
            }
            else
            {
                controller.Move(motion * Time.deltaTime);
            }
        }

        void TurnToTarget(Vector3 targetDirection)
        {
            const float turnSpeed = 20f;

            enemyMotor.transform.forward = Vector3.RotateTowards(enemyMotor.transform.forward, targetDirection, turnSpeed * Mathf.Deg2Rad, 0.0f);
        }

        void FallCheck(Vector3 direction)
        {
            if (CanFly() || enemyMotor.IsLevitating || obstacleDetected || foundUpwardSlope || foundDoor)
            {
                fallDetected = false;
                return;
            }

            var checkDistance = 1;
            var rayOrigin = enemyMotor.transform.position;

            direction *= checkDistance;
            var ray = new Ray(rayOrigin + direction, Vector3.down);
            fallDetected = !Physics.Raycast(ray, out RaycastHit hit, (originalHeight * 0.5f) + 1.5f);
        }

        bool CanFly()
        {
            return mobile.Enemy.Behaviour == MobileBehaviour.Flying || mobile.Enemy.Behaviour == MobileBehaviour.Spectral;
        }

        void ObstacleCheck(Vector3 direction)
        {
            obstacleDetected = false;
            float checkDistance = controller.radius / Mathf.Sqrt(2f);
            foundUpwardSlope = false;
            foundDoor = false;

            RaycastHit hit;
            // Climbable/not climbable step for the player seems to be at around a height of 0.65f. The player is 1.8f tall.
            // Using the same ratio to height as these values, set the capsule for the enemy.
            Vector3 p1 = enemyMotor.transform.position + (Vector3.up * -originalHeight * 0.1388F);
            Vector3 p2 = p1 + (Vector3.up * Mathf.Min(originalHeight, doorCrouchingHeight) / 2);

            if (Physics.CapsuleCast(p1, p2, controller.radius / 2, direction, out hit, checkDistance, ignoreMaskForObstacles))
            {
                // Debug.DrawRay(transform.position, direction, Color.red, 2.0f);
                obstacleDetected = true;
                DaggerfallEntityBehaviour entityBehaviour2 = hit.transform.GetComponent<DaggerfallEntityBehaviour>();
                DaggerfallActionDoor door = hit.transform.GetComponent<DaggerfallActionDoor>();
                DaggerfallLoot loot = hit.transform.GetComponent<DaggerfallLoot>();

                if (entityBehaviour2)
                {
                    if (entityBehaviour2 == enemySenses.Target)
                        obstacleDetected = false;
                }
                else if (door)
                {
                    obstacleDetected = false;
                    foundDoor = true;
                    if (enemySenses.TargetIsWithinYawAngle(22.5f, door.transform.position))
                    {
                        enemySenses.LastKnownDoor = door;
                        enemySenses.DistanceToDoor = Vector3.Distance(enemyMotor.transform.position, door.transform.position);
                    }
                }
                else if (loot)
                {
                    obstacleDetected = false;
                }
                else if (!CanFly() && !enemyMotor.IsLevitating)
                {
                    // If an obstacle was hit, check for a climbable upward slope
                    Vector3 checkUp = enemyMotor.transform.position + direction;
                    checkUp.y++;

                    direction = (checkUp - enemyMotor.transform.position).normalized;
                    p1 = enemyMotor.transform.position + (Vector3.up * -originalHeight * 0.25f);
                    p2 = p1 + (Vector3.up * originalHeight * 0.75f);

                    if (!Physics.CapsuleCast(p1, p2, controller.radius / 2, direction, checkDistance))
                    {
                        obstacleDetected = false;
                        foundUpwardSlope = true;
                    }
                }
            }
        }
    }
}