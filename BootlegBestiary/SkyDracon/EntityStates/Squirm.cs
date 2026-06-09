using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using static BootlegBestiary.BootlegBestiary;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class Squirm : BaseState
    {
        private static float baseDuration = 2.95f;
        public Vector3 direction;
        private static GameObject impactEffect = VanillaAssets.BeetleGuardSlamEffect;
        private static GameObject burrowEffect = VanillaAssets.MiniMushrumPlantEffect;
        private float effectTimer;
        private static float effectInterval = 0.2f;
        public Vector3 groundHitPoint;
        private Animator animator;
        private SkyDraconBehaviourController controller;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayCrossfade("Base", "DiveBombEnd", 0.1f);
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            if (controller != null && controller.draconState != SkyDraconBehaviourController.DraconState.Squirming)
            {
                animator.SetFloat("diveBombAngleCycle", 0f);
                Util.PlaySound("Play_skyDracon_unburrow", characterBody.gameObject);
                EffectManager.SpawnEffect(impactEffect, new EffectData { origin = groundHitPoint, scale = 1f }, true);
                controller.EnterSquirmState();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (outer.nextState is SkyDraconCharacterMain)
            {
                outer.SetNextState(new BurrowRelocate());
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.transform.position = groundHitPoint;
            inputBank.moveVector = direction;
            inputBank.aimDirection = direction;
            characterDirection.moveVector = direction;
            effectTimer -= Time.fixedDeltaTime;
            if (effectTimer < 0)
            {
                effectTimer += effectInterval;
                EffectManager.SpawnEffect(burrowEffect, new EffectData { origin = groundHitPoint, scale = 1f }, true);
            }
            if (base.fixedAge > baseDuration)
            {
                controller.BeginBurrowedState();
                outer.SetNextState(new BurrowRelocate());
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
