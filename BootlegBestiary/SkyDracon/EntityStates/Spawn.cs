using EntityStates;
using UnityEngine;
using RoR2;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class Spawn : BaseState
    {
        private static string spawnStateString = "Spawn";
        private static float duration = 2f;
        private static GameObject spawnEffect = VanillaAssets.BeetleGuardSlamEffect;
        private static float screechDelay = 1f;
        private bool playedScreech = false;
        private bool playedFlap = false;
        private Animator modelAnimator;
        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            PlayAnimation("Base", spawnStateString);
            PlayAnimation("DiveBombAngle", "DiveBombAngleControl");
            modelAnimator.SetFloat("diveBombAngleCycle", 0.5f);
            Util.PlaySound("Play_skyDracon_unburrow", characterBody.gameObject);
            EffectManager.SpawnEffect(spawnEffect, new EffectData { origin = characterBody.transform.position, scale = 1f }, true);
            if (characterMotor != null && characterMotor.Motor != null)
            {
                characterMotor.Motor.ForceUnground();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (characterBody.skillLocator != null && characterBody.skillLocator.secondary != null)
            {
                skillLocator.secondary.RemoveAllStocks();
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float velocityScaling = 1f - Mathf.Clamp01(base.fixedAge / 0.75f);
            if (velocityScaling > 0f)
            {
                characterMotor.velocity = Vector3.up * 30f * velocityScaling;
            }
            if (base.fixedAge > screechDelay && !playedScreech)
            {
                playedScreech = true;
                Util.PlaySound("Play_skyDracon_screech", characterBody.gameObject);
            }
            if (modelAnimator != null && modelAnimator.GetFloat("spawnFlapTrigger") > 0.6f && !playedFlap)
            {
                playedFlap = true;
                Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
            }
            if (base.fixedAge > duration && base.isAuthority)
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
