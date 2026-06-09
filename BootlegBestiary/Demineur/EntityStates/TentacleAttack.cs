using EntityStates;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BootlegBestiary.Demineur.EntityStates
{
    public class TentacleAttack : BaseSkillState
    {
        private static float baseDuration = 4f;
        private static float baseAttackStartTime = 1.25f;
        private static float baseAttackEndTime = 2.92f;
        private static GameObject hitEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Common_VFX.OmniImpactVFXSlash_prefab).WaitForCompletion();
        private static string chargeSfxString = "Play_demineur_attack1_charge";
        private static string stopChargeSfxString = "Stop_demineur_attack1_charge";
        private static string swingSfxString = "Play_demineur_attack1_swing";
        private static string stopSwingSfxString = "Stop_demineur_attack1_swing";
        private static string endSfxString = "Play_demineurAttack1End";
        private enum SkillState
        {
            Start,
            Attack,
            End,
        }
        private SkillState skillState;
        private OverlapAttack attack;
        private static float attackInterval = 0.2f;
        private float attackTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Gesture, Override", "TentacleAttack");
            Util.PlaySound(chargeSfxString, characterBody.gameObject);
            skillState = SkillState.Start;
            characterMotor.walkSpeedPenaltyCoefficient = 0f;
            characterBody.SetBuffCount(DemineurSetup.hiddenJumpCountDebuff.buffIndex, 1);

            attack = new OverlapAttack();
            attack.attacker = characterBody.gameObject;
            attack.inflictor = characterBody.gameObject;
            attack.teamIndex = teamComponent.teamIndex;
            attack.damage = 0.75f * damageStat;
            attack.hitEffectPrefab = hitEffect;
            attack.isCrit = RollCrit();
            attack.procCoefficient = 0.5f;
            DamageTypeCombo combo = new DamageTypeCombo { damageType = DamageType.Generic, damageSource = DamageSource.Primary };
            attack.hitBoxGroup = FindHitBoxGroup("tentacles");
        }
        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("Gesture, Override", "BufferEmpty");
            Util.PlaySound(stopChargeSfxString, characterBody.gameObject);
            Util.PlaySound(stopSwingSfxString, characterBody.gameObject);
            characterBody.SetBuffCount(DemineurSetup.hiddenJumpCountDebuff.buffIndex, 0);
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > baseAttackStartTime && skillState == SkillState.Start)
            {
                skillState = SkillState.Attack;
                Vector3 forward = GetAimRay().direction;
                characterMotor.velocity = forward * 20f;
                characterMotor.walkSpeedPenaltyCoefficient = 1f;
                Util.PlaySound(swingSfxString, characterBody.gameObject);
            }
            if (skillState == SkillState.Attack)
            {
                attackTimer -= Time.fixedDeltaTime;
                if (attackTimer < 0)
                {
                    attackTimer += attackInterval;
                    attack.Fire();
                    attack.ResetIgnoredHealthComponents();
                }
            }
            if (base.fixedAge > baseAttackEndTime && skillState == SkillState.Attack)
            {
                skillState = SkillState.End;
                Util.PlaySound(stopSwingSfxString, characterBody.gameObject);
                Util.PlaySound(endSfxString, characterBody.gameObject);
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
