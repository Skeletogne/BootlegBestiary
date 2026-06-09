using System.Collections.Generic;
using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
using RoR2.Navigation;
using BootlegBestiary.SkyDracon.Components;
using BootlegBestiary.Shared.Assets;

namespace BootlegBestiary.SkyDracon.EntityStates
{
    public class BurrowRelocate : BaseSkillState
    {
        private static float burrowDuration = 1.25f;
        private static float emergeDuration = 1f;
        private static float unburrowDelay = 0.5f;
        private float effectTimer;
        private static float effectInterval = 0.2f;
        private static GameObject burrowEffect = VanillaAssets.MiniMushrumPlantEffect;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool completedMovement;
        private SkyDraconBehaviourController controller;
        public override void OnEnter()
        {
            base.OnEnter();
            Animator animator = GetModelAnimator();
            if (animator != null)
            {
                animator.SetFloat("diveBombAngleCycle", 0.5f);
            }
            if (skillLocator != null && skillLocator.utility != null)
            {
                skillLocator.utility.RemoveAllStocks();
            }
            controller = characterBody.gameObject.GetComponent<SkyDraconBehaviourController>();
            startPosition = characterBody.transform.position;
            targetPosition = startPosition;
            Vector3 nodeSearchPosition = characterBody.transform.position;
            if (characterBody != null && characterBody.master != null)
            {
                BaseAI ai = characterBody.master.gameObject.GetComponent<BaseAI>();
                if (ai != null && ai.currentEnemy.characterBody != null)
                {
                    Vector3 targetPosition = ai.currentEnemy.characterBody.transform.position;
                    Vector3 currentPosition = characterBody.transform.position;
                    float distance = Vector3.Distance(targetPosition, currentPosition);
                    if (distance < 40f)
                    {
                        nodeSearchPosition = targetPosition;
                    }
                }
            }
            NodeGraph nodeGraph = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = GetFavoredNodeIndex(nodeSearchPosition, nodeGraph);
            if (nodeGraph.GetNodePosition(nodeIndex, out Vector3 raycastStartPosition))
            {
                int iterations = 0;
                bool foundSuitablePosition = false;
                while (iterations < 6 && !foundSuitablePosition)
                {
                    float randomXOffset = UnityEngine.Random.Range(-2f, 2f);
                    float randomZOffset = UnityEngine.Random.Range(-2f, 2f);
                    raycastStartPosition.x += randomXOffset;
                    raycastStartPosition.z += randomZOffset;
                    raycastStartPosition.y += 5f;
                    if (Physics.Raycast(raycastStartPosition, Vector3.down, out RaycastHit hit, 10f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        targetPosition = hit.point;
                        foundSuitablePosition = true;
                    }
                    iterations++;
                }
            }
        }
        public NodeGraph.NodeIndex GetFavoredNodeIndex(Vector3 nodeSearchPosition, NodeGraph nodeGraph)
        {
            List<NodeGraph.NodeIndex> nodeIndices = nodeGraph.FindNodesInRange(nodeSearchPosition, 10f, 30f, HullMask.Human);
            if (nodeIndices.Count == 0)
            {
                NodeGraph.NodeIndex nodeIndex = nodeGraph.FindClosestNode(nodeSearchPosition, HullClassification.Human);
                return nodeIndex;
            }
            return nodeIndices[UnityEngine.Random.RandomRangeInt(0, nodeIndices.Count)];

        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float completionPercentage = Mathf.Clamp01(base.fixedAge / burrowDuration);
            Vector3 lerpPosition = Vector3.Lerp(startPosition, targetPosition, completionPercentage);
            characterBody.transform.position = lerpPosition;
            effectTimer -= Time.fixedDeltaTime;
            if (effectTimer < 0)
            {
                effectTimer += effectInterval;
                Vector3 raycastStartPosition = characterBody.transform.position;
                raycastStartPosition.y = Mathf.Max(targetPosition.y, startPosition.y) + 1f;
                if (Physics.Raycast(raycastStartPosition, Vector3.down, out RaycastHit hit, 25f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 effectPoint = hit.point;
                    EffectManager.SpawnEffect(burrowEffect, new EffectData { origin = effectPoint, scale = 1f }, true);
                }
            }
            if (base.fixedAge > burrowDuration + unburrowDelay && !completedMovement)
            {
                completedMovement = true;
                controller.ExitBurrowIntoGroundedState();
                PlayAnimation("Base", "Unearth");
            }
            if (base.fixedAge > burrowDuration + unburrowDelay + emergeDuration)
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
