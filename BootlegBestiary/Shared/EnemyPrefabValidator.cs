using System.Linq;
using RoR2.Networking;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using RoR2.CharacterAI;

namespace BootlegBestiary.Shared
{
    public class EnemyPrefabValidator
    {
        public static void ValidateAssets(GameObject bodyPrefab, GameObject masterPrefab, CharacterSpawnCard characterSpawnCard)
        {
            ValidateBodyPrefab(bodyPrefab);
            ValidateMasterPrefab(masterPrefab, bodyPrefab);
            ValidateCharacterSpawnCard(characterSpawnCard, masterPrefab, bodyPrefab);
        }
        public static void ValidateBodyPrefab(GameObject bodyPrefab)
        {
            if (bodyPrefab == null)
            {
                Log.Error("Cannot validate body prefab as prefab is null.");
                return;
            }
            Log.Debug($"Validating body prefab: {bodyPrefab.name}");

            if (bodyPrefab.GetComponent<NetworkIdentity>() == null)
            {
                Log.Error("Prefab has no NetworkIdentity.");
            }
            if (bodyPrefab.GetComponent<InputBankTest>() == null)
            {
                Log.Error("Prefab has no InputBankTest.");
            }
            if (bodyPrefab.GetComponent<TeamComponent>() == null)
            {
                Log.Error("Prefab has no TeamComponent.");
            }
            if (bodyPrefab.GetComponent<SkillLocator>() == null)
            {
                Log.Error("Prefab has no SkillLocator.");
            }
            if (bodyPrefab.GetComponent<CharacterNetworkTransform>() == null)
            {
                Log.Error("Prefab has no CharacterNetworkTransform.");
            }

            EntityStateMachine[] entityStateMachines = bodyPrefab.GetComponents<EntityStateMachine>();
            bool hasNonBodyESM = entityStateMachines.Any(esm => esm != null && esm.customName != "Body");

            CharacterBody characterBody = bodyPrefab.GetComponent<CharacterBody>();
            if (characterBody == null)
            {
                Log.Error("Prefab has no CharacterBody.");
            }
            else
            {
                Log.Debug("Checking CharacterBody fields...");
                if (string.IsNullOrWhiteSpace(characterBody.baseNameToken))
                {
                    Log.Error("CharacterBody.baseNameToken is unset.");
                }
                if (characterBody.baseMaxHealth <= 0f)
                {
                    Log.Error($"CharacterBody.baseMaxHealth is zero or negative ({characterBody.baseMaxHealth}).");
                }
                if (characterBody.baseRegen < 0f)
                {
                    Log.Warning($"CharacterBody.baseRegen is negative ({characterBody.baseRegen}).");
                }
                if (characterBody.baseMoveSpeed < 0f)
                {
                    Log.Error($"CharacterBody.baseMoveSpeed is negative ({characterBody.baseMoveSpeed}).");
                }
                else if (characterBody.baseMoveSpeed == 0f)
                {
                    Log.Warning("CharacterBody.baseMoveSpeed is zero.");
                }
                if (characterBody.baseAcceleration < 0f)
                {
                    Log.Error($"CharacterBody.baseAcceleration is negative ({characterBody.baseAcceleration}).");
                }
                else if (characterBody.baseAcceleration == 0f && characterBody.baseMoveSpeed > 0f)
                {
                    Log.Warning("CharacterBody.baseAcceleration is zero while baseMoveSpeed is positive.");
                }
                if (characterBody.baseJumpPower < 0f)
                {
                    Log.Error($"CharacterBody.baseJumpPower is negative ({characterBody.baseJumpPower}).");
                }
                else if (characterBody.baseJumpPower == 0f && characterBody.baseJumpCount > 0)
                {
                    Log.Warning($"CharacterBody.baseJumpPower is zero but baseJumpCount is {characterBody.baseJumpCount}.");
                }
                if (characterBody.baseJumpCount < 0)
                {
                    Log.Error($"CharacterBody.baseJumpCount is negative ({characterBody.baseJumpCount}).");
                }
                if (characterBody.baseDamage < 0f)
                {
                    Log.Error($"CharacterBody.baseDamage is negative ({characterBody.baseDamage}).");
                }
                else if (characterBody.baseDamage == 0f)
                {
                    Log.Warning("CharacterBody.baseDamage is zero.");
                }
                if (characterBody.baseAttackSpeed < 0f)
                {
                    Log.Error($"CharacterBody.baseAttackSpeed is negative ({characterBody.baseAttackSpeed}).");
                }
                else if (characterBody.baseAttackSpeed == 0f)
                {
                    Log.Warning("CharacterBody.baseAttackSpeed is zero.");
                }
                if (characterBody.baseCrit < 0f)
                {
                    Log.Error($"CharacterBody.baseCrit is negative ({characterBody.baseCrit}).");
                }
                if (characterBody.baseArmor < 0f)
                {
                    Log.Warning($"CharacterBody.baseArmor is negative ({characterBody.baseArmor}).");
                }
                if (characterBody.sprintingSpeedMultiplier < 0f)
                {
                    Log.Error($"CharacterBody.sprintingSpeedMultiplier is negative ({characterBody.sprintingSpeedMultiplier}).");
                }
                if (!characterBody.autoCalculateLevelStats)
                {
                    Log.Warning("CharacterBody.autoCalculateLevelStats is false. Verify level stats are set manually.");
                }
                if (characterBody.aimOriginTransform == null)
                {
                    Log.Warning("CharacterBody.aimOriginTransform is unset. AimOrigin will fall back to the body transform position.");
                }
                if (characterBody.hullClassification == HullClassification.Count)
                {
                    Log.Error("CharacterBody.hullClassification is invalid.");
                }
                if (characterBody.portraitIcon == null)
                {
                    Log.Warning("CharacterBody.portraitIcon is unset.");
                }

                EntityStateMachine[] vehicleIdleSMs = characterBody.vehicleIdleStateMachine;
                if (vehicleIdleSMs == null || vehicleIdleSMs.Length == 0)
                {
                    if (hasNonBodyESM)
                    {
                        Log.Error("CharacterBody.vehicleIdleStateMachine is empty but prefab has non-Body EntityStateMachines.");
                    }
                }
                else if (vehicleIdleSMs.Any(esm => esm != null && esm.customName == "Body"))
                {
                    Log.Error("CharacterBody.vehicleIdleStateMachine contains the Body EntityStateMachine.");
                }
            }

            CameraTargetParams cameraTargetParams = bodyPrefab.GetComponent<CameraTargetParams>();
            if (cameraTargetParams == null)
            {
                Log.Warning("Prefab has no CameraTargetParams. Optional, but useful for DebugToolkit's spawn_as command.");
            }
            else
            {
                Log.Debug("Checking CameraTargetParams fields...");
                if (cameraTargetParams.cameraPivotTransform == null)
                {
                    Log.Message("CameraTargetParams.cameraPivotTransform is unset - falls back to CharacterBody.aimOriginTransform.");
                }
            }
            ModelLocator modelLocator = bodyPrefab.GetComponent<ModelLocator>();
            if (modelLocator == null)
            {
                Log.Error("Prefab has no ModelLocator.");
            }
            else
            {
                Log.Debug("Checking ModelLocator fields...");
                if (modelLocator.modelTransform == null)
                {
                    Log.Error("ModelLocator.modelTransform is unset.");
                }
                else if (modelLocator.modelTransform.gameObject.GetComponent<CharacterModel>() == null)
                {
                    Log.Error("ModelLocator.modelTransform points to a GameObject without a CharacterModel.");
                }
                if (modelLocator.modelBaseTransform == null)
                {
                    Log.Error("ModelLocator.modelBaseTransform is unset.");
                }
            }
            if (entityStateMachines.Length == 0)
            {
                Log.Error("Prefab has no EntityStateMachines.");
            }
            else
            {
                if (!entityStateMachines.Any(esm => esm != null && esm.customName == "Body"))
                {
                    Log.Error("Prefab has no EntityStateMachine with customName 'Body'.");
                }
                foreach (EntityStateMachine esm in entityStateMachines)
                {
                    if (esm == null) continue;
                    if (string.IsNullOrWhiteSpace(esm.customName))
                    {
                        Log.Error("Prefab has an unnamed EntityStateMachine.");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(esm.mainStateType.typeName))
                    {
                        Log.Warning($"EntityStateMachine '{esm.customName}' has no mainStateType. Set in code.");
                    }
                    if (string.IsNullOrWhiteSpace(esm.initialStateType.typeName))
                    {
                        Log.Warning($"EntityStateMachine '{esm.customName}' has no initialStateType. Set in code.");
                    }
                }
            }
            NetworkStateMachine networkStateMachine = bodyPrefab.GetComponent<NetworkStateMachine>();
            if (networkStateMachine == null)
            {
                Log.Error("Prefab has no NetworkStateMachine.");
            }
            else if (networkStateMachine.stateMachines == null || networkStateMachine.stateMachines.Length == 0)
            {
                Log.Error("NetworkStateMachine.stateMachines is empty.");
            }
            else
            {
                foreach (EntityStateMachine esm in entityStateMachines)
                {
                    if (esm == null) continue;
                    if (!networkStateMachine.stateMachines.Contains(esm))
                    {
                        Log.Error($"NetworkStateMachine.stateMachines does not contain '{esm.customName}'. This ESM will not be networked.");
                    }
                }
            }
            HealthComponent healthComponent = bodyPrefab.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Log.Error("Prefab has no HealthComponent.");
            }
            else if (healthComponent.body == null)
            {
                Log.Message("HealthComponent.body is unset. Auto-resolved in HealthComponent.Awake().");
            }
            Interactor interactor = bodyPrefab.GetComponent<Interactor>();
            InteractionDriver interactionDriver = bodyPrefab.GetComponent<InteractionDriver>();
            if (interactor != null && interactionDriver == null)
            {
                Log.Error("Prefab has an Interactor but no InteractionDriver.");
            }
            else if (interactor == null && interactionDriver != null)
            {
                Log.Error("Prefab has an InteractionDriver but no Interactor.");
            }
            else if (interactor == null && interactionDriver == null)
            {
                Log.Message("Prefab has no Interactor or InteractionDriver. Optional.");
            }
            CharacterDeathBehavior characterDeathBehaviour = bodyPrefab.GetComponent<CharacterDeathBehavior>();
            if (characterDeathBehaviour == null)
            {
                Log.Error("Prefab has no CharacterDeathBehavior.");
            }
            else
            {
                Log.Debug("Checking CharacterDeathBehavior fields...");
                if (characterDeathBehaviour.deathStateMachine == null)
                {
                    Log.Error("CharacterDeathBehavior.deathStateMachine is unset.");
                }
                else if (characterDeathBehaviour.deathStateMachine.customName != "Body")
                {
                    Log.Error("CharacterDeathBehavior.deathStateMachine is not the Body EntityStateMachine.");
                }
                if (string.IsNullOrWhiteSpace(characterDeathBehaviour.deathState.typeName))
                {
                    Log.Warning("CharacterDeathBehavior.deathState is unset. Set in code.");
                }

                EntityStateMachine[] idleSMs = characterDeathBehaviour.idleStateMachine;
                if (idleSMs == null || idleSMs.Length == 0)
                {
                    if (hasNonBodyESM)
                    {
                        Log.Warning("CharacterDeathBehavior.idleStateMachine is empty. Non-Body EntityStateMachines will keep operating after death.");
                    }
                }
                else
                {
                    foreach (EntityStateMachine esm in idleSMs)
                    {
                        if (esm == null)
                        {
                            Log.Error("CharacterDeathBehavior.idleStateMachine contains a null entry.");
                            continue;
                        }
                        if (esm.customName == "Body")
                        {
                            Log.Error("CharacterDeathBehavior.idleStateMachine contains the Body EntityStateMachine.");
                        }
                    }
                }
            }
            SetStateOnHurt setStateOnHurt = bodyPrefab.GetComponent<SetStateOnHurt>();
            if (setStateOnHurt == null)
            {
                Log.Warning("Prefab has no SetStateOnHurt. Required if this enemy needs to be stunned or frozen.");
            }
            else
            {
                Log.Debug("Checking SetStateOnHurt fields...");
                if (setStateOnHurt.targetStateMachine == null)
                {
                    Log.Error("SetStateOnHurt.targetStateMachine is unset.");
                }
                else if (setStateOnHurt.targetStateMachine.customName != "Body")
                {
                    Log.Error("SetStateOnHurt.targetStateMachine is not the Body EntityStateMachine.");
                }
                if (string.IsNullOrWhiteSpace(setStateOnHurt.hurtState.typeName))
                {
                    Log.Warning("SetStateOnHurt.hurtState is unset. Set in code.");
                }

                EntityStateMachine[] idleSMs = setStateOnHurt.idleStateMachine;
                if (idleSMs == null || idleSMs.Length == 0)
                {
                    if (hasNonBodyESM)
                    {
                        Log.Warning("SetStateOnHurt.idleStateMachine is empty. Non-Body EntityStateMachines will not be set to idle when stunned.");
                    }
                }
                else
                {
                    foreach (EntityStateMachine esm in entityStateMachines)
                    {
                        if (esm == null) continue;
                        if (esm.customName == "Body") continue;
                        if (!idleSMs.Contains(esm))
                        {
                            Log.Warning($"SetStateOnHurt.idleStateMachine does not contain '{esm.customName}'.");
                        }
                    }
                }
            }
            DeathRewards deathRewards = bodyPrefab.GetComponent<DeathRewards>();
            if (deathRewards == null)
            {
                Log.Warning("Prefab has no DeathRewards. Enemy will yield no gold or XP on death.");
            }
            else
            {
                Log.Debug("Checking DeathRewards fields...");
                if (deathRewards.logUnlockableDef == null)
                {
                    Log.Warning("DeathRewards.logUnlockableDef is unset. Enemy will not drop a logbook.");
                }
                if (characterBody != null && characterBody.isChampion && deathRewards.bossDropTable == null)
                {
                    Log.Warning("CharacterBody.isChampion is true but DeathRewards.bossDropTable is unset. Enemy will not drop a boss item.");
                }
            }
            CharacterMotor characterMotor = bodyPrefab.GetComponent<CharacterMotor>();
            CharacterDirection characterDirection = bodyPrefab.GetComponent<CharacterDirection>();
            KinematicCharacterMotor kinematicMotor = bodyPrefab.GetComponent<KinematicCharacterMotor>();
            Rigidbody rigidBody = bodyPrefab.GetComponent<Rigidbody>();
            PseudoCharacterMotor pseudoCharacterMotor = bodyPrefab.GetComponent<PseudoCharacterMotor>();
            RigidbodyMotor rigidbodyMotor = bodyPrefab.GetComponent<RigidbodyMotor>();
            RigidbodyDirection rigidBodyDirection = bodyPrefab.GetComponent<RigidbodyDirection>();
            if (characterMotor != null && characterDirection == null)
            {
                Log.Error("Prefab has a CharacterMotor but no CharacterDirection.");
            }
            else if (characterMotor == null && characterDirection != null)
            {
                Log.Error("Prefab has a CharacterDirection but no CharacterMotor.");
            }
            if (characterMotor != null)
            {
                Log.Debug("Checking CharacterMotor fields...");
                if (characterMotor.characterDirection == null)
                {
                    Log.Message("CharacterMotor.characterDirection is unset. Not used by the base game, but may be referenced by mods.");
                }
                if (characterMotor.mass < 0f)
                {
                    Log.Error($"CharacterMotor.mass is negative ({characterMotor.mass}).");
                }
                if (characterMotor.airControl < 0f)
                {
                    Log.Error($"CharacterMotor.airControl is negative ({characterMotor.airControl}).");
                }
            }
            if (characterDirection != null)
            {
                Log.Debug("Checking CharacterDirection fields...");
                if (characterDirection.targetTransform == null)
                {
                    Log.Error("CharacterDirection.targetTransform is unset.");
                }
                if (characterDirection.turnSpeed < 0f)
                {
                    Log.Error($"CharacterDirection.turnSpeed is negative ({characterDirection.turnSpeed}).");
                }
                else if (characterDirection.turnSpeed == 0f)
                {
                    Log.Warning("CharacterDirection.turnSpeed is zero.");
                }
                if (characterDirection.modelAnimator == null)
                {
                    Log.Message("CharacterDirection.modelAnimator is unset. Auto-resolved in CharacterDirection.Start().");
                }
            }
            if (kinematicMotor != null)
            {
                if (rigidBody == null)
                {
                    Log.Error("Prefab has a KinematicCharacterMotor but no Rigidbody.");
                }
                if (kinematicMotor.Capsule == null)
                {
                    Log.Error("KinematicCharacterMotor.Capsule is unset.");
                }
            }
            if (rigidBody != null)
            {
                Log.Debug("Checking Rigidbody fields...");
                if (rigidBody.mass < 0f)
                {
                    Log.Error($"Rigidbody.mass is negative ({rigidBody.mass}).");
                }
                if (rigidBody.drag < 0f)
                {
                    Log.Error($"Rigidbody.drag is negative ({rigidBody.drag}).");
                }
                if (rigidBody.angularDrag < 0f)
                {
                    Log.Error($"Rigidbody.angularDrag is negative ({rigidBody.angularDrag}).");
                }
                if (rigidBody.isKinematic && kinematicMotor == null)
                {
                    Log.Error("Rigidbody.isKinematic is true but prefab has no KinematicCharacterMotor.");
                }
                if (!rigidBody.isKinematic && kinematicMotor != null)
                {
                    Log.Error("Rigidbody.isKinematic is false but prefab has a KinematicCharacterMotor.");
                }
            }
            if (rigidBodyDirection != null)
            {
                Log.Debug("Checking RigidbodyDirection fields...");
                if (rigidBody == null)
                {
                    Log.Error("Prefab has a RigidbodyDirection but no Rigidbody.");
                }
                if (rigidBodyDirection.rigid == null)
                {
                    Log.Error("RigidbodyDirection.rigid is unset.");
                }
                if (rigidBodyDirection.angularVelocityPID == null)
                {
                    Log.Error("RigidbodyDirection.angularVelocityPID is unset.");
                }
                if (rigidBodyDirection.torquePID == null)
                {
                    Log.Error("RigidbodyDirection.torquePID is unset.");
                }
            }
            if (rigidbodyMotor != null)
            {
                Log.Debug("Checking RigidbodyMotor fields...");
                if (rigidbodyMotor.rigid == null)
                {
                    Log.Error("RigidbodyMotor.rigid is unset.");
                }
                if (rigidbodyMotor.forcePID == null)
                {
                    Log.Error("RigidbodyMotor.forcePID is unset.");
                }
            }
            QuaternionPID[] quaternionPIDs = bodyPrefab.GetComponents<QuaternionPID>();
            VectorPID[] vectorPIDs = bodyPrefab.GetComponents<VectorPID>();
            if (rigidBodyDirection != null)
            {
                if (quaternionPIDs.Length == 0)
                {
                    Log.Error("Prefab has a RigidbodyDirection but no QuaternionPID.");
                }
                else if (quaternionPIDs.Length > 1)
                {
                    Log.Warning($"Prefab has {quaternionPIDs.Length} QuaternionPIDs. Only one is required (named 'Angular Velocity PID').");
                }
                else if (quaternionPIDs[0].customName != "Angular Velocity PID")
                {
                    Log.Warning("QuaternionPID.customName is not 'Angular Velocity PID'. Rename for consistency.");
                }
            }
            else if (quaternionPIDs.Length > 0)
            {
                Log.Warning("Prefab has QuaternionPIDs but no RigidbodyDirection to use them.");
            }
            if (rigidbodyMotor != null)
            {
                if (vectorPIDs.Length == 0)
                {
                    Log.Error("Prefab has a RigidbodyMotor but no VectorPIDs.");
                }
                else if (vectorPIDs.Length != 2)
                {
                    Log.Error($"RigidbodyMotor expects 2 VectorPIDs (Force PID + torquePID), found {vectorPIDs.Length}.");
                }
                else
                {
                    VectorPID forcePID = vectorPIDs.FirstOrDefault(pid => pid.customName == "Force PID");
                    if (forcePID == null)
                    {
                        Log.Warning("Could not find a VectorPID with customName 'Force PID'.");
                    }
                    else if (forcePID.isAngle)
                    {
                        Log.Warning("Force PID has isAngle set. This may result in unexpected behaviour.");
                    }

                    VectorPID torquePID = vectorPIDs.FirstOrDefault(pid => pid.customName == "torquePID");
                    if (torquePID == null)
                    {
                        Log.Warning("Could not find a VectorPID with customName 'torquePID'.");
                    }
                    else if (!torquePID.isAngle)
                    {
                        Log.Warning("Torque PID has isAngle unset. This may result in unexpected behaviour.");
                    }
                }
            }
            else if (vectorPIDs.Length > 0)
            {
                Log.Warning("Prefab has VectorPIDs but no RigidbodyMotor to use them.");
            }
            int motorCount = (characterMotor != null ? 1 : 0) + (rigidbodyMotor != null ? 1 : 0) + (pseudoCharacterMotor != null ? 1 : 0);
            if (motorCount == 0)
            {
                Log.Error("Prefab has no motor (CharacterMotor, RigidbodyMotor, or PseudoCharacterMotor).");
            }
            else if (motorCount > 1)
            {
                Log.Error("Prefab has more than one motor - these will conflict with each other.");
            }
            if (bodyPrefab.GetComponent<CapsuleCollider>() == null && bodyPrefab.GetComponent<SphereCollider>() == null && bodyPrefab.GetComponent<BoxCollider>() == null)
            {
                Log.Warning("Prefab has no collider - enemy will not have body collision.");
            }
            if (modelLocator != null && modelLocator.modelTransform != null)
            {
                ValidateModel(modelLocator.modelTransform.gameObject);
            }
        }
        private static void ValidateModel(GameObject model)
        {
            Log.Debug($"Validating model: {model.name}");
            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                Log.Error("Model has no Animator.");
            }
            else
            {
                Log.Debug("Checking Animator fields...");
                if (animator.runtimeAnimatorController == null)
                {
                    Log.Warning("Animator.runtimeAnimatorController is unset. Only valid if a ModelSkinController provides one via address.");
                }
                if (animator.avatar == null)
                {
                    Log.Warning("Animator.avatar is unset. Only valid if a ModelSkinController provides one via address.");
                }
                if (animator.applyRootMotion)
                {
                    Log.Warning("Animator.applyRootMotion is enabled. Ensure this is intentional.");
                }
            }
            ChildLocator childLocator = model.GetComponent<ChildLocator>();
            if (childLocator == null)
            {
                Log.Warning("Model has no ChildLocator. Finding bones at runtime will be more of a pain in the ass");
            }
            else
            {
                Log.Debug("Checking ChildLocator fields...");
                if (childLocator.transformPairs == null || childLocator.transformPairs.Length == 0)
                {
                    Log.Error("ChildLocator.transformPairs is empty.");
                }
                else
                {
                    foreach (var pair in childLocator.transformPairs)
                    {
                        if (string.IsNullOrWhiteSpace(pair.name))
                        {
                            Log.Error("ChildLocator entry has an unset name.");
                        }
                        if (pair.transform == null)
                        {
                            Log.Error($"ChildLocator entry '{pair.name}' has a null transform.");
                        }
                    }
                }
            }
            CharacterModel characterModel = model.GetComponent<CharacterModel>();
            if (characterModel == null)
            {
                Log.Error("Model has no CharacterModel.");
            }
            else
            {
                Log.Debug("Checking CharacterModel fields...");
                if (characterModel.body == null)
                {
                    Log.Error("CharacterModel.body is unset.");
                }
                if (characterModel.itemDisplayRuleSet == null)
                {
                    Log.Warning("CharacterModel.itemDisplayRuleSet is unset. All items (including elite horns) will not display.");
                }
                if (characterModel.baseRendererInfos == null || characterModel.baseRendererInfos.Length == 0)
                {
                    Log.Warning("CharacterModel.baseRendererInfos is empty. Only valid if a ModelSkinController provides renderers via SkinDefs.");
                }
                else
                {
                    for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
                    {
                        var info = characterModel.baseRendererInfos[i];
                        if (info.renderer == null)
                        {
                            Log.Error($"CharacterModel.baseRendererInfos[{i}].renderer is unset.");
                        }
                        if (info.defaultMaterial == null)
                        {
                            Log.Error($"CharacterModel.baseRendererInfos[{i}].defaultMaterial is unset.");
                        }
                    }
                }
            }
            AimAnimator aimAnimator = model.GetComponent<AimAnimator>();
            if (aimAnimator == null)
            {
                Log.Warning("Model has no AimAnimator. Aim animations will not trigger.");
            }
            else
            {
                Log.Debug("Checking AimAnimator fields...");
                if (aimAnimator.inputBank == null)
                {
                    Log.Error("AimAnimator.inputBank is unset.");
                }
                if (aimAnimator.directionComponent == null)
                {
                    Log.Warning("AimAnimator.directionComponent is unset. Direction will fall back to base.transform.eulerAngles.");
                }
            }
            HurtBoxGroup hurtBoxGroup = model.GetComponent<HurtBoxGroup>();
            if (hurtBoxGroup == null)
            {
                Log.Error("Model has no HurtBoxGroup.");
            }
            else
            {
                Log.Debug("Checking HurtBoxGroup fields...");
                if (hurtBoxGroup.mainHurtBox == null)
                {
                    Log.Error("HurtBoxGroup.mainHurtBox is unset.");
                }
                if (hurtBoxGroup.hurtBoxes == null || hurtBoxGroup.hurtBoxes.Length == 0)
                {
                    Log.Error("HurtBoxGroup.hurtBoxes is empty.");
                }
            }

            HurtBox[] hurtBoxes = model.GetComponentsInChildren<HurtBox>();
            if (hurtBoxes == null || hurtBoxes.Length == 0)
            {
                Log.Error("Model has no HurtBoxes.");
                return;
            }

            bool atLeastOneBullseye = false;
            bool atLeastOneSniperTarget = false;
            bool groupListUsable = hurtBoxGroup != null && hurtBoxGroup.hurtBoxes != null && hurtBoxGroup.hurtBoxes.Length > 0;

            foreach (HurtBox hurtBox in hurtBoxes)
            {
                if (hurtBox == null) continue;
                string hurtBoxName = hurtBox.name;

                if (hurtBox.GetComponent<CapsuleCollider>() == null && hurtBox.GetComponent<BoxCollider>() == null && hurtBox.GetComponent<SphereCollider>() == null)
                {
                    Log.Error($"HurtBox '{hurtBoxName}' has no Capsule, Box, or Sphere collider.");
                }
                if (hurtBox.healthComponent == null)
                {
                    Log.Error($"HurtBox '{hurtBoxName}' has no HealthComponent.");
                }
                if (hurtBox.gameObject.layer != LayerIndex.entityPrecise.intVal)
                {
                    Log.Warning($"HurtBox '{hurtBoxName}' is not on the entityPrecise layer.");
                }
                if (groupListUsable && !hurtBoxGroup.hurtBoxes.Contains(hurtBox))
                {
                    Log.Warning($"HurtBox '{hurtBoxName}' is not in the HurtBoxGroup.");
                }
                if (hurtBox.isBullseye)
                {
                    atLeastOneBullseye = true;
                }
                if (hurtBox.isSniperTarget)
                {
                    atLeastOneSniperTarget = true;
                }
            }

            if (!atLeastOneBullseye)
            {
                Log.Warning("No HurtBox has isBullseye set. Characters that rely on this like Huntress and Operator will struggle to target this enemy.");
            }
            if (!atLeastOneSniperTarget)
            {
                Log.Warning("No HurtBox has isSniperTarget set. Characters that rely on this like Railgunner will not be able to hit weak spots on this enemy.");
            }
        }
        public static void ValidateMasterPrefab(GameObject masterPrefab, GameObject bodyPrefab)
        {
            if (masterPrefab == null)
            {
                Log.Error("Cannot validate master prefab: prefab is null.");
                return;
            }
            Log.Debug($"Validating master prefab: {masterPrefab.name}");

            if (masterPrefab.GetComponent<NetworkIdentity>() == null)
            {
                Log.Error("Master prefab has no NetworkIdentity.");
            }
            if (masterPrefab.GetComponent<Inventory>() == null)
            {
                Log.Error("Master prefab has no Inventory.");
            }
            if (masterPrefab.GetComponent<MinionOwnership>() == null)
            {
                Log.Error("Master prefab has no MinionOwnership.");
            }
            CharacterMaster master = masterPrefab.GetComponent<CharacterMaster>();
            if (master == null)
            {
                Log.Error("Master prefab has no CharacterMaster.");
            }
            else if (master.bodyPrefab != bodyPrefab)
            {
                Log.Error("CharacterMaster.bodyPrefab does not match the provided body prefab.");
            }
            BaseAI baseAI = masterPrefab.GetComponent<BaseAI>();
            if (baseAI == null)
            {
                Log.Error("Master prefab has no BaseAI.");
            }
            else
            {
                Log.Debug("Checking BaseAI fields...");
                if (baseAI.stateMachine == null)
                {
                    Log.Error("BaseAI.stateMachine is unset.");
                }
                if (string.IsNullOrWhiteSpace(baseAI.scanState.typeName))
                {
                    Log.Warning("BaseAI.scanState is unset. Set in code.");
                }
            }
            EntityStateMachine[] masterESMs = masterPrefab.GetComponents<EntityStateMachine>();
            if (masterESMs.Length == 0)
            {
                Log.Error("Master prefab has no EntityStateMachines.");
            }
            else if (masterESMs.Length > 1)
            {
                Log.Error($"Master prefab has {masterESMs.Length} EntityStateMachines. Only one is required (named 'AI').");
            }
            else
            {
                EntityStateMachine aiESM = masterESMs.FirstOrDefault(esm => esm != null && esm.customName == "AI");
                if (aiESM == null)
                {
                    Log.Error("Master prefab has no EntityStateMachine with customName 'AI'.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(aiESM.initialStateType.typeName))
                    {
                        Log.Warning("AI EntityStateMachine has no initialStateType. Set in code.");
                    }
                    if (string.IsNullOrWhiteSpace(aiESM.mainStateType.typeName))
                    {
                        Log.Warning("AI EntityStateMachine has no mainStateType. Set in code.");
                    }
                }
            }
            AISkillDriver[] skillDrivers = masterPrefab.GetComponents<AISkillDriver>();
            if (skillDrivers.Length == 0)
            {
                Log.Warning("Master prefab has no AISkillDrivers. Verify these are added in code.");
            }
        }
        public static void ValidateCharacterSpawnCard(CharacterSpawnCard csc, GameObject masterPrefab, GameObject bodyPrefab)
        {
            if (csc == null)
            {
                Log.Error("Cannot validate CharacterSpawnCard as card is null!");
                return;
            }
            Log.Debug($"Validating CharacterSpawnCard: {csc.name}");
            if (csc.prefab == null)
            {
                Log.Error("CharacterSpawnCard.prefab is unset.");
            }
            else if (csc.prefab == bodyPrefab)
            {
                Log.Error("CharacterSpawnCard.prefab points at the body prefab. Should point instead at the master prefab.");
            }
            else if (csc.prefab != masterPrefab)
            {
                Log.Error("CharacterSpawnCard.prefab does not match the provided master prefab.");
            }
            if (bodyPrefab != null)
            {
                CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
                if (body != null && body.hullClassification != csc.hullSize)
                {
                    Log.Warning($"Hull mismatch: body is {body.hullClassification}, spawn card is {csc.hullSize}.");
                }
            }
            if (!csc.sendOverNetwork)
            {
                Log.Error("CharacterSpawnCard.sendOverNetwork is false. Must be true for multiplayer.");
            }
            if (!csc.forbiddenFlags.HasFlag(RoR2.Navigation.NodeFlags.NoCharacterSpawn))
            {
                Log.Warning("CharacterSpawnCard.forbiddenFlags does not include NoCharacterSpawn. Enemy may spawn in unintended locations.");
            }
            if (csc.directorCreditCost < 0)
            {
                Log.Error($"CharacterSpawnCard.directorCreditCost is negative ({csc.directorCreditCost}).");
            }
            else if (csc.directorCreditCost == 0)
            {
                Log.Warning("CharacterSpawnCard.directorCreditCost is zero. This will spawn stupid numbers of this enemy if put into a Combat Director.");
            }
        }
    }
}