using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootlegBestiary.SkyDracon.EntityStates;
using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace BootlegBestiary.SkyDracon.Components
{
    public class SkyDraconBehaviourController : MonoBehaviour
    {
        private BaseAI baseAI;
        private CharacterBody body;
        public bool canUseSkills;
        private float abilityCheckTimer;
        private static float abilityCheckInterval = 0.25f;
        public EntityStateMachine flightMachine;
        private int originalLayer;
        private GameObject model;
        private Animator animator;
        private CharacterMotor motor;
        private List<AISkillDriver> groundedBehaviourDrivers = new List<AISkillDriver>();
        private bool groundedDriversEnabled;
        private EntityStateMachine bodyMachine;
        private bool collisionRemoved;
        private ChildLocator modelChildLocator;
        private Transform neckTransform;
        private Transform armLTransform;
        private Transform armRTransform;
        private Transform tailTransform;
        private uint soundID;
        private bool stoppedSoundOnDeath;
        private int stuckFrames;
        private static int maxStuckFrames = 2;
        public enum DraconState
        {
            Flying,
            Squirming,
            Burrowed,
            Grounded
        };
        public DraconState draconState;
        public void Start()
        {
            draconState = DraconState.Flying;
            body = GetComponent<CharacterBody>();
            if (body != null)
            {
                motor = body.characterMotor;
                if (body.modelLocator != null && body.modelLocator.modelTransform != null)
                {
                    model = body.modelLocator.modelTransform.gameObject;
                    if (model != null)
                    {
                        animator = model.GetComponent<Animator>();
                        modelChildLocator = model.GetComponent<ChildLocator>();
                        if (modelChildLocator != null)
                        {
                            armLTransform = modelChildLocator.FindChild("ArmL");
                            armRTransform = modelChildLocator.FindChild("ArmR");
                            neckTransform = modelChildLocator.FindChild("Neck");
                            tailTransform = modelChildLocator.FindChild("TailCutoff");
                        }
                    }
                }
                if (body.master != null)
                {
                    baseAI = body.master.gameObject.GetComponent<BaseAI>();
                    groundedBehaviourDrivers = body.master.gameObject.GetComponents<AISkillDriver>().Where(driver => driver.customName.Contains("Grounded")).ToList();
                    foreach (var driver in groundedBehaviourDrivers)
                    {
                        driver.enabled = false;
                    }
                }
                flightMachine = body.GetComponents<EntityStateMachine>().Where(esm => esm.customName == "ControlFlight").FirstOrDefault();
                bodyMachine = body.GetComponents<EntityStateMachine>().Where(esm => esm.customName == "Body").FirstOrDefault();
            }
            abilityCheckTimer = abilityCheckInterval;

        }
        public void FixedUpdate()
        {
            //the unstuckifier
            if (bodyMachine.state is SkyDraconCharacterMain && draconState == DraconState.Squirming)
            {
                stuckFrames++;
                if (stuckFrames > maxStuckFrames)
                {
                    BeginBurrowedState();
                    bodyMachine.SetNextState(new BurrowRelocate());
                    stuckFrames = 0;
                    return;
                }
            }
            else if (stuckFrames > 0)
            {
                stuckFrames = 0;
            }

            abilityCheckTimer -= Time.fixedDeltaTime;
            if (abilityCheckTimer < 0f)
            {
                abilityCheckTimer += abilityCheckInterval;
                canUseSkills = CanUsePrimaryOrSecondary();
            }
            if (draconState == DraconState.Burrowed || draconState == DraconState.Grounded)
            {
                if (!groundedDriversEnabled)
                {
                    groundedDriversEnabled = true;
                    foreach (AISkillDriver driver in groundedBehaviourDrivers)
                    {
                        driver.enabled = true;
                    }
                }
            }
            else
            {
                if (groundedDriversEnabled)
                {
                    groundedDriversEnabled = false;
                    foreach (AISkillDriver driver in groundedBehaviourDrivers)
                    {
                        driver.enabled = false;
                    }
                }
            }
            if (!stoppedSoundOnDeath)
            {
                if (body == null || body.healthComponent == null)
                {
                    stoppedSoundOnDeath = true;
                    AkSoundEngine.StopPlayingID(soundID);
                }
                else if (body != null && body.healthComponent != null && body.healthComponent.alive == false)
                {
                    stoppedSoundOnDeath = true;
                    AkSoundEngine.StopPlayingID(soundID);
                }
            }
        }
        private bool CanUsePrimaryOrSecondary()
        {
            canUseSkills = false;
            if (draconState != DraconState.Flying)
            {
                return false;
            }
            if (body != null && baseAI != null && baseAI.currentEnemy.hasLoS == true)
            {
                CharacterBody targetBody = baseAI.currentEnemy.characterBody;
                if (targetBody != null)
                {
                    float targetYPos = targetBody.transform.position.y;
                    float draconYPos = body.transform.position.y;
                    if (draconYPos > targetYPos && draconYPos < targetYPos + 35f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void RemoveCollision()
        {
            if (motor != null && !collisionRemoved)
            {
                originalLayer = body.gameObject.layer;
                body.gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(body.teamComponent.teamIndex).intVal;
                motor.Motor.RebuildCollidableLayers();
                collisionRemoved = true;
            }
        }
        public void RestoreCollision()
        {
            if (motor != null && collisionRemoved)
            {
                collisionRemoved = false;
                body.gameObject.layer = originalLayer;
                motor.Motor.RebuildCollidableLayers();
            }
        }
        public void EnterSquirmState()
        {
            if (flightMachine == null)
            {
                return;
            }
            flightMachine.SetNextState(new Idle());
            RemoveCollision();
            draconState = DraconState.Squirming;
            motor.enabled = false;
            ToggleUpperBodyVisibility(false);
        }
        public void BeginBurrowedState()
        {
            draconState = DraconState.Burrowed;
            soundID = Util.PlaySound("Play_skyDracon_dig", this.gameObject);
            if (model != null)
            {
                model.SetActive(false);
            }
            if (animator != null)
            {
                animator.SetFloat("diveBombAngleCycle", 0.5f);
            }
        }
        public void ExitBurrowIntoGroundedState()
        {
            if (model != null)
            {
                model.SetActive(true);
            }
            RestoreCollision();
            draconState = DraconState.Grounded;
            ToggleUpperBodyVisibility(true);
            Util.PlaySound("Stop_skyDracon_dig", body.gameObject);
            //yes, it is unborrow and not unburrow
            Util.PlaySound("Play_miniMushroom_unborrow", this.gameObject);
        }
        public void ExitBurrowIntoAirborneState()
        {
            if (model != null)
            {
                model.SetActive(true);
            }
            RestoreCollision();
            motor.enabled = true;
            motor.velocity = Vector3.zero;
            draconState = DraconState.Flying;
            flightMachine.SetNextState(new Flight());
            ToggleUpperBodyVisibility(true);
            Util.PlaySound("Stop_skyDracon_dig", body.gameObject);
        }
        public void ToggleUpperBodyVisibility(bool active)
        {
            if (neckTransform != null)
            {
                neckTransform.gameObject.SetActive(active);
            }
            if (armLTransform != null)
            {
                armLTransform.gameObject.SetActive(active);
            }
            if (armRTransform != null)
            {
                armRTransform.gameObject.SetActive(active);
            }
        }
        public void ToggleTailVisibility(bool active)
        {

        }
    }
}
