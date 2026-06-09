using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using UnityEngine;

namespace BootlegBestiary.Demineur.EntityStates
{
    public class DemineurCharacterMain : GenericCharacterMain
    {
        private static float groundCheckInterval = 0.2f;
        private float groundCheckTimer;
        private float groundDistance;
        private float maximumFallSpeed;
        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor != null && characterMotor.velocity.y < -maximumFallSpeed && !characterMotor.isGrounded)
            {
                characterMotor.velocity.y = -maximumFallSpeed;
            }
            groundCheckTimer -= Time.fixedDeltaTime;
            if (groundCheckTimer < 0)
            {
                groundCheckTimer += groundCheckInterval;
                groundDistance = -1f;
                bool success = Physics.Raycast(characterBody.footPosition, Vector3.down, out RaycastHit hit, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
                if (success)
                {
                    groundDistance = hit.distance;
                }
                if (groundDistance == -1f)
                {
                    maximumFallSpeed = 100f;
                }
                else
                {
                    maximumFallSpeed = Mathf.Clamp(groundDistance, 1f, 100f);
                }
            }
        }
    }
}
