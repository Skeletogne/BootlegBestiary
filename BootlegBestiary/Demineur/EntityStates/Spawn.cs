using EntityStates;
using UnityEngine;
using RoR2;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.Demineur.EntityStates
{
   public class Spawn : BaseState
    {
        private static float duration = 2.5f;
        private static string spawnSfxString = "Play_demineur_spawn";
        public override void OnEnter()
        {
            base.OnEnter();
            Animator animator = GetModelAnimator();
            PlayAnimation("Body", "Spawn1");
            Util.PlaySound(spawnSfxString, characterBody.gameObject);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
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
