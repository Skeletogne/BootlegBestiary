using BepInEx.Configuration;
using BootlegBestiary.Shared;
using BootlegBestiary.Shared.Assets;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.SkyDracon.EntityStates;
using EntityStates;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;


namespace BootlegBestiary.SkyDracon
{
    [BootlegBestiary.ModuleInfo("Enable Enemy", "Adds Marrowings to the game.", "Marrowing", true)]
    public class SkyDraconSetup : SetupModule
    {
        private CharacterBody draconBody;
        private static ConfigEntry<bool> replaceMonsters { get; set; }
        public override void RegisterConfig()
        {
            base.RegisterConfig();
            BindStats(CustomAssets.SkyDraconBodyPrefab, [CustomAssets.SkyDraconSpawnCard]);
            replaceMonsters = BindBool("Replace Enemies", true, "Causes the Marrowing to replace:\nScorch Worms on Iron Alluvium and Auroras\nSolus Extractors on Abyssal Depths\nSolus Scorchers on Helminth Hatchery");
        }
        public override void Initialise()
        {
            base.Initialise();
            GameObject skyDraconFlamebreathEffect = ClonedAssets.SkyDraconFlamebreathEffect;
            GameObject skyDraconBodyPrefab = CustomAssets.SkyDraconBodyPrefab;
            GameObject skyDraconMasterPrefab = CustomAssets.SkyDraconMasterPrefab;
            CharacterSpawnCard cscSkyDracon = CustomAssets.SkyDraconSpawnCard;

            PrefabAPI.RegisterNetworkPrefab(skyDraconBodyPrefab);
            PrefabAPI.RegisterNetworkPrefab(skyDraconMasterPrefab);

            //EnemyPrefabValidator.ValidateAssets(skyDraconBodyPrefab, skyDraconMasterPrefab, cscSkyDracon);

            skyDraconBodyPrefab.AddComponent<SkyDraconBehaviourController>();
            draconBody = skyDraconBodyPrefab.GetComponent<CharacterBody>();

            HurtBoxLayersToEntityPrecise(skyDraconBodyPrefab);
            string enemyName = "Marrowing";
            AddNameToken(skyDraconBodyPrefab, enemyName);
            AddLoreToken(skyDraconBodyPrefab, CreateLoreToken());
            SetDeathState(skyDraconBodyPrefab, typeof(Death), true);
            ConfigureESM(skyDraconBodyPrefab, "Body", typeof(SkyDraconCharacterMain), true, typeof(Spawn), true);
            ConfigureESM(skyDraconBodyPrefab, "ControlFlight", typeof(Flight), true, typeof(Flight), false);

            ContentAddition.AddBody(skyDraconBodyPrefab);
            ContentAddition.AddMaster(skyDraconMasterPrefab);

            DirectorAPI.MonsterCategory minibossCategory = DirectorAPI.MonsterCategory.Minibosses;
            DirectorCore.MonsterSpawnDistance standardDistance = DirectorCore.MonsterSpawnDistance.Standard;

            //base
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 1, DirectorAPI.Stage.DistantRoost);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.AbandonedAqueduct);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.IronAlluvium);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.IronAuroras);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.AbyssalDepths);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.HelminthHatchery);

            //simulacrum
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 1, DirectorAPI.Stage.AbandonedAqueductSimulacrum);
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 1, DirectorAPI.Stage.AbyssalDepthsSimulacrum);

            //modded
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.AncientObservatoryString);

            //modded simulacrum
            AddCardToMap(cscSkyDracon, minibossCategory, 1, standardDistance, false, 1, DirectorAPI.Stage.Custom, ModdedAssets.ITAncientObservatoryString);

            if (replaceMonsters.Value)
            {
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscScorchling", DirectorAPI.Stage.IronAlluvium);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscScorchling", DirectorAPI.Stage.IronAuroras);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscExtractorUnit", DirectorAPI.Stage.AbyssalDepths);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscTanker", DirectorAPI.Stage.HelminthHatchery);
            }

            SkillDefData primaryData = new SkillDefData
            {
                objectName = "SkyDraconBodyFlamebreath",
                skillName = "SkyDraconFlamebreath",
                esmName = "Body",
                activationState = ContentAddition.AddEntityState<Flamebreath>(out _),
                cooldown = 4f,
                cdOnEnd = true,
                intPrio = InterruptPriority.Any,
                combatSkill = true
            };
            var flamebreath = CreateSkillDef<DraconFlamebreathAndDiveSkillDef>(primaryData);
            CreateGenericSkill(skyDraconBodyPrefab, flamebreath, "SkyDraconPrimaryFamily", SkillSlot.Primary);
            SkillDefData secondaryData = new SkillDefData
            {
                objectName = "SkyDraconBodyDiveBomb",
                skillName = "SkyDraconDiveBomb",
                esmName = "Body",
                activationState = ContentAddition.AddEntityState<DiveBomb>(out _),
                cooldown = 15f,
                combatSkill = true,
                intPrio = InterruptPriority.Any
            };
            var divebomb = CreateSkillDef<DraconFlamebreathAndDiveSkillDef>(secondaryData);
            CreateGenericSkill(skyDraconBodyPrefab, divebomb, "SkyDraconSecondaryFamily", SkillSlot.Secondary);
            SkillDefData utilityData = new SkillDefData
            {
                objectName = "SkyDraconBodyEmerge",
                skillName = "SkyDraconEmerge",
                esmName = "Body",
                activationState = ContentAddition.AddEntityState<BurrowEmerge>(out _),
                cooldown = 6f,
                combatSkill = true,
                intPrio = InterruptPriority.Any
            };
            var emerge = CreateSkillDef<DraconEmergeSkillDef>(utilityData);
            CreateGenericSkill(skyDraconBodyPrefab, emerge, "SkyDraconUtilityFamily", SkillSlot.Utility);
            RegisterLooseEntityStates([typeof(Squirm), typeof(BurrowRelocate), typeof(Stun), typeof(FlyingStun)]);
            SetupFlamebreathPrefab();
            On.RoR2.EntityStateMachine.SetInterruptState += SetDraconStunState;
            SetupLogbookEntry(skyDraconBodyPrefab, "UnlockableSkyDraconLog", "SKELETOGNE_UNLOCKABLE_LOG_SKYDRACON", enemyName);

            ItemDisplayRuleSet idrs = CustomAssets.SkyDraconItemDisplayRuleset;

            SetUpEliteAffix(idrs, VanillaAssets.EDEliteFire,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixFire,
                    localPos = new Vector3(0.6F, 1.26F, -1.14F),
                    localAngles = new Vector3(0F, 0F, 315F),
                    localScale = new Vector3(1.2F, 1.2F, 1.2F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixFire,
                    localPos = new Vector3(-0.6F, 1.26F, -1.14F),
                    localAngles = new Vector3(0F, 0F, 45F),
                    localScale = new Vector3(-1.2F, 1.2F, 1.2F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteLightning,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning,
                    localPos = new Vector3(0F, 0.83514F, -2.05429F),
                    localAngles = new Vector3(41.17938F, 180F, 180F),
                    localScale = new Vector3(3F, 3F, 3F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning,
                    localPos = new Vector3(0F, 1.7556F, -1.87802F),
                    localAngles = new Vector3(359.0717F, 180F, 180F),
                    localScale = new Vector3(2F, 2F, 2F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteIce,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixIce,
                    localPos = new Vector3(0F, -0.11602F, -3.22486F),
                    localAngles = new Vector3(342.0648F, 0F, 180F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteEarth,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixEarth,
                    localPos = new Vector3(-0.08931F, 1.62895F, -1.40961F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(7F, 7F, 7F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteAurelionite,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixAurelionite,
                    localPos = new Vector3(0F, 1.29796F, -1.89872F),
                    localAngles = new Vector3(300.7701F, 180F, 180F),
                    localScale = new Vector3(5F, 5F, 5F)

                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteVoid,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixVoid,
                    localPos = new Vector3(0F, 2.18085F, -0.24028F),
                    localAngles = new Vector3(290.697F, 0F, -0.00002F),
                    localScale = new Vector3(2.5F, 2.5F, 2.5F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDElitePoison,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixPoison,
                    localPos = new Vector3(0F, 0.64402F, -1.16346F),
                    localAngles = new Vector3(38.47652F, 180F, 180F),
                    localScale = new Vector3(0.6F, 0.6F, 1F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteHaunted,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixHaunted,
                    localPos = new Vector3(0F, -0.08929F, -2.40165F),
                    localAngles = new Vector3(338.0297F, 0F, 180F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteBead,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixBead,
                    localPos = new Vector3(-0.00036F, 0.54291F, -1.55782F),
                    localAngles = new Vector3(291.9088F, 169.829F, 173.293F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)

                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteCollective,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective,
                    localPos = new Vector3(0.65F, -0.25F, -2.14F),
                    localAngles = new Vector3(20.40761F, 322F, 20F),
                    localScale = new Vector3(2.8F, 2.8F, 2.8F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective,
                    localPos = new Vector3(-0.65F, -0.25F, -2.14F),
                    localAngles = new Vector3(340F, 222F, 20.00001F),
                    localScale = new Vector3(2.8F, 2.8F, -2.8F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollectiveRing,
                    localPos = new Vector3(0F, 0.03604F, -0.00557F),
                    localAngles = new Vector3(270F, -0.00003F, 0F),
                    localScale = new Vector3(5F, 5F, 5F)
                }]);
        }
        public string CreateLoreToken()
        {
            //down here for easier readability
            string loreToken = "The water lasted two days after my exile.\n\nI had a lot of time to contemplate. On how the dunepeople weren't afraid. Not of the shrieks, not of the flames, not of my warnings.\n\nSoundly, they slept through nights where I tossed and turned, kept awake by those otherworldly calls.\n\nWhen I encountered one of the hell-beasts in the wild, when it erupted from the sand below, adrenaline overtook me.\n\nI saw a demon-skull and blood-red scales. I didn't hesitate. It wasn't the first beast I'd had to put down on this miserable planet. I told myself it wouldn't be the last.\n\n\"They're coming!\" I yelled. \"There's dozens of them out there!\" I warned. But none listened. The dunefolk stopped talking to me. Wouldn't meet my eyeline. Not long after, the chief painted a tar-black mark on my forehead. Cursed. Exiled. The guards escorted me to the walls, weapons drawn.\n\nHow was I supposed to know that killing one of those horrid things was some ridiculous cultural taboo?\n\n----------------------------------------\n\nThe mark burns. My skin burns. Everything burns. I dream of cold water and wake to a dry mouth and a screaming sky.\n\nI can hear their shrieks. Above and Below. Following. Patient. Circling.\n\n----------------------------------------\n\nI... don't think the curse is superstition.";
            return loreToken;
        }
        public void SetupFlamebreathPrefab()
        {
            ParticleSystem[] particleSystems = ClonedAssets.SkyDraconFlamebreathEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.transform.localScale = new Vector3(1.8f, 1.8f, 4.5f);
                particleSystem.transform.localPosition = new Vector3(0f, 0f, 16f);
            }
        }
        private bool SetDraconStunState(On.RoR2.EntityStateMachine.orig_SetInterruptState orig, EntityStateMachine self, EntityState newNextState, InterruptPriority interruptPriority)
        {
            bool success = orig(self, newNextState, interruptPriority);
            if (success)
            {
                if (self.commonComponents.characterBody != null)
                {
                    CharacterBody body = self.commonComponents.characterBody;
                    if (body.bodyIndex == draconBody.bodyIndex && newNextState is StunState)
                    {
                        self.SetNextState(new Stun());
                    }
                }
            }
            return success;
        }
        public class DraconFlamebreathAndDiveSkillDef : SkillDef
        {
            private class InstanceData : BaseSkillInstanceData
            {
                public SkyDraconBehaviourController controller;
            }
            public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
            {
                return new InstanceData
                {
                    controller = skillSlot.characterBody.gameObject.GetComponent<SkyDraconBehaviourController>()
                };
            }
            public override bool IsReady([NotNull] GenericSkill skillSlot)
            {
                InstanceData data = skillSlot.skillInstanceData as InstanceData;
                if (data?.controller == null)
                {
                    return false;
                }
                return (data?.controller.draconState == SkyDraconBehaviourController.DraconState.Grounded || data.controller.canUseSkills) && base.IsReady(skillSlot);
            }
        }
        public class DraconEmergeSkillDef : SkillDef
        {
            private class InstanceData : BaseSkillInstanceData
            {
                public SkyDraconBehaviourController controller;
            }
            public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
            {
                return new InstanceData
                {
                    controller = skillSlot.characterBody.gameObject.GetComponent<SkyDraconBehaviourController>()
                };
            }
            public override bool IsReady([NotNull] GenericSkill skillSlot)
            {
                InstanceData data = skillSlot.skillInstanceData as InstanceData;
                if (data?.controller == null)
                {
                    return false;
                }
                return (data?.controller.draconState == SkyDraconBehaviourController.DraconState.Grounded) && base.IsReady(skillSlot);
            }
        }
    }
}
