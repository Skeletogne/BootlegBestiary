using RoR2;
using UnityEngine;
using EntityStates;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class Death : GenericCharacterDeath
    {
        private static float startDuration = 1f;
        private Animator animator;
        protected ICharacterGravityParameterProvider gravityProvider;
        protected ICharacterFlightParameterProvider flightProvider;
        private static GameObject dirtEffect = VanillaAssets.BellPartsImpactEffect;
        private float effectTimer;
        private static float effectInterval = 0.1f;
        private bool playedFlap;
        private SkyDraconBehaviourController controller;
        private SkyDraconBehaviourController.DraconState draconStateAtDeath;
        private enum FallingDeathAnimState
        {
            Start,
            Falling,
            End
        }
        private FallingDeathAnimState fallingDeathAnimState;
        public override void OnEnter()
        {
            draconStateAtDeath = SkyDraconBehaviourController.DraconState.Flying;
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            if (controller != null)
            {
                draconStateAtDeath = controller.draconState;
            }
            base.OnEnter();
            Util.PlaySound("Play_skyDracon_death_new", this.gameObject);
            maxFallDuration = 8f;
            animator = GetModelAnimator();
            animator.SetFloat("diveBombAngleCycle", 0.5f);
            if (draconStateAtDeath == SkyDraconBehaviourController.DraconState.Flying)
            {
                if (characterMotor != null)
                {
                    if (characterMotor.isGrounded)
                    {
                        HitGround();
                    }
                    else
                    {
                        characterMotor.onHitGroundAuthority += CharacterMotor_onHitGround;
                    }
                }

            }
            flightProvider = base.gameObject.GetComponent<ICharacterFlightParameterProvider>();
            gravityProvider = base.gameObject.gameObject.GetComponent<ICharacterGravityParameterProvider>();

        }
        private void CharacterMotor_onHitGround(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            HitGround();
        }
        private void HitGround()
        {
            if (fallingDeathAnimState != FallingDeathAnimState.End)
            {
                fallingDeathAnimState = FallingDeathAnimState.End;
                PlayCrossfade("Base", "DeathEnd", 0.1f);
                Util.PlaySound("Play_skyDracon_unburrow", this.gameObject);
                characterMotor.onHitGroundAuthority -= CharacterMotor_onHitGround;
                if (gravityProvider != null)
                {
                    CharacterGravityParameters gravityParams = gravityProvider.gravityParameters;
                    gravityParams.channeledAntiGravityGranterCount--;
                    gravityProvider.gravityParameters = gravityParams;
                }
                if (flightProvider != null)
                {
                    CharacterFlightParameters flightParams = flightProvider.flightParameters;
                    flightParams.channeledFlightGranterCount--;
                    flightProvider.flightParameters = flightParams;
                }
            }
        }
        public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
        {
            if (draconStateAtDeath == SkyDraconBehaviourController.DraconState.Flying)
            {
                PlayCrossfade("Base", "DeathStart", crossfadeDuration);
            }
            if (draconStateAtDeath == SkyDraconBehaviourController.DraconState.Squirming)
            {
                PlayCrossfade("Base", "SquirmDeath", 0.1f);
            }
            if (draconStateAtDeath == SkyDraconBehaviourController.DraconState.Grounded)
            {
                PlayCrossfade("Base", "GroundedDeath", 0.1f);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor != null && characterMotor.isGrounded && fallingDeathAnimState != FallingDeathAnimState.End && draconStateAtDeath == SkyDraconBehaviourController.DraconState.Flying)
            {
                HitGround();
            }
            if (draconStateAtDeath == SkyDraconBehaviourController.DraconState.Flying)
            {
                if (base.fixedAge > startDuration && fallingDeathAnimState == FallingDeathAnimState.Start)
                {
                    fallingDeathAnimState = FallingDeathAnimState.Falling;
                    PlayCrossfade("Base", "DeathLoop", 0.1f);
                }
                if (animator != null && animator.GetFloat("deathFlapTrigger") > 0.6f && !playedFlap)
                {
                    playedFlap = true;
                    Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
                }
                if (fallingDeathAnimState != FallingDeathAnimState.End)
                {
                    if (characterMotor != null)
                    {
                        Vector3 direction = (1.5f * characterDirection.forward + Vector3.down).normalized;
                        characterMotor.velocity += direction * 50f * Time.fixedDeltaTime;
                    }
                }
                if (fallingDeathAnimState == FallingDeathAnimState.End && characterMotor != null && characterMotor.velocity.magnitude > 3f && characterMotor.isGrounded)
                {
                    effectTimer -= Time.fixedDeltaTime;
                    if (effectTimer < 0)
                    {
                        effectTimer += effectInterval;
                        EffectManager.SpawnEffect(dirtEffect, new EffectData { origin = characterBody.transform.position, scale = 10f }, true);
                    }
                }
            }
        }
    }
}
