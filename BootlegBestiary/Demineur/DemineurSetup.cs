using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using BootlegBestiary.Demineur.EntityStates;
using BootlegBestiary.Shared;
using BootlegBestiary.Shared.Assets;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using static R2API.DamageAPI;

namespace BootlegBestiary.Demineur
{
    [BootlegBestiary.ModuleInfo("Enable Enemy", "Adds Bathylopods to the game.", "Bathylopod", true)]
    public class DemineurSetup : SetupModule
    {
        private CharacterBody demineurBody;
        public static ModdedDamageType inkDamageType;
        public static BuffDef hiddenJumpCountDebuff;
        private static ConfigEntry<bool> replaceMonsters { get; set; }
        public static ConfigEntry<bool> inkCausesBlindness { get; set; }
        public override void RegisterConfig()
        {
            base.RegisterConfig();
            BindStats(CustomAssets.DemineurBodyPrefab, [CustomAssets.DemineurSpawnCard]);
            replaceMonsters = BindBool("Replace Enemies", true, "Causes the Bathylopod to replace:\nSolus Scorchers on Verdant and Viscous Falls\nBeetles on Wetland Aspect\nSolus Prospectors on Sulfur Pools.");
            inkCausesBlindness = BindBool("Ink Causes Blindness", true, "Causes the Bathylopod to apply blindness for 4 seconds with it's secondary attack. If disabled, instead applies tar for 4 seconds.");
        }
        public override void Initialise()
        {
            inkDamageType = ReserveDamageType();

            GameObject inkSprayEffect = CustomAssets.InkSprayEffect;
            ContentAddition.AddEffect(inkSprayEffect);
            if (inkSprayEffect != null)
            {
                ParticleSystemRenderer renderer = inkSprayEffect.GetComponentInChildren<ParticleSystemRenderer>();
                renderer.sharedMaterial = VanillaAssets.InkMaterial;
            }
            GlobalEventManager.onServerDamageDealt += ApplyInkDamageType;

            GameObject demineurBodyPrefab = CustomAssets.DemineurBodyPrefab;
            GameObject demineurMasterPrefab = CustomAssets.DemineurMasterPrefab;
            CharacterSpawnCard cscDemineur = CustomAssets.DemineurSpawnCard;

            base.Initialise();
            PrefabAPI.RegisterNetworkPrefab(demineurBodyPrefab);
            PrefabAPI.RegisterNetworkPrefab(demineurMasterPrefab);

            demineurBody = demineurBodyPrefab.GetComponent<CharacterBody>();
            HurtBoxLayersToEntityPrecise(demineurBodyPrefab);

            string enemyName = "Bathylopod";
            AddNameToken(demineurBodyPrefab, enemyName);
            AddLoreToken(demineurBodyPrefab, CreateLoreToken());

            ConfigureESM(demineurBodyPrefab, "Body", typeof(DemineurCharacterMain), true, typeof(Spawn), true);
            ConfigureESM(demineurBodyPrefab, "Weapon", typeof(Idle), false, typeof(Idle), false);
            SetDeathState(demineurBodyPrefab, typeof(Death), true);

            ContentAddition.AddBody(demineurBodyPrefab);
            ContentAddition.AddMaster(demineurMasterPrefab);

            DirectorCore.MonsterSpawnDistance standardDistance = DirectorCore.MonsterSpawnDistance.Standard;
            DirectorAPI.MonsterCategory basicCategory = DirectorAPI.MonsterCategory.BasicMonsters;
            //base
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.VerdantFalls);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.ViscousFalls);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.WetlandAspect);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.SulfurPools);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.GildedCoast);

            //simulacrum
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.SkyMeadowSimulacrum);

            //modded
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.SunkenTombsString);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.WetlandDownpourString);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.FogboundLagoonString);

            //modded simulacrum
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.ITWetlandDownpourString);
            AddCardToMap(cscDemineur, basicCategory, 1, standardDistance, false, 0, DirectorAPI.Stage.Custom, ModdedAssets.ITSunkenTombsString);

            //needs to match internal spawn card name
            if (replaceMonsters.Value)
            {
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscTanker", DirectorAPI.Stage.VerdantFalls);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscTanker", DirectorAPI.Stage.ViscousFalls);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscBeetle", DirectorAPI.Stage.WetlandAspect);
                DirectorAPI.Helpers.RemoveExistingMonsterFromStage("cscWorkerUnit", DirectorAPI.Stage.SulfurPools);
            }

            SetupLogbookEntry(demineurBodyPrefab, "UnlockableDemineurLog", "SKELETOGNE_UNLOCKABLE_LOG_DEMINEUR", enemyName);

            SkillDefData primaryData = new SkillDefData
            {
                objectName = "DemineurBodyTentacleAttack",
                skillName = "DemineurTentacleAttack",
                esmName = "Weapon",
                activationState = ContentAddition.AddEntityState<TentacleAttack>(out _),
                cooldown = 5f,
                intPrio = InterruptPriority.Any,
                combatSkill = true
            };
            var tentacleAttack = CreateSkillDef<SkillDef>(primaryData);
            CreateGenericSkill(demineurBodyPrefab, tentacleAttack, "DemineurPrimaryFamily", SkillSlot.Primary);

            SkillDefData secondaryData = new SkillDefData
            {
                objectName = "DemineurBodyInkAttack",
                skillName = "DemineurInkAttack",
                esmName = "Weapon",
                activationState = ContentAddition.AddEntityState<InkAttack>(out _),
                cooldown = 3f,
                intPrio = InterruptPriority.Any,
                combatSkill = true
            };
            var inkAttack = CreateSkillDef<SkillDef>(secondaryData);
            CreateGenericSkill(demineurBodyPrefab, inkAttack, "DemineurSecondaryFamily", SkillSlot.Secondary);

            hiddenJumpCountDebuff = ScriptableObject.CreateInstance<BuffDef>();
            hiddenJumpCountDebuff.isDebuff = false;
            hiddenJumpCountDebuff.ignoreGrowthNectar = true;
            hiddenJumpCountDebuff.isHidden = true;
            ContentAddition.AddBuffDef(hiddenJumpCountDebuff);

            RecalculateStatsAPI.GetStatCoefficients += DisableJumpsDuringSkills;


            ItemDisplayRuleSet idrs = CustomAssets.DemineurItemDisplayRuleset;
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteFire,
            [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixFire,
                    localPos = new Vector3(0.6F, 0.27F, 0.12F),
                    localAngles = new Vector3(0F, 0F, 315F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixFire,
                    localPos = new Vector3(-0.6F, 0.27F, 0.12F),
                    localAngles = new Vector3(0F, 0F, 225F),
                    localScale = new Vector3(0.5F, -0.5F, 0.5F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteLightning,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning,
                    localPos = new Vector3(0F, 0.45F, -0.8F),
                    localAngles = new Vector3(353F, 180F, 180F),
                    localScale = new Vector3(0.9F, 0.9F, 0.9F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning,
                    localPos = new Vector3(0F, 0.28F, -0.87F),
                    localAngles = new Vector3(20F, 180F, 180F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteIce,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixIce,
                    localPos = new Vector3(0F, -0.3F, -0.55F),
                    localAngles = new Vector3(22F, 180F, 180F),
                    localScale = new Vector3(0.17F, 0.17F, 0.17F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteEarth,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixEarth,
                    localPos = new Vector3(0F, 0.34F, 0.1F),
                    localAngles = new Vector3(275F, 0F, 0F),
                    localScale = new Vector3(3.5F, 3.5F, 3.5F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteAurelionite,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixAurelionite,
                    localPos = new Vector3(0F, 0.48F, -0.025F),
                    localAngles = new Vector3(285F, 180F, 180F),
                    localScale = new Vector3(2F, 2.5F, 1.4F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteVoid,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixVoid,
                    localPos = new Vector3(0.48F, -0.065F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F),
                    childName = "Porthole"
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDElitePoison,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixPoison,
                    localPos = new Vector3(0F, 0.1F, -0.48F),
                    localAngles = new Vector3(0F, 180F, 0F),
                    localScale = new Vector3(0.2F, 0.2F, 0.4F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteHaunted,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixHaunted,
                    localPos = new Vector3(0F, 0F, -0.5F),
                    localAngles = new Vector3(12F, 180F, 180F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteBead,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixBead,
                    localPos = new Vector3(0.1F, 0.16F, -0.66F),
                    localAngles = new Vector3(270F, 345F, 0F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)

                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteCollective,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective,
                    localPos = new Vector3(-0.40742F, -0.55971F, -0.30653F),
                    localAngles = new Vector3(0F, 323.8561F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective,
                    localPos = new Vector3(0.40577F, -0.49682F, -0.35082F),
                    localAngles = new Vector3(-0.00001F, 35.47788F, 0F),
                    localScale = new Vector3(-1F, 1F, 1F)
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollectiveRing,
                    localPos = new Vector3(0F, 0.4F, 0F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }]);

            On.RoR2.PostProcessing.VisionLimitEffect.LateUpdate += AdjustVisionCurve;
        }

        private void AdjustVisionCurve(On.RoR2.PostProcessing.VisionLimitEffect.orig_LateUpdate orig, RoR2.PostProcessing.VisionLimitEffect self)
        {
            CameraRigController cameraRigController = self.cameraRigController;
            Transform transform = (cameraRigController.target ? cameraRigController.target.transform : null);
            CharacterBody targetBody = cameraRigController.targetBody;
            if ((bool)transform)
            {
                self.lastKnownTargetPosition = transform.position;
            }
            self.desiredVisionDistance = (targetBody ? targetBody.visionDistance : float.PositiveInfinity);
            float target = 0f;
            float target2 = 4000f;
            if (self.desiredVisionDistance != float.PositiveInfinity)
            {
                target = 1f;
                target2 = self.desiredVisionDistance;
            }
            if (self.desiredVisionDistance < self.currentVisionDistance)
            {
                self.currentAlpha = Mathf.SmoothDamp(self.currentAlpha, target, ref self.alphaVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
                self.currentVisionDistance = Mathf.SmoothDamp(self.currentVisionDistance, target2, ref self.currentVisionDistanceVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
            }
            else
            {
                self.currentAlpha = Mathf.SmoothDamp(self.currentAlpha, target, ref self.alphaVelocity, 3f, float.PositiveInfinity, Time.deltaTime);
                self.currentVisionDistance = Mathf.SmoothDamp(self.currentVisionDistance, target2, ref self.currentVisionDistanceVelocity, 3f, float.PositiveInfinity, Time.deltaTime);
            }
        }

        private void DisableJumpsDuringSkills(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(hiddenJumpCountDebuff))
            {
                args.jumpCountMult = 0;
            }
        }
        private void ApplyInkDamageType(DamageReport report)
        {
            if (report != null && report.victimBody != null && report.damageInfo != null && report.damageInfo.damageType.HasModdedDamageType(inkDamageType))
            {
                if (inkCausesBlindness.Value)
                {
                    report.victimBody.AddTimedBuff(DLC1Content.Buffs.Blinded, 4f);
                }
                else
                {
                    report.victimBody.AddTimedBuff(RoR2Content.Buffs.ClayGoo, 4f);
                }
            }
        }
        public string CreateLoreToken()
        {
            string loreToken = "//-- UES [REDACTED], OBSERVED FLORA/FAUNA DATABASE INITIALIZING --\r\n\r\n>> ENTER COMMAND...\r\n> new_entry\r\n\r\n>> ADMINISTRATOR LOGIN REQUIRED\r\n> username: F_Tamaki\r\n> password: **************\r\n\r\n>> LOGIN SUCCESSFUL.\r\n>> PLEASE PROCEED WITH NEW DATABASE ENTRY\r\n\r\n>> ENTER COMMON NAME...\r\n> Bathylopod\r\n\r\n>> ENTER SPECIES CLASSIFICATION...\r\n> Holcodiscus Galea\r\n\r\n>> WARNING: GENUS MATCHES EXISTING ENTRIES FROM PLANET: EARTH. PROCEED WITH CLASSIFICATION? Y/N...\r\n> Y \r\n\r\n>> ENTER DESCRIPTION\r\n> Specimen recovered by Planet-side crews. I was presented with a diving helmet containing the corpse of the creature, and cracked remains of an evolute mollusc shell. Shell presumed to have broken upon death, from the diving helmet impacting the ground. Looking up fossil matches from back home confirmed my suspicions. The structure, internally and externally, matches the Ammonitida Order, specifically Holcodiscidae. How it wound up here on the planet is far beyond my paygrade to speculate upon, but I suspect it might relate back to similar creatures encountered in Contact Light database records...\r\n\r\n>> ENTER BEHAVIORAL TRAITS...\r\n> Presents as a diving helmet with tentacles hanging beneath, capable of floating approximately 2m above ground level. My investigations into the creature's biology match observations made by Contact Light crews, that this creature floats using a constant stream of gas created from within the shell. Creature is hostile to humans and other forms of life, and presumed carnivorous by its stomach contents. The diving helmet does not appear to be behaviorally related to intentional mimicry or deception by the creature, but rather as a convenient additional means of defense to its fragile shell and vital organs. Creature's shell had conformed to the inner walls of the helmet, suggesting it had inhabited it for an extended period, growing into and past the spatial limits the helmet presented. Similar to a hermit crab, but this specimen had seemingly trapped itself within the helmet of its own accord. Strange... \r\nEntry will be updated when we have further specimens to research.\r\n\r\n>> ENTRY COMPLETE. REGISTERING TO DATABASE...\r\n>> WARNING: ENTRY FLAGGED FOR REVIEW.\r\n>> LOGGING OUT.";
            return loreToken;
        }
    }
}
