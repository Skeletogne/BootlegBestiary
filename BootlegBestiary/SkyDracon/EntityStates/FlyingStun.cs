using RoR2;
using EntityStates;
using static BootlegBestiary.BootlegBestiary;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class FlyingStun : BaseState
    {
        private Animator modelAnimator;
        private bool unsubscribedToEvent;
        public float stunDurationToCarryOver;
        private static GameObject impactEffect = VanillaAssets.BeetleGuardSlamEffect;
        private SkyDraconBehaviourController controller;
        private static float timeoutDuration = 5f;
        private static int maxGroundedFrames = 2;
        private int groundedFrames;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayCrossfade("Base", "StunAirStart", 0.1f);
            modelAnimator = GetModelAnimator();
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            if (characterMotor != null)
            {
                characterMotor.onHitGroundAuthority += OnHitGround;
            }
        }

        private void OnHitGround(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            Vector3 groundPoint = hitGroundInfo.position;
            if (!unsubscribedToEvent)
            {
                bool success = Physics.Raycast(characterBody.transform.position, Vector3.down, 5f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                if (success)
                {
                    bool surfaceIsTooThin = Physics.Raycast(characterBody.transform.position - new Vector3(0f, 10f, 0f), Vector3.up, 10f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                    if (surfaceIsTooThin)
                    {
                        characterMotor.velocity = Vector3.zero;
                        return;
                    }
                    groundPoint = hitGroundInfo.position;
                }
                else
                {
                    return;
                }
            }
            if (characterMotor != null)
            {
                characterMotor.onHitGroundAuthority -= OnHitGround;
                unsubscribedToEvent = true;
                Util.PlaySound("Play_skyDracon_unburrow", characterBody.gameObject);
                EffectManager.SpawnEffect(impactEffect, new EffectData { origin = groundPoint }, true);
                characterBody.transform.position = groundPoint;
                controller.EnterSquirmState();
                modelAnimator.SetFloat("diveBombAngleCycle", 0f);
                outer.SetNextState(new Stun { stunDuration = stunDurationToCarryOver });
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (characterMotor != null && !unsubscribedToEvent)
            {
                characterMotor.onHitGroundAuthority -= OnHitGround;
                unsubscribedToEvent = true;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor != null && !unsubscribedToEvent)
            {
                if (characterMotor.isGrounded)
                {
                    groundedFrames++;
                }
                else if (groundedFrames > 0)
                {
                    groundedFrames = 0;
                }
                if (groundedFrames > maxGroundedFrames)
                {
                    outer.SetNextStateToMain();
                    return;
                }
                characterMotor.velocity += new Vector3(0f, -1f, 0f);
                Vector3 velocityDirection = characterMotor.velocity.normalized;
                float pitchInDegrees = Mathf.Asin(Mathf.Clamp(velocityDirection.y, -1f, 1f)) * Mathf.Rad2Deg;
                float pitchValue = Mathf.Clamp((pitchInDegrees + 90f) / 180f, 0f, 0.999f);
                modelAnimator.SetFloat("diveBombAngleCycle", pitchValue);
            }
            if (base.fixedAge > timeoutDuration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
