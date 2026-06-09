using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using R2API;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BootlegBestiary.Shared.Assets
{
    internal static class CustomAssets
    {
        private static AssetBundle assetBundle;

        //MARROWING
        public static GameObject SkyDraconBodyPrefab { get; private set; }
        public static GameObject SkyDraconMasterPrefab { get; private set; }
        public static CharacterSpawnCard SkyDraconSpawnCard { get; private set; }
        public static ItemDisplayRuleSet SkyDraconItemDisplayRuleset { get; private set; }

        //BATHYLOPOD
        public static GameObject DemineurBodyPrefab { get; private set; }
        public static GameObject DemineurMasterPrefab { get; private set; }
        public static CharacterSpawnCard DemineurSpawnCard { get; private set; }
        public static GameObject InkSprayEffect { get; private set; }
        public static ItemDisplayRuleSet DemineurItemDisplayRuleset { get; private set; }

        public static void Init()
        {
            Shader replacementShader = Addressables.LoadAssetAsync<Shader>(RoR2_Base_Shaders.HGStandard_shader).WaitForCompletion();
            assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(BootlegBestiary.Instance.Info.Location), "BootlegBestiary"));
            if (assetBundle == null)
            {
                Log.Error($"Failed to load AssetBundle!");
                return;
            }
            foreach (Material material in assetBundle.LoadAllAssets<Material>())
            {
                material.shader = replacementShader;
            }
            SkyDraconBodyPrefab = Load<GameObject>("SkyDraconBody");
            SkyDraconMasterPrefab = Load<GameObject>("SkyDraconMaster");
            SkyDraconSpawnCard = Load<CharacterSpawnCard>("cscSkyDracon");
            SkyDraconItemDisplayRuleset = Load<ItemDisplayRuleSet>("idrsSkyDracon");

            DemineurBodyPrefab = Load<GameObject>("DemineurBody");
            DemineurMasterPrefab = Load<GameObject>("DemineurMaster");
            DemineurSpawnCard = Load<CharacterSpawnCard>("cscDemineur");
            InkSprayEffect = Load<GameObject>("InkSprayEffect");
            DemineurItemDisplayRuleset = Load<ItemDisplayRuleSet>("idrsDemineur");
        }
        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = assetBundle.LoadAsset<T>(path);
            if (asset == null)
            {
                Log.Error($"Asset under path {path} could not be found!");
            }
            return asset;
        }
    }
    internal static class VanillaAssets
    {
        public static GameObject BeetleGuardSlamEffect { get; private set; }
        public static GameObject MiniMushrumPlantEffect { get; private set; }
        public static GameObject BellPartsImpactEffect { get; private set; }
        public static GameObject OmniExplosionEffect { get; private set; }
        public static GameObject LemBruiserFlamebreathChargeEffect { get; private set; }
        public static GameObject GroundOnlyTargetIndicator { get; private set; }
        public static GameObject LemBruiserFlamebreathEffect { get; private set; }
        public static DirectorCardCategorySelection DccsRoostBase { get; private set; }
        public static DirectorCardCategorySelection DccsAphelianBase { get; private set; }
        public static DirectorCardCategorySelection DccsAqueductBase { get; private set; }
        public static DirectorCardCategorySelection DccsAcresBase { get; private set; }
        public static DirectorCardCategorySelection DccsAlluviumBase { get; private set; }
        public static DirectorCardCategorySelection DccsAlluviumDLC2 { get; private set; }
        public static DirectorCardCategorySelection DccsAurorasBase { get; private set; }
        public static DirectorCardCategorySelection DccsAurorasDLC2 { get; private set; }
        public static DirectorCardCategorySelection DccsAbyssalBase { get; private set; }
        public static DirectorCardCategorySelection DccsAbyssalDLC3 { get; private set; }
        public static DirectorCardCategorySelection DccsCraterBase { get; private set; }
        public static DirectorCardCategorySelection DccsWetlandBase { get; private set; }
        public static DirectorCardCategorySelection DccsSulfurBase { get; private set; }
        public static DirectorCardCategorySelection DccsMeadowBase { get; private set; }
        public static DirectorCardCategorySelection DccsHelminthBase { get; private set; }
        public static DirectorCardCategorySelection DccsHelminthDLC3 { get; private set; }

        
        public static CharacterSpawnCard CscScorchling { get; private set; }
        public static CharacterSpawnCard CscExtractor { get; private set; }
        public static CharacterSpawnCard CscTanker { get; private set; }

        public static EquipmentDef EDEliteFire { get; private set; }
        public static EquipmentDef EDEliteLightning { get; private set; }
        public static EquipmentDef EDEliteIce { get; private set; }
        public static EquipmentDef EDElitePoison { get; private set; }
        public static EquipmentDef EDEliteHaunted { get; private set; }
        public static EquipmentDef EDEliteEarth { get; private set; }
        public static EquipmentDef EDEliteVoid { get; private set; }
        public static EquipmentDef EDEliteAurelionite { get; private set; }
        public static EquipmentDef EDEliteBead {  get; private set; }
        public static EquipmentDef EDEliteCollective { get; private set; }
        public static EquipmentDef EDEliteLunar { get; private set; }

        public static GameObject DisplayAffixFire { get; private set; }
        public static GameObject DisplayAffixLightning { get; private set; }
        public static GameObject DisplayAffixIce { get; private set; }
        public static GameObject DisplayAffixPoison { get; private set; }
        public static GameObject DisplayAffixHaunted { get; private set; }
        public static GameObject DisplayAffixEarth { get; private set; }
        public static GameObject DisplayAffixVoid { get; private set; }
        public static GameObject DisplayAffixAurelionite { get; private set; }
        public static GameObject DisplayAffixBead { get; private set; }
        public static GameObject DisplayAffixCollective { get; private set; }
        public static GameObject DisplayAffixCollectiveRing { get; private set; }
        public static GameObject DisplayAffixLunar { get; private set; }
        public static GameObject DisplayAffixLunarFire { get; private set; }

        public static Material InkMaterial { get; set; }

        public static void Init()
        {
            //VFX
            BeetleGuardSlamEffect = Load<GameObject>(RoR2_Base_BeetleGuard.BeetleGuardGroundSlam_prefab);
            MiniMushrumPlantEffect = Load<GameObject>(RoR2_Base_MiniMushroom.MiniMushroomPlantEffect_prefab);
            BellPartsImpactEffect = Load<GameObject>(RoR2_Base_Bell.BellBodyPartsImpact_prefab);
            OmniExplosionEffect = Load<GameObject>(RoR2_Base_Common_VFX.OmniExplosionVFX_prefab);
            LemBruiserFlamebreathChargeEffect = Load<GameObject>(RoR2_Base_Lemurian.LemurianChargeFire_prefab);
            GroundOnlyTargetIndicator = Load<GameObject>(RoR2_Base_Common.TeamAreaIndicator__GroundOnly_prefab);
            LemBruiserFlamebreathEffect = Load<GameObject>(RoR2_Base_Lemurian.FlamebreathEffect_prefab);

            //CSC
            CscExtractor = Load<CharacterSpawnCard>(RoR2_DLC3_ExtractorUnit.cscExtractorUnit_asset);
            CscScorchling = Load<CharacterSpawnCard>(RoR2_DLC2_Scorchling.cscScorchling_asset);
            CscTanker = Load<CharacterSpawnCard>(RoR2_DLC3_Tanker.cscTanker_asset);

            //DCCS
            DccsRoostBase = Load<DirectorCardCategorySelection>(RoR2_Base_blackbeach.dccsBlackBeachMonsters_asset);
            DccsAphelianBase = Load<DirectorCardCategorySelection>(RoR2_DLC1_ancientloft.dccsAncientLoftMonstersDLC1_asset);
            DccsAqueductBase = Load<DirectorCardCategorySelection>(RoR2_Base_goolake.dccsGooLakeMonsters_asset);
            DccsWetlandBase = Load<DirectorCardCategorySelection>(RoR2_Base_foggyswamp.dccsFoggySwampMonsters_asset);
            DccsAcresBase = Load<DirectorCardCategorySelection>(RoR2_Base_wispgraveyard.dccsWispGraveyardMonsters_asset);
            DccsSulfurBase = Load<DirectorCardCategorySelection>(RoR2_DLC1_sulfurpools.dccsSulfurPoolsMonstersDLC1_asset);
            DccsAlluviumBase = Load<DirectorCardCategorySelection>(RoR2_DLC3_ironalluvium.dccsIronalluviumMonsters_asset);
            DccsAurorasBase = Load<DirectorCardCategorySelection>(RoR2_DLC3_ironalluvium2.dccsIronalluvium2Monsters_asset);
            DccsAlluviumDLC2 = Load<DirectorCardCategorySelection>(RoR2_DLC3_ironalluvium.dccsIronalluviumMonstersDLC2_asset);
            DccsAurorasDLC2 = Load<DirectorCardCategorySelection>(RoR2_DLC3_ironalluvium2.dccsIronalluvium2MonstersDLC2_asset);
            DccsAbyssalBase = Load<DirectorCardCategorySelection>(RoR2_Base_dampcave.dccsDampCaveMonsters_asset);
            DccsAbyssalDLC3 = Load<DirectorCardCategorySelection>(RoR2_Base_dampcave.dccsDampCaveMonstersDLC3_asset);
            DccsCraterBase = Load<DirectorCardCategorySelection>(RoR2_DLC3_repurposedcrater.dccsRepurposedcraterMonsters_asset);
            DccsMeadowBase = Load<DirectorCardCategorySelection>(RoR2_Base_skymeadow.dccsSkyMeadowMonsters_asset);
            DccsHelminthBase = Load<DirectorCardCategorySelection>(RoR2_DLC2_helminthroost.dccsHelminthRoostMonsters_asset);
            DccsHelminthDLC3 = Load<DirectorCardCategorySelection>(RoR2_DLC2_helminthroost.dccsHelminthRoostMonstersDLC3_asset);

            //Equipment Defs
            EDEliteFire = Load<EquipmentDef>(RoR2_Base_EliteFire.EliteFireEquipment_asset);
            EDEliteLightning = Load<EquipmentDef>(RoR2_Base_EliteLightning.EliteLightningEquipment_asset);
            EDEliteIce = Load<EquipmentDef>(RoR2_Base_EliteIce.EliteIceEquipment_asset);
            EDElitePoison = Load<EquipmentDef>(RoR2_Base_ElitePoison.ElitePoisonEquipment_asset);
            EDEliteHaunted = Load<EquipmentDef>(RoR2_Base_EliteHaunted.EliteHauntedEquipment_asset);
            EDEliteEarth = Load<EquipmentDef>(RoR2_DLC1_EliteEarth.EliteEarthEquipment_asset);
            EDEliteVoid = Load<EquipmentDef>(RoR2_DLC1_EliteVoid.EliteVoidEquipment_asset);
            EDEliteAurelionite = Load<EquipmentDef>(RoR2_DLC2_Elites_EliteAurelionite.EliteAurelioniteEquipment_asset);
            EDEliteBead = Load<EquipmentDef>(RoR2_DLC2_Elites_EliteBead.EliteBeadEquipment_asset);
            EDEliteCollective = Load<EquipmentDef>(RoR2_DLC3_Collective.EliteCollectiveEquipment_asset);

            //Elite Displays
            DisplayAffixFire = Load<GameObject>(RoR2_Base_EliteFire.DisplayEliteHorn_prefab);
            DisplayAffixLightning = Load<GameObject>(RoR2_Base_EliteLightning.DisplayEliteRhinoHorn_prefab);
            DisplayAffixIce = Load<GameObject>(RoR2_Base_EliteIce.DisplayEliteIceCrown_prefab);
            DisplayAffixPoison = Load<GameObject>(RoR2_Base_ElitePoison.DisplayEliteUrchinCrown_prefab);
            DisplayAffixHaunted = Load<GameObject>(RoR2_Base_EliteHaunted.DisplayEliteStealthCrown_prefab);
            DisplayAffixEarth = Load<GameObject>(RoR2_DLC1_EliteEarth.DisplayEliteMendingAntlers_prefab);
            DisplayAffixVoid = Load<GameObject>(RoR2_DLC1_EliteVoid.DisplayAffixVoid_prefab);
            DisplayAffixAurelionite = Load<GameObject>(RoR2_DLC2_Elites_EliteAurelionite.DisplayEliteAurelioniteEquipment_prefab);
            DisplayAffixBead = Load<GameObject>(RoR2_DLC2_Elites_EliteBead.DisplayEliteBeadSpike_prefab);
            DisplayAffixCollective = Load<GameObject>(RoR2_DLC3_Collective.DisplayEliteCollectiveHorn_prefab);
            DisplayAffixCollectiveRing = Load<GameObject>(RoR2_DLC3_Collective.DisplayEliteCollectiveRing_prefab);
            DisplayAffixLunar = Load<GameObject>(RoR2_Base_EliteLunar.DisplayEliteLunar_Eye_prefab);
            DisplayAffixLunarFire = Load<GameObject>(RoR2_Base_EliteLunar.DisplayEliteLunar__Fire_prefab);

            //Materials
            InkMaterial = Load<Material>(RoR2_Base_Clay.matClayBubble_mat);
        }
        private static T Load<T>(string path) where T: UnityEngine.Object
        {
            T asset = Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
            if (asset == null)
            {
                Log.Error($"Asset under path {path} could not be found!");
            }
            return asset;
        }
    }
    internal static class ClonedAssets
    {
        public static GameObject SkyDraconFlamebreathEffect { get; private set; }
        public static void Init()
        {
            SkyDraconFlamebreathEffect = VanillaAssets.LemBruiserFlamebreathEffect.InstantiateClone("SkyDraconFlamebreathEffect");
        }
    }
    internal static class ModdedAssets
    {
        public static readonly string FogboundLagoonString = "FBLScene";
        public static readonly string BroadcastPerchString = "broadcastperch_wormsworms";
        public static readonly string WetlandDownpourString = "foggyswampdownpour";
        public static readonly string HollowSummitString = "hollowsummit_wormsworms";
        public static readonly string FrozenSummitString = "hollowsummitnight_wormsworms";
        public static readonly string AncientObservatoryString = "observatory_wormsworms";
        public static readonly string SunkenTombsString = "sunkentombs_wormsworms";
        public static readonly string SunsetTropicsString = "tropics_wormsworms";
        public static readonly string MidnightTropicsString = "tropicsnight_wormsworms";

        public static readonly string ITBroadcastPerchString = "itbroadcastperch_wormsworms";
        public static readonly string ITWetlandDownpourString = "itfoggyswampdownpour";
        public static readonly string ITAncientObservatoryString = "itobservatory_wormsworms";
        public static readonly string ITHollowSummitString = "itsummit_wormsworms";
        public static readonly string ITSunkenTombsString = "itsunkentombs_wormsworms";
        public static readonly string ITSunsetTropicsString = "ittropics_wormsworms";
    }
}
