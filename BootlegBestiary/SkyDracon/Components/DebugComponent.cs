using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace BootlegBestiary.SkyDracon.Components
{
    public class DebugComponent : MonoBehaviour
    {
        private BaseAI baseAI;
        private CharacterBody body;
        private CharacterMaster master;
        private AISkillDriver lastDriver;
        public void Start()
        {
            body = GetComponent<CharacterBody>();
            if (body != null)
            {
                master = body.master;
                if (master != null)
                {
                    baseAI = master.gameObject.GetComponent<BaseAI>();
                }
            }
        }
        public void FixedUpdate()
        {
            if (baseAI != null)
            {
                AISkillDriver currentDriver = baseAI.skillDriverEvaluation.dominantSkillDriver;
                if (currentDriver != lastDriver)
                {
                    //Log.Debug($"Driver update! Changed from: {(lastDriver == null ? "null" : $"{lastDriver.customName}")} to {(currentDriver == null ? "null" : $"{currentDriver.customName}")}");
                    lastDriver = currentDriver;
                }
            }
        }
    }
}
