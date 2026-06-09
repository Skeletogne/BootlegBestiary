using System;
using System.Collections.Generic;
using System.Text;
using static BootlegBestiary.BootlegBestiary;
using UnityEngine;
using EntityStates;
using RoR2;
using BootlegBestiary.SkyDracon.Components;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class Stun : StunState
    {
        private SkyDraconBehaviourController controller;
        private Animator modelAnimator;
        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            if (controller != null)
            {
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Flying)
                {
                    outer.SetNextState(new FlyingStun { stunDurationToCarryOver = stunDuration });
                }
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Squirming)
                {
                    PlayCrossfade("Base", "Squirm", 0.1f);
                }
                if (controller.draconState == SkyDraconBehaviourController.DraconState.Grounded)
                {
                    modelAnimator.SetFloat("diveBombAngleCycle", 0.5f);
                    PlayCrossfade("Base", "GroundedStun", 0.1f);
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (characterBody == null || characterBody.healthComponent == null || characterBody.healthComponent.alive == false)
            {
                return;
            }
            if (controller.draconState == SkyDraconBehaviourController.DraconState.Squirming)
            {
                Vector3 groundHitPoint = characterBody.transform.position;
                if (Physics.Raycast(characterBody.transform.position, Vector3.down, out RaycastHit hit, 10f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    groundHitPoint = hit.point;
                }
                outer.SetNextState(new Squirm { groundHitPoint = groundHitPoint, direction = characterBody.transform.rotation.eulerAngles });
            }
        }
    }
}
