using EntityStates;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BootlegBestiary.Demineur.EntityStates
{
    public class Death : GenericCharacterDeath
    {
        private static string deathSfxString = "Play_demineur_death";
        private static GameObject deathVFX = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Jellyfish.JellyfishDeath_prefab).WaitForCompletion();
        private static GameObject helmetImpactVFX = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Bell.BellBodyPartsImpact_prefab).WaitForCompletion();
        private static float helmetImpactTime = 1.2f;
        private bool helmetImpact = false;
        private Transform headTransform;
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(deathVFX, new EffectData { origin = characterBody.corePosition, scale = 1f }, transmit: true);
            Util.PlaySound(deathSfxString, characterBody.gameObject);
            if (characterMotor != null)
            {
                characterMotor.velocity = Vector3.zero;
            }
            ChildLocator locator = GetModelChildLocator();
            headTransform = locator.FindChild("Head");
            if (headTransform != null)
            {
                
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > helmetImpactTime && !helmetImpact)
            {
                helmetImpact = true;
                EffectManager.SpawnEffect(helmetImpactVFX, new EffectData { origin = headTransform.position, scale = 1f }, transmit: true);
            }
        }
    }
}
