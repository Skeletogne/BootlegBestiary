using UnityEngine;
using EntityStates;
using RoR2;
using System.Linq;
using static BootlegBestiary.BootlegBestiary;
using BootlegBestiary.SkyDracon.Components;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class SkyDraconCharacterMain : GenericCharacterMain
    {
        private EntityStateMachine weaponEsm;
        private bool attacking;
        private float groundCheckTimer;
        private float groundCheckInterval = 1f;
        private bool playedFlapThisLoop;
        private SkyDraconBehaviourController controller;
        private Animator animator;

        //weapon is always idle as it's unused, need to clean this up

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            controller = GetComponent<SkyDraconBehaviourController>();
            if (controller != null)
            {
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Flying)
                {
                    PlayCrossfade("Base", "Move", 0.1f);
                    animator.SetFloat("diveBombAngleCycle", 0.5f);
                }
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Grounded)
                {
                    PlayCrossfade("Base", "GroundedIdle", 0.1f);
                    animator.SetFloat("diveBombAngleCycle", 0.5f);
                }
            }
            weaponEsm = characterBody.GetComponents<EntityStateMachine>().Where(esm => esm.customName == "Weapon").FirstOrDefault();
            groundCheckTimer = groundCheckInterval;
        }
        public override void OnExit()
        {
            base.OnExit();
            //re-enables the aimAnimator so that it is enabled for all states
            if (aimAnimator != null)
            {
                aimAnimator.enabled = true;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool weaponIdle = (weaponEsm.state is Idle);
            if (weaponIdle)
            {
                if (!playedFlapThisLoop && modelAnimator != null && modelAnimator.GetFloat("moveFlapTrigger") > 0.4f)
                {
                    playedFlapThisLoop = true;
                    Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
                }
                if (playedFlapThisLoop && modelAnimator != null && modelAnimator.GetFloat("moveFlapTrigger") < 0.4f)
                {
                    playedFlapThisLoop = false;
                }
            }
            if (weaponIdle && attacking)
            {
                attacking = false;
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Flying)
                {
                    PlayCrossfade("Base", "Move", 0.2f);
                }
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Grounded)
                {
                    PlayCrossfade("Base", "GroundedIdle", 0.2f);
                }
            }
            if (!weaponIdle && !attacking)
            {
                attacking = true;
            }
            if (weaponIdle)
            {
                groundCheckTimer -= Time.fixedDeltaTime;
                if (groundCheckTimer < 0f)
                {
                    groundCheckTimer += groundCheckInterval;
                    if (Physics.Raycast(characterBody.transform.position, Vector3.down, 10f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        if (!Physics.Raycast(characterBody.transform.position, Vector3.up, 20f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            characterMotor.Motor.ForceUnground();
                            characterMotor.velocity += new Vector3(0f, 10f, 0f);
                        }
                    }
                }
            }
        }
    }
}
