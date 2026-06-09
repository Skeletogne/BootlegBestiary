using System;
using System.Collections.Generic;
using System.Text;
using BootlegBestiary.Shared.Assets;
using EntityStates;
using R2API;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace BootlegBestiary.Demineur.EntityStates
{
    public class InkAttack : BaseSkillState
    {
        private static float baseDuration = 3f;
        private static float baseAttackStartTime = 1f;
        private static float durationUntilJump = 1f;
        private static float baseAttackEndTime = 2f;
        private static GameObject hitEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Common_VFX.OmniImpactVFXSlash_prefab).WaitForCompletion();
        private static GameObject explosion = Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_ClayGrenadier.ClayGrenadierBarrelExplosion_prefab).WaitForCompletion();
        private static string chargeSfxString = "Play_demineur_attack2_charge";
        private static string stopChargeSfxString = "Stop_demineur_attack2_charge";
        private enum SkillState
        {
            Start,
            Attack,
            End
        }
        private bool jumpApplied;
        private SkillState skillState;
        private OverlapAttack attack;
        private Transform muzzleTransform;
        private GameObject effectInstance;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(chargeSfxString, characterBody.gameObject);
            PlayAnimation("Gesture, Override", "InkAttack");
            skillState = SkillState.Start;
            characterMotor.walkSpeedPenaltyCoefficient = 0f;
            attack = new OverlapAttack();
            attack.attacker = characterBody.gameObject;
            attack.inflictor = characterBody.gameObject;
            attack.teamIndex = teamComponent.teamIndex;
            attack.damage = 0.5f * damageStat;
            attack.hitEffectPrefab = hitEffect;
            attack.isCrit = RollCrit();
            attack.procCoefficient = 1f;
            DamageTypeCombo combo = new DamageTypeCombo { damageType = DamageType.Generic, damageSource = DamageSource.Secondary };
            attack.hitBoxGroup = FindHitBoxGroup("arms");
            ChildLocator locator = GetModelChildLocator();
            if (locator != null)
            {
                muzzleTransform = locator.FindChild("InkSprayOrigin");
            }
            effectInstance = Object.Instantiate(CustomAssets.InkSprayEffect, muzzleTransform.position, Quaternion.identity);
            effectInstance.transform.parent = muzzleTransform;
            characterBody.SetBuffCount(DemineurSetup.hiddenJumpCountDebuff.buffIndex, 1);
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound(stopChargeSfxString, characterBody.gameObject);
            PlayAnimation("Gesture, Override", "BufferEmpty");
            characterBody.SetBuffCount(DemineurSetup.hiddenJumpCountDebuff.buffIndex, 0);
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > baseAttackStartTime && skillState == SkillState.Start)
            {
                Util.PlaySound(stopChargeSfxString, characterBody.gameObject);
                characterMotor.walkSpeedPenaltyCoefficient = 1f;
                skillState = SkillState.Attack;
                DamageTypeCombo combo = new DamageTypeCombo { damageType = DamageType.Generic, damageSource = DamageSource.Secondary };
                combo.AddModdedDamageType(DemineurSetup.inkDamageType);
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = characterBody.footPosition;
                blastAttack.radius = 8f;
                blastAttack.inflictor = characterBody.gameObject;
                blastAttack.attacker = characterBody.gameObject;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.procCoefficient = 1f;
                blastAttack.procChainMask = default;
                blastAttack.baseDamage = 2.75f * damageStat;
                blastAttack.baseForce = 1000f;
                blastAttack.crit = RollCrit();
                blastAttack.damageColorIndex = DamageColorIndex.DeathMark;
                blastAttack.teamIndex = teamComponent.teamIndex;
                blastAttack.damageType = combo;
                blastAttack.Fire();
                EffectManager.SpawnEffect(explosion, new EffectData { origin = characterBody.footPosition, scale = 8f }, true);
                effectInstance.transform.parent = null;
            }
            if (skillState == SkillState.Attack)
            {
                attack.Fire();
            }
            if (base.fixedAge > durationUntilJump && !jumpApplied)
            {
                jumpApplied = true;
                characterMotor.velocity = 25f * Vector3.up;
                characterMotor.Motor.ForceUnground();
            }
            if (base.fixedAge > baseAttackEndTime && skillState == SkillState.Attack)
            {
                skillState = SkillState.End;
            }
            if (base.fixedAge > baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}
