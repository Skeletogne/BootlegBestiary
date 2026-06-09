using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using BootlegBestiary.Shared.Assets;
using BootlegBestiary.SkyDracon.EntityStates;
using EntityStates;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;
using static R2API.DirectorAPI;
using Object = UnityEngine.Object;

namespace BootlegBestiary.Shared
{
    public class SetupModule : MonoBehaviour
    {
        protected ConfigFile config => BootlegBestiary.Instance.Config;
        private string _section;
        protected string Section
        {
            get
            {
                if (_section == null)
                {
                    BootlegBestiary.ModuleInfoAttribute attribute = GetType().GetCustomAttribute<BootlegBestiary.ModuleInfoAttribute>();
                    _section = attribute?.Section ?? GetType().Name;
                }
                return _section;
            }
        }
        protected class SkillDefData
        {
            public string objectName;
            public string skillName;
            public string esmName;
            public SerializableEntityStateType activationState;
            public float cooldown;
            public int baseMaxStock = 1;
            public int rechargeStock = 1;
            public int requiredStock = 1;
            public int stockToConsume = 1;
            public InterruptPriority intPrio = InterruptPriority.Any;
            public bool resetCdOnUse = false;
            public bool cdOnEnd = false;
            public bool cdBlocked = false;
            public bool combatSkill = true;
        }
        protected class AISkillDriverData
        {
            public GameObject masterPrefab;
            public string customName;
            public SkillSlot skillSlot;
            public float minDistance = 0f;
            public float maxDistance = float.PositiveInfinity;
            public int desiredIndex = 0;
            public float moveInputScale = 1f;
            public AISkillDriver.MovementType movementType;
            public AISkillDriver.AimType aimType;
            public AISkillDriver.TargetType targetType;
            public bool ignoreNodeGraph = false;
            public float maxHealthFraction = float.PositiveInfinity;
            public float minHealthFraction = float.NegativeInfinity;
            public float maxTargetHealthFraction = float.PositiveInfinity;
            public float minTargetHealthFraction = float.NegativeInfinity;
            public bool requireReady = false;
            public SkillDef requiredSkillDef = null;
            public bool activationRequiresAimTargetLoS = false;
            public bool activationRequiresAimConfirmation = false;
            public bool activationRequiresTargetLoS = false;
            public bool selectionRequiresAimTarget = false;
            public bool selectionRequiresOnGround = false;
            public bool selectionRequiresTargetLoS = false;
            public bool selectionRequiresTargetNonFlier = false;
            public int maxTimesSelected = -1;
            public float driverUpdateTimerOverride = -1f;
            public bool noRepeat = false;
            public AISkillDriver nextHighPriorityOverride = null;
            public bool shouldSprint = false;
            public float aimVectorMaxSpeedOverride = -1f;
            public AISkillDriver.ButtonPressType buttonPressType = AISkillDriver.ButtonPressType.Hold;
        }
        internal ConfigEntry<float> baseMaxHealthCfg;
        internal ConfigEntry<float> baseDamageCfg;
        internal ConfigEntry<float> baseMoveSpeedCfg;
        internal ConfigEntry<float> baseAccelerationCfg;
        internal ConfigEntry<float> baseArmorCfg;
        internal ConfigEntry<float> costCfg;

        protected GameObject cachedBodyPrefab;
        protected List<CharacterSpawnCard> cachedSpawnCards;
        protected class HornDisplayInfo
        {
            public GameObject followerPrefab;
            public Vector3 localPos = Vector3.zero;
            public Vector3 localAngles = Vector3.zero;
            public Vector3 localScale = Vector3.one;
            public string childName = "Head";
        }

        protected void BindStats(GameObject prefab, List<CharacterSpawnCard> spawnCards = null)
        {
            cachedBodyPrefab = prefab;
            cachedSpawnCards = spawnCards;
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                Log.Error($"BodyPrefab has no CharacterBody!");
                return;
            }
            string bodyName = Language.GetString(body.baseNameToken);
            baseMaxHealthCfg = BindFloat("Base Max Health", body.baseMaxHealth, $"Base Max Health of this enemy.\nDefault is {body.baseMaxHealth}", body.baseMaxHealth / 5f, body.baseMaxHealth * 5f, 1f);
            baseDamageCfg = BindFloat("Base Damage", body.baseDamage, $"Base Damage of this enemy.\nDefault is {body.baseDamage}.", body.baseDamage / 5f, body.baseDamage * 5f, 0.1f);
            if (body.baseMoveSpeed > 0)
            {
                baseMoveSpeedCfg = BindFloat("Base Move Speed", body.baseMoveSpeed, $"Base Move Speed of this enemy.\nDefault is {body.baseMoveSpeed}.a", body.baseMoveSpeed / 5f, body.baseMoveSpeed * 5f, 0.1f);
            }
            if (body.baseAcceleration > 0)
            {
                baseAccelerationCfg = BindFloat("Base Acceleration", body.baseAcceleration, $"Base Acceleration of this enemy.\nDefault is {body.baseAcceleration}.", body.baseAcceleration / 5f, body.baseAcceleration * 5f, 0.1f);
            }
            baseArmorCfg = BindFloat("Base Armor", body.baseArmor, $"Base Armor of this enemy.\nDefault is {body.baseArmor}.", 0, Mathf.Max(100, body.baseArmor), 1f);
            if (spawnCards != null)
            {
                costCfg = BindFloat("Director Cost", spawnCards[0].directorCreditCost, $"The amount of credits required for the director to spawn this enemy.\nDefault is {spawnCards[0].directorCreditCost}.</style>", spawnCards[0].directorCreditCost / 5f, spawnCards[0].directorCreditCost * 5f, 1f);
            }
        }
        protected void ApplyStats()
        {
            if (cachedBodyPrefab == null)
            {
                Log.Error($"CachedBodyPrefab is null! BindStats has to have been run already before ApplyStats is called!!!");
                return;
            }
            CharacterBody body = cachedBodyPrefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                return;
            }
            string bodyName = Language.GetString(body.baseNameToken);
            body.baseMaxHealth = baseMaxHealthCfg.Value;
            body.levelMaxHealth = baseMaxHealthCfg.Value * 0.3f;
            body.baseDamage = baseDamageCfg.Value;
            body.levelDamage = baseDamageCfg.Value * 0.2f;
            if (baseMoveSpeedCfg != null)
            {
                body.baseMoveSpeed = baseMoveSpeedCfg.Value;
            }
            if (baseAccelerationCfg != null)
            {
                body.baseAcceleration = baseAccelerationCfg.Value;
            }
            if (baseArmorCfg != null)
            {
                body.baseArmor = baseArmorCfg.Value;
            }
            if (cachedSpawnCards != null && costCfg != null)
            {
                for (int i = 0; i < cachedSpawnCards.Count; i++)
                {
                    CharacterSpawnCard card = cachedSpawnCards[i];
                    card.directorCreditCost = (int)costCfg.Value;

                }
            }
        }
        public virtual void Awake()
        {
            RegisterConfig();
            if (IsModuleEnabled())
            {
                Initialise();
            }
        }
        private bool IsModuleEnabled()
        {
            if (BootlegBestiary.Instance.mainModuleConfigEntries.TryGetValue(GetType(), out ConfigEntry<bool> entry))
            {
                return entry.Value;
            }
            return false;
        }
        protected ConfigEntry<float> BindFloat(string name, float defaultValue, string desc, float min, float max, float step = 0.1f, PluginConfig.FormatType format = PluginConfig.FormatType.None)
        {
            return config.BindOptionSteppedSlider(Section, name, defaultValue, step, desc, min, max, true, format);
        }
        protected ConfigEntry<bool> BindBool(string name, bool defaultValue, string desc)
        {
            return config.BindOption(Section, name, defaultValue, desc, true);
        }
        public virtual void RegisterConfig()
        {
            //stuff overwrites this
        }
        public virtual void Initialise()
        {
            ApplyStats();
            //stuff also overwrites this
        }
        protected T CreateSkillDef<T>(SkillDefData data) where T : SkillDef
        {
            T skillDef = ScriptableObject.CreateInstance<T>();
            (skillDef as ScriptableObject).name = data.objectName;
            skillDef.skillName = data.skillName;
            skillDef.activationStateMachineName = data.esmName;
            skillDef.activationState = data.activationState;
            skillDef.baseRechargeInterval = data.cooldown;
            skillDef.baseMaxStock = data.baseMaxStock;
            skillDef.rechargeStock = data.rechargeStock;
            skillDef.requiredStock = data.requiredStock;
            skillDef.stockToConsume = data.stockToConsume;
            skillDef.interruptPriority = data.intPrio;
            skillDef.resetCooldownTimerOnUse = data.resetCdOnUse;
            skillDef.beginSkillCooldownOnSkillEnd = data.cdOnEnd;
            skillDef.isCooldownBlockedUntilManuallyReset = data.cdBlocked;
            skillDef.isCombatSkill = data.combatSkill;
            ContentAddition.AddSkillDef(skillDef);
            return skillDef;
        }
        protected GenericSkill CreateGenericSkill(GameObject bodyPrefab, SkillDef skillDef, string familyName, SkillSlot slot)
        {
            SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
            (family as ScriptableObject).name = familyName;
            family.variants = [new SkillFamily.Variant() { skillDef = skillDef }];

            string skillName = skillDef.skillName;
            GenericSkill skill = bodyPrefab.AddComponent<GenericSkill>();
            skill.skillName = skillName;
            skill._skillFamily = family;

            SkillLocator locator = bodyPrefab.GetComponent<SkillLocator>();
            switch (slot)
            {
                case SkillSlot.None:
                    Log.Error($"SkillSlot.None detected in AddGenericSkill!"); break;
                case SkillSlot.Primary:
                    locator.primary = skill; break;
                case SkillSlot.Secondary:
                    locator.secondary = skill; break;
                case SkillSlot.Utility:
                    locator.utility = skill; break;
                case SkillSlot.Special:
                    locator.special = skill; break;
            }
            ContentAddition.AddSkillFamily(family);
            return skill;
        }
        protected EntityStateMachine CreateEntityStateMachine(GameObject bodyPrefab, string name, Type initialState = null, Type mainState = null)
        {
            EntityStateMachine esm = bodyPrefab.AddComponent<EntityStateMachine>();
            esm.customName = name;
            esm.initialStateType = new SerializableEntityStateType(initialState ?? typeof(Idle));
            esm.mainStateType = new SerializableEntityStateType(mainState ?? typeof(Idle));
            return esm;
        }
        protected AISkillDriver CreateAISkillDriver(AISkillDriverData data)
        {
            if (data == null || data.masterPrefab == null)
            {
                Log.Error($"Could not create AISkillDriver");
                return null;
            }
            AISkillDriver driver = data.masterPrefab.AddComponent<AISkillDriver>();
            driver.customName = data.customName;
            driver.skillSlot = data.skillSlot;
            driver.minDistance = data.minDistance;
            driver.maxDistance = data.maxDistance;
            driver.moveInputScale = data.moveInputScale;
            driver.movementType = data.movementType;
            driver.aimType = data.aimType;
            driver.moveTargetType = data.targetType;
            driver.ignoreNodeGraph = data.ignoreNodeGraph;
            driver.maxUserHealthFraction = data.maxHealthFraction;
            driver.minUserHealthFraction = data.minHealthFraction;
            driver.maxTargetHealthFraction = data.maxTargetHealthFraction;
            driver.minTargetHealthFraction = data.minTargetHealthFraction;
            driver.requireSkillReady = data.requireReady;
            driver.requiredSkill = data.requiredSkillDef;
            driver.activationRequiresAimConfirmation = data.activationRequiresAimConfirmation;
            driver.activationRequiresAimTargetLoS = data.activationRequiresAimTargetLoS;
            driver.activationRequiresTargetLoS = data.activationRequiresTargetLoS;
            driver.selectionRequiresAimTarget = data.selectionRequiresAimTarget;
            driver.selectionRequiresOnGround = data.selectionRequiresOnGround;
            driver.selectionRequiresTargetLoS = data.selectionRequiresTargetLoS;
            driver.selectionRequiresTargetNonFlier = data.selectionRequiresTargetNonFlier;
            driver.maxTimesSelected = data.maxTimesSelected;
            driver.driverUpdateTimerOverride = data.driverUpdateTimerOverride;
            driver.noRepeat = data.noRepeat;
            driver.nextHighPriorityOverride = data.nextHighPriorityOverride;
            driver.shouldSprint = data.shouldSprint;
            driver.aimVectorMaxSpeedOverride = data.aimVectorMaxSpeedOverride;
            driver.buttonPressType = data.buttonPressType;
            data.masterPrefab.ReorderSkillDrivers(driver, data.desiredIndex);
            return driver;
        }
        protected void RegisterLooseEntityStates(List<Type> entityStateTypes)
        {
            foreach (Type entityStateType in entityStateTypes)
            {
                ContentAddition.AddEntityState(entityStateType, out bool success);
                if (!success)
                {
                    Log.Error($"Failed to add EntityState {entityStateType.Name}!");
                }
            }
        }
        protected void AddNameToken(GameObject bodyPrefab, string name)
        {
            if (bodyPrefab == null)
            {
                return;
            }
            CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                return;
            }
            string token = body.baseNameToken;
            if (Language.GetString(token) == token)
            {
                LanguageAPI.Add(token, name);
            }
            else
            {
                Log.Error($"Could not add token {token} as it is already present.");
            }
        }
        protected void AddLoreToken(GameObject bodyPrefab, string lore)
        {
            if (bodyPrefab == null)
            {
                return;
            }
            CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                return;
            }
            string nameToken = body.baseNameToken;
            string loreToken = body.baseNameToken.Replace("_NAME", "_LORE");
            if (!loreToken.EndsWith("_LORE"))
            {
                Log.Error($"NameToken does not end with _NAME, unable to create matching Lore Token!");
                return;
            }
            LanguageAPI.Add(loreToken, lore);

        }
        protected void HurtBoxLayersToEntityPrecise(GameObject bodyPrefab)
        {
            if (bodyPrefab == null)
            {
                Log.Debug($"No bodyPrefab!");
                return;
            }
            HurtBox[] hurtBoxes = bodyPrefab.GetComponentsInChildren<HurtBox>();
            if (hurtBoxes != null && hurtBoxes.Length > 0)
            {
                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if (hurtBox == null)
                    {
                        continue;
                    }
                    hurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                }
            }
        }
        protected void ConfigureESM(GameObject bodyPrefab, string esmName, Type mainStateType = null, bool registerMainStateType = true, Type initialStateType = null, bool registerInitialStateType = true)
        {
            if (bodyPrefab == null)
            {
                return;
            }
            EntityStateMachine entityStateMachine = bodyPrefab.GetComponents<EntityStateMachine>().Where(esm => esm.customName == esmName).FirstOrDefault();
            if (entityStateMachine == null)
            {
                return;
            }
            if (mainStateType != null)
            {
                entityStateMachine.mainStateType = registerMainStateType ? ContentAddition.AddEntityState(mainStateType, out _) : new SerializableEntityStateType(mainStateType);
            }
            if (initialStateType != null)
            {
                entityStateMachine.initialStateType = registerInitialStateType ? ContentAddition.AddEntityState(initialStateType, out _) : new SerializableEntityStateType(initialStateType);
            }
        }
        protected void SetDeathState(GameObject bodyPrefab, Type deathStateType, bool register = true)
        {
            if (bodyPrefab == null || deathStateType == null)
            {
                return;
            }
            CharacterDeathBehavior deathBehaviour = bodyPrefab.GetComponent<CharacterDeathBehavior>();  
            if (deathBehaviour == null)
            {
                Log.Error($"{bodyPrefab} does not have DeathBehaviour!");
                return;
            }
            deathBehaviour.deathState = register ? ContentAddition.AddEntityState(deathStateType, out _) : new SerializableEntityStateType(deathStateType);
        }
        protected void AddCardToMap(CharacterSpawnCard csc, MonsterCategory category, int spawnWeight, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, int minStageCompletions, DirectorAPI.Stage stage, string customStageName = "")
        {
            if (csc == null)
            {
                return;
            }
            DirectorCard directorCard = new DirectorCard
            {
                spawnCard = csc,
                selectionWeight = spawnWeight,
                spawnDistance = spawnDistance,
                preventOverhead = preventOverhead,
                minimumStageCompletions = minStageCompletions
            };
            DirectorCardHolder holder = new DirectorCardHolder
            {
                Card = directorCard,
                MonsterCategory = category,
                MonsterCategorySelectionWeight = 1
            };
            Helpers.AddNewMonsterToStage(holder, false, stage, customStageName);
        }
        protected void RemoveCardFromDCCS(CharacterSpawnCard csc, DirectorCardCategorySelection dccs)
        {
            dccs.RemoveCardsThatFailFilter(directorCard => directorCard.spawnCard != csc);
        }
        protected void SetupLogbookEntry(GameObject bodyPrefab, string cachedName, string unlockableNameToken, string enemyName)
        {
            if (bodyPrefab == null)
            {
                return;
            }
            if (Language.GetString(unlockableNameToken) != unlockableNameToken)
            {
                return;
            }
            CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                return;
            }
            DeathRewards deathRewards = bodyPrefab.GetComponent<DeathRewards>();
            if (deathRewards == null)
            {
                return;
            }
            UnlockableDef unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = cachedName;
            unlockableDef.displayModelPrefab = bodyPrefab;
            unlockableDef.nameToken = unlockableNameToken;
            ContentAddition.AddUnlockableDef(unlockableDef);
            LanguageAPI.Add(unlockableNameToken, $"Monster Log: {enemyName}");
            deathRewards.logUnlockableDef = unlockableDef;
        }
        //blanket sets up all the parts that we need to use IDRS Helper
        protected void SetUpAllEliteAffixes(ItemDisplayRuleSet idrs)
        {
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteFire, 
                [new HornDisplayInfo 
                { 
                    followerPrefab = VanillaAssets.DisplayAffixFire
                }, 
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixFire
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteLightning,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLightning
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteIce,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixIce
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteEarth,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixEarth
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteAurelionite,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixAurelionite
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteVoid,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixVoid
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDElitePoison,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixPoison
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteHaunted,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixHaunted
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteBead,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixBead
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteCollective,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollective
                },
                new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixCollectiveRing
                }]);
            SetUpEliteAffix(idrs, VanillaAssets.EDEliteLunar,
                [new HornDisplayInfo
                {
                    followerPrefab = VanillaAssets.DisplayAffixLunar
                }]);
        }
        protected void SetUpEliteAffix(ItemDisplayRuleSet idrs, Object keyAsset, HornDisplayInfo[] displayInfos)
        {
            if (idrs == null || keyAsset == null || displayInfos == null || displayInfos.Length == 0)
            {
                Log.Error($"SetupMultiHorn failed!");
                return;
            }
            DisplayRuleGroup ruleGroup = new DisplayRuleGroup();
            foreach (HornDisplayInfo displayInfo in displayInfos)
            {
                if (displayInfo.followerPrefab == null)
                {
                    continue;
                }
                ItemDisplayRule rule = new ItemDisplayRule
                {
                    childName = displayInfo.childName,
                    followerPrefab = displayInfo.followerPrefab,
                    localPos = displayInfo.localPos,
                    localAngles = displayInfo.localAngles,
                    localScale = displayInfo.localScale
                };
                ruleGroup.AddDisplayRule(rule);
            }

            idrs.SetDisplayRuleGroup(keyAsset, ruleGroup);
        }
    }
}
