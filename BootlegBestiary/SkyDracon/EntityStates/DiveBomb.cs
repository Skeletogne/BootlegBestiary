using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class DiveBomb : BaseSkillState
    {
        private static float windupDuration = 1.25f;
        private static float attackDuration = 2f;
        private static float attackDurationLeniency = 0.25f;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 initialVelocity;
        private Vector3 moveDirection;
        private bool hitGround = false;
        private bool startedDive = false;
        private Animator animator;
        private float desiredPitchValue;
        private bool failedToBurrow;
        private float flapTimer;
        private static float flapInterval = 0.2f;
        private int flapCount;
        private static int maxFramesTouchingGround = 2;
        private int framesTouchingGround;
        public override void OnEnter()
        {
            base.OnEnter();
            flapTimer = flapInterval;
            animator = GetModelAnimator();
            hitGround = false;
            characterMotor.onHitGroundAuthority += OnHitGround;
            startPosition = characterBody.transform.position;
            if (characterBody != null && characterBody.master != null)
            {
                BaseAI ai = characterBody.master.gameObject.GetComponent<BaseAI>();
                if (ai != null && ai.currentEnemy.characterBody != null)
                {
                    CharacterBody targetBody = ai.currentEnemy.characterBody;
                    NodeGraph groundNodeGraph = SceneInfo.instance.groundNodes;

                    bool success = Physics.Raycast(targetBody.transform.position, Vector3.down, out RaycastHit hit, 50f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                    if (success)
                    {
                        targetPosition = hit.point;
                    }
                    else
                    {
                        if (targetBody != null && groundNodeGraph != null)
                        {
                            NodeGraph.NodeIndex nodeIndex = groundNodeGraph.FindClosestNode(targetBody.transform.position, HullClassification.Human);
                            if (!groundNodeGraph.GetNodePosition(nodeIndex, out targetPosition) || targetPosition.y > startPosition.y)
                            {
                                outer.SetNextStateToMain();
                                return;
                            }
                        }
                    }
                    initialVelocity = Trajectory.CalculateInitialVelocityFromTime(startPosition, targetPosition, attackDuration - attackDurationLeniency, gravity: Physics.gravity.y * 1.5f);
                    moveDirection = targetPosition - startPosition;
                    moveDirection.y = 0f;
                    moveDirection.Normalize();
                    PlayCrossfade("Base", "DiveBombStart", 0.1f);
                    Vector3 initialVelocityDirection = initialVelocity.normalized;
                    float pitchInDegrees = Mathf.Asin(Mathf.Clamp(initialVelocityDirection.y, -1f, 1f)) * Mathf.Rad2Deg;
                    desiredPitchValue = Mathf.Clamp((pitchInDegrees + 90f) / 180f, 0f, 0.999f);
                }
            }
        }
        private void StartDiveBomb()
        {
            PlayCrossfade("Base", "DiveBomb", 0.1f);
        }
        private void OnHitGround(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            Vector3 ground = hitGroundInfo.position;
            bool success = Physics.Raycast(characterBody.transform.position, Vector3.down, out RaycastHit hit, 5f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
            if (success)
            {
                bool surfaceIsTooThin = Physics.Raycast(characterBody.transform.position - new Vector3(0f, 10f, 0f), Vector3.up, 10f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                if (surfaceIsTooThin)
                {
                    characterMotor.velocity = Vector3.zero;
                    return;
                }
                ground = hit.point;
            }
            else
            {
                return;
            }
            if (!startedDive)
            {
                outer.SetNextStateToMain();
                return;
            }
            if (hitGround)
            {
                return;
            }
            hitGround = true;
            characterMotor.velocity = Vector3.zero;
            outer.SetNextState(new Squirm { direction = moveDirection, groundHitPoint = ground });

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.attacker = characterBody.gameObject;
            blastAttack.inflictor = characterBody.gameObject;
            blastAttack.position = ground;
            blastAttack.radius = 6f;
            blastAttack.crit = RollCrit();
            blastAttack.procCoefficient = 1f;
            blastAttack.damageColorIndex = DamageColorIndex.Default;
            blastAttack.baseDamage = 1f * damageStat;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            blastAttack.baseForce = 2000f;
            blastAttack.bonusForce = new Vector3(0f, 2000f, 0f);
            blastAttack.damageType = new DamageTypeCombo { damageType = DamageType.Generic, damageSource = DamageSource.Secondary };
            blastAttack.procChainMask = default(ProcChainMask);
            blastAttack.Fire();
        }
        public override void OnExit()
        {
            base.OnExit();
            characterMotor.onHitGroundAuthority -= OnHitGround;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hitGround && characterMotor != null && characterMotor.isGrounded)
            {
                framesTouchingGround++;
            }
            else if (framesTouchingGround != 0)
            {
                framesTouchingGround = 0;
            }
            if (framesTouchingGround > maxFramesTouchingGround && !hitGround)
            {
                //gotten stuck somehow, just get the heck outta there
                outer.SetNextStateToMain();
                return;
            }
            flapTimer -= Time.fixedDeltaTime;
            if (flapTimer < 0 && flapCount < 3)
            {
                flapTimer += flapInterval;
                flapCount++;
                Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
            }
            if (hitGround)
            {
                return;
            }
            if (base.fixedAge > windupDuration && !startedDive)
            {
                startedDive = true;
                StartDiveBomb();
            }
            inputBank.moveVector = moveDirection;
            inputBank.aimDirection = moveDirection;
            characterDirection.moveVector = moveDirection;
            if (characterMotor)
            {
                Vector3 velocity = initialVelocity + ((Physics.gravity * 1.5f) * (base.fixedAge - windupDuration));
                if (velocity.y < 0)
                {
                    velocity.y *= 2f;
                }
                if (base.fixedAge - windupDuration < 0)
                {
                    velocity = initialVelocity * Mathf.Pow(Mathf.Clamp01(base.fixedAge / windupDuration), 3f);
                }
                characterMotor.velocity = velocity;
            }
            if (base.fixedAge > attackDuration + windupDuration)
            {
                failedToBurrow = true;
                animator.SetFloat("diveBombAngleCycle", 0.5f);
                outer.SetNextStateToMain();
            }
        }
        public override void Update()
        {
            base.Update();
            if (failedToBurrow)
            {
                animator.SetFloat("diveBombAngleCycle", 0.5f);
                return;
            }
            if (!startedDive)
            {
                if (animator != null)
                {
                    float windupPercentage = Mathf.Clamp01(base.age / windupDuration);
                    float newPitchValue = Mathf.Lerp(0.5f, desiredPitchValue, windupPercentage);
                    animator.SetFloat("diveBombAngleCycle", newPitchValue);
                    ;
                }
            }
            else
            {
                if (animator != null)
                {
                    if (characterMotor.velocity.sqrMagnitude > 0.01f)
                    {
                        Vector3 velocityDirection = characterMotor.velocity.normalized;
                        float pitchInDegrees = Mathf.Asin(Mathf.Clamp(velocityDirection.y, -1f, 1f)) * Mathf.Rad2Deg;
                        float newPitchValue = Mathf.Clamp((pitchInDegrees + 90f) / 180f, 0f, 0.999f);
                        animator.SetFloat("diveBombAngleCycle", newPitchValue);
                    }
                }
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
