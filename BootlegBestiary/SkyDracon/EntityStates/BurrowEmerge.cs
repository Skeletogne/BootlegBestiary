using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class BurrowEmerge : BaseSkillState
    {
        private static float startDuration = 1.5f;
        private static float burrowDuration = 1.25f;
        private static float telegraphDuration = 1f;
        private static float emergeDuration = 1f;
        private SkyDraconBehaviourController controller;
        private static GameObject indicatorPrefab = VanillaAssets.GroundOnlyTargetIndicator;
        private GameObject indicatorInstance;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float effectTimer;
        private static float effectInterval = 0.15f;
        private static GameObject burrowEffect = VanillaAssets.MiniMushrumPlantEffect;
        private static GameObject emergeEffect = VanillaAssets.BeetleGuardSlamEffect;
        private Vector3 differenceBetweenPositionAndFootPosition;
        private enum UnburrowState
        {
            Start,
            Burrowing,
            Telegraphing,
            Emerging
        }
        private UnburrowState unburrowState = UnburrowState.Start;
        public override void OnEnter()
        {
            base.OnEnter();
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            controller.RemoveCollision();
            PlayCrossfade("Base", "GroundedBurrow", 0.1f);
            startPosition = characterBody.transform.position;
            differenceBetweenPositionAndFootPosition = characterBody.transform.position - characterBody.footPosition;
        }
        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(indicatorInstance);
            if (skillLocator != null && skillLocator.secondary != null)
            {
                skillLocator.secondary.RemoveAllStocks();
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > startDuration && unburrowState == UnburrowState.Start)
            {
                unburrowState = UnburrowState.Burrowing;
                controller.BeginBurrowedState();
                if (characterBody != null && characterBody.master != null)
                {
                    targetPosition = characterBody.transform.position;
                    BaseAI baseAI = characterBody.master.gameObject.GetComponent<BaseAI>();
                    if (baseAI != null && baseAI.currentEnemy.characterBody != null)
                    {
                        Vector3 currentEnemyPosition = baseAI.currentEnemy.characterBody.corePosition;
                        Vector3 currentPosition = characterBody.transform.position;
                        if (Vector3.Distance(currentEnemyPosition, targetPosition) < 50f)
                        {
                            if (Physics.Raycast(currentEnemyPosition, Vector3.down, out RaycastHit hit, 50f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                            {
                                targetPosition = hit.point;
                            }
                        }
                    }
                }
            }
            if (unburrowState == UnburrowState.Burrowing)
            {
                float burrowPercentage = Mathf.Clamp01((base.fixedAge - startDuration) / burrowDuration);
                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, burrowPercentage);
                TeleportHelper.TeleportBody(characterBody, newPosition, true);
                effectTimer -= Time.fixedDeltaTime;
                if (effectTimer < 0)
                {
                    effectTimer += effectInterval;
                    Vector3 effectPosition = characterBody.transform.position;
                    Vector3 raycastStartPosition = characterBody.transform.position;
                    raycastStartPosition.y = Mathf.Max(startPosition.y, targetPosition.y);
                    bool success = Physics.Raycast(raycastStartPosition + new Vector3(0f, 10f, 0f), Vector3.down, out RaycastHit hit, 50f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                    if (success)
                    {
                        effectPosition = hit.point;
                        EffectManager.SpawnEffect(burrowEffect, new EffectData { origin = effectPosition, scale = 1f }, true);
                    }
                }
            }
            if (base.fixedAge > startDuration + burrowDuration && unburrowState == UnburrowState.Burrowing)
            {
                unburrowState = UnburrowState.Telegraphing;
                indicatorInstance = UnityEngine.Object.Instantiate(indicatorPrefab, targetPosition, Util.QuaternionSafeLookRotation(Vector3.up));
                TeamAreaIndicator teamAreaIndicator = indicatorInstance.GetComponent<TeamAreaIndicator>();
                teamAreaIndicator.teamComponent = characterBody.teamComponent;
                teamAreaIndicator.transform.localScale = Vector3.one * 6f;
            }
            if (base.fixedAge > startDuration + burrowDuration + telegraphDuration && unburrowState == UnburrowState.Telegraphing)
            {

                unburrowState = UnburrowState.Emerging;
                controller.RestoreCollision();
                controller.ExitBurrowIntoAirborneState();
                TeleportHelper.TeleportBody(characterBody, targetPosition + new Vector3(0f, 2f, 0f), true);
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = new Vector3(0f, 25f, 0f);
                PlayAnimation("Base", "GroundedToAir");
                EffectManager.SpawnEffect(emergeEffect, new EffectData { origin = targetPosition, scale = 1f }, true);

                Util.PlaySound("Play_skyDracon_unburrow", characterBody.gameObject);
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = characterBody.gameObject;
                blastAttack.inflictor = characterBody.gameObject;
                blastAttack.position = targetPosition;
                blastAttack.radius = 6f;
                blastAttack.crit = RollCrit();
                blastAttack.procCoefficient = 1f;
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.baseDamage = 2f * damageStat;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.baseForce = 2000f;
                blastAttack.bonusForce = new Vector3(0f, 2000f, 0f);
                blastAttack.damageType = new DamageTypeCombo { damageType = DamageType.Generic, damageSource = DamageSource.Secondary };
                blastAttack.procChainMask = default(ProcChainMask);
                blastAttack.Fire();
            }
            if (base.fixedAge > startDuration + burrowDuration + telegraphDuration + emergeDuration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (unburrowState == UnburrowState.Start || unburrowState == UnburrowState.Emerging)
            {
                return InterruptPriority.PrioritySkill;
            }
            else
            {
                return InterruptPriority.Death;
            }
        }
    }
}
