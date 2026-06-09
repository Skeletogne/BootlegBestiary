using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using static BootlegBestiary.BootlegBestiary;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using RoR2.CharacterAI;
using BootlegBestiary.Shared.Assets;
using BootlegBestiary.SkyDracon.Components;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class Flamebreath : BaseSkillState
    {
        private static float windupDuration = 1.25f;
        private float attackDuration = 1.5f;
        private static float exitDuration = 1.75f;
        private Vector3 moveDirection;
        private Vector3 attackDirection;
        private static float flightSpeed = 30f;
        private Transform flamebreathInstance;
        private EffectManagerHelper _emh_flamebreathEffectInstance;
        private static GameObject impactEffect = VanillaAssets.OmniExplosionEffect;
        private float tickInterval = 0.25f;
        private float tickTimer;
        private bool playedEndFlap;
        private static GameObject chargeVfxPrefab = VanillaAssets.LemBruiserFlamebreathChargeEffect;
        private GameObject chargeVfxInstance;
        private EffectManagerHelper _emh_chargeVfx;
        private SkyDraconBehaviourController controller;
        private bool groundedAttack;
        private float perTickDamageCoefficient = 1f;
        private enum FlamebreathState
        {
            Windup,
            Attack,
            Exit
        }
        private FlamebreathState attackState;
        private float totalDuration;
        private static string muzzleString = "Muzzle";
        private Transform muzzleTransform;
        private Animator modelAnimator;
        private CharacterBody targetBody;
        public override void OnEnter()
        {
            base.OnEnter();
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            if (controller != null)
            {
                groundedAttack = controller.draconState == SkyDraconBehaviourController.DraconState.Grounded;
            }
            if (!groundedAttack)
            {
                Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
            }
            attackDuration = groundedAttack ? 2.5f : 1.5f;
            perTickDamageCoefficient = groundedAttack ? 0.5f : 0.75f;
            Util.PlaySound("Play_skyDracon_flameWindup", this.gameObject);
            modelAnimator = GetComponent<Animator>();
            totalDuration = windupDuration + attackDuration + exitDuration;
            StartAimMode(totalDuration, false);
            attackState = FlamebreathState.Windup;
            PlayCrossfade("Body", groundedAttack ? "GroundedFlamebreathWindup" : "FlamebreathWindup", 0.2f);
            BaseAI baseAI = characterBody.master.gameObject.GetComponent<BaseAI>();
            ChildLocator locator = GetModelChildLocator();
            if (locator != null)
            {
                muzzleTransform = locator.FindChild(muzzleString);
            }

            if (baseAI != null && baseAI.currentEnemy.characterBody != null)
            {
                targetBody = baseAI.currentEnemy.characterBody;
                Vector3 targetDirection = targetBody.corePosition - characterBody.corePosition;
                if (!groundedAttack)
                {
                    targetDirection.y = 0f;
                    targetDirection.Normalize();
                    moveDirection = targetDirection;
                    attackDirection = (targetDirection + 2 * Vector3.down).normalized;
                }
                else
                {
                    moveDirection = targetDirection.normalized;
                    attackDirection = targetDirection.normalized;
                }
            }
            else
            {
                moveDirection = inputBank.aimDirection;
                moveDirection.y = 0f;
                attackDirection = inputBank.aimDirection;
            }
            if (muzzleTransform != null)
            {
                if (!EffectManager.ShouldUsePooledEffect(chargeVfxPrefab))
                {
                    chargeVfxInstance = UnityEngine.Object.Instantiate(chargeVfxPrefab, muzzleTransform.position, muzzleTransform.rotation);
                }
                else
                {
                    _emh_chargeVfx = EffectManager.GetAndActivatePooledEffect(chargeVfxPrefab, muzzleTransform.position, muzzleTransform.rotation);
                    chargeVfxInstance = _emh_chargeVfx.gameObject;
                }
                chargeVfxInstance.transform.parent = muzzleTransform;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            inputBank.moveVector = moveDirection;
            if (!groundedAttack || targetBody == null)
            {
                inputBank.aimDirection = (moveDirection + Vector3.down).normalized;
            }
            characterDirection.moveVector = moveDirection;
            if (groundedAttack && targetBody != null)
            {
                Vector3 toTargetBody = (targetBody.corePosition - GetAimRay().origin).normalized;
                attackDirection = Vector3.RotateTowards(attackDirection, toTargetBody, 0.005f, 0f);
                inputBank.aimDirection = attackDirection;
            }
            if (!groundedAttack)
            {
                ApplyVelocity();
                if (modelAnimator != null && modelAnimator.GetFloat("flameEndFlapTrigger") > 0.6f && !playedEndFlap)
                {
                    Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
                    playedEndFlap = true;
                }
            }
            if (base.fixedAge > windupDuration && attackState == FlamebreathState.Windup)
            {
                attackState = FlamebreathState.Attack;
                PlayCrossfade("Body", groundedAttack ? "GroundedFlamebreathLoop" : "Flamebreath", 0.1f);
                if (!EffectManager.ShouldUsePooledEffect(ClonedAssets.SkyDraconFlamebreathEffect))
                {
                    flamebreathInstance = UnityEngine.Object.Instantiate(ClonedAssets.SkyDraconFlamebreathEffect, muzzleTransform).transform;
                }
                else
                {
                    _emh_flamebreathEffectInstance = EffectManager.GetAndActivatePooledEffect(ClonedAssets.SkyDraconFlamebreathEffect, muzzleTransform, inResetLocal: true);
                    flamebreathInstance = _emh_flamebreathEffectInstance.gameObject.transform;
                }
                flamebreathInstance.localScale = Vector3.one;
                flamebreathInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = attackDuration;
                Util.PlaySound("Play_lemurianBruiser_m2_loop", characterBody.gameObject);
                DestroyChargeEffect();
            }
            if (base.fixedAge > windupDuration + attackDuration && attackState == FlamebreathState.Attack)
            {
                PlayCrossfade("Body", groundedAttack ? "GroundedFlamebreathEnd" : "FlamebreathEnd", 0.1f);
                if (!groundedAttack)
                {
                    Util.PlaySound("Play_skyDracon_Flap", characterBody.gameObject);
                }
                attackState = FlamebreathState.Exit;
                DestroyFlamebreathEffect();
                Util.PlaySound("Stop_lemurianBruiser_m2_loop", characterBody.gameObject);
            }
            if (attackState == FlamebreathState.Attack)
            {
                tickTimer -= Time.fixedDeltaTime;
                if (tickTimer < 0)
                {
                    tickTimer += tickInterval;
                    FireAttack();
                }
            }
            if (flamebreathInstance != null)
            {
                flamebreathInstance.transform.forward = attackDirection;
            }
            if (base.fixedAge > totalDuration)
            {
                outer.SetNextStateToMain();
            }
        }
        private void FireAttack()
        {
            BulletAttack bulletAttack = new BulletAttack();
            bulletAttack.owner = base.gameObject;
            bulletAttack.weapon = base.gameObject;
            bulletAttack.origin = muzzleTransform.position;
            bulletAttack.aimVector = attackDirection;
            bulletAttack.minSpread = 0f;
            bulletAttack.maxSpread = 0f;
            bulletAttack.damage = perTickDamageCoefficient * damageStat;
            bulletAttack.force = 0f;
            bulletAttack.isCrit = RollCrit();
            bulletAttack.hitEffectPrefab = impactEffect;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.stopperMask = LayerIndex.world.mask;
            bulletAttack.procCoefficient = 0.5f;
            bulletAttack.maxDistance = 35f;
            bulletAttack.smartCollision = true;
            bulletAttack.damageType = new DamageTypeCombo { damageType = DamageType.IgniteOnHit, damageSource = DamageSource.Primary };
            bulletAttack.radius = 2.5f;
            bulletAttack.Fire();
        }
        private void ApplyVelocity()
        {
            if (characterMotor == null)
            {
                return;
            }
            if (attackState == FlamebreathState.Windup)
            {
                float clamp = Mathf.Clamp01(base.fixedAge / windupDuration);
                float modifier = Mathf.Pow(clamp, 2f);
                characterMotor.velocity = moveDirection * flightSpeed * modifier;

            }
            if (attackState == FlamebreathState.Attack)
            {
                characterMotor.velocity = moveDirection * flightSpeed;
            }
            if (attackState == FlamebreathState.Exit)
            {
                float clamp = 1f - Mathf.Clamp01((base.fixedAge - windupDuration - attackDuration) / exitDuration);
                float modifier = Mathf.Pow(clamp, 2f);
                characterMotor.velocity = moveDirection * flightSpeed * modifier;
            }
        }
        private void DestroyFlamebreathEffect()
        {
            if (flamebreathInstance != null)
            {
                if (_emh_flamebreathEffectInstance != null && _emh_flamebreathEffectInstance.OwningPool != null)
                {
                    _emh_flamebreathEffectInstance.OwningPool.ReturnObject(_emh_flamebreathEffectInstance);
                }
                else
                {
                    EntityState.Destroy(flamebreathInstance.gameObject);
                }
                flamebreathInstance = null;
                _emh_flamebreathEffectInstance = null;
            }
        }
        private void DestroyChargeEffect()
        {
            if (chargeVfxInstance != null)
            {
                if (_emh_chargeVfx != null && _emh_chargeVfx.OwningPool != null)
                {
                    _emh_chargeVfx.OwningPool.ReturnObject(_emh_chargeVfx);
                }
                else
                {
                    EntityState.Destroy(chargeVfxInstance);
                }
                chargeVfxInstance = null;
                _emh_chargeVfx = null;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            DestroyFlamebreathEffect();
            DestroyChargeEffect();
            Util.PlaySound("Stop_lemurianBruiser_m2_loop", characterBody.gameObject);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
