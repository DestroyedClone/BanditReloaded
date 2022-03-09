using System;
using UnityEngine;
using R2API;
using RoR2;
using R2API.Utils;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
//using BanditReloaded.Modules.Achievements;
using static R2API.LoadoutAPI;
using BanditReloaded;

namespace BanditReloaded.Modules
{
    public static class BanditSkins
    {

        public static void RegisterSkins()
        {
            GameObject bodyPrefab = BanditReloaded.BanditBody;
            GameObject modelTransform = bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            ModelSkinController skinController = modelTransform.AddComponent<ModelSkinController>();
            ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            List<SkinDef> skinDefs = new List<SkinDef>();

            if (Modules.Config.useOldModel)
            {
                //BanditBody.AddComponent<ClassicMenuAnimComponent>();  //seems to be broken
                AddClassicSkin();
                On.RoR2.CameraRigController.OnEnable += (orig, self) =>
                {
                    SceneDef sd = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
                    if (sd && sd.baseSceneName.Equals("lobby"))
                    {
                        self.enableFading = false;
                    }
                    orig(self);
                };
            }
            else
            {
                BanditReloaded.BanditBody.GetComponentInChildren<ModelSkinController>().skins[1].unlockableDef = null;
            }

            #region GameobjectActivation

            #endregion

            #region default
            SkinDefInfo defaultSkinDefInfo = new SkinDefInfo();
            defaultSkinDefInfo.Name = "DEFAULT_SKIN";
            defaultSkinDefInfo.NameToken = "DEFAULT_SKIN";         //actual skin icon coming soon
            defaultSkinDefInfo.Icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSniperSkinDefault");
            defaultSkinDefInfo.RootObject = modelTransform;

            defaultSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            defaultSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            defaultSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            defaultSkinDefInfo.GameObjectActivations = getGameObjectActivations();

            //enter a list of strings in order of rendererinfos
            //null or empty strings simply skips mesh replacement
            defaultSkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                "meshSniperDefault_Gun",
                "meshSniperDefault_GunAlt",
                "meshSniperDefault_CriticalHitCarl",
                "meshSniperMastery_Beret",
                "meshSniperDefault"
                );

            defaultSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;

            SkinDef defaultSkinDef = Skins.CreateSkinDef(defaultSkinDefInfo);
            skinDefs.Add(defaultSkinDef);

            #endregion

            #region Mastery
            SkinDefInfo masterySkinDefInfo = new SkinDefInfo();
            masterySkinDefInfo.Name = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masterySkinDefInfo.NameToken = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masterySkinDefInfo.Icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSniperSkinMaster");
            masterySkinDefInfo.UnlockableDef = SniperUnlockables.MasteryUnlockableDef;
            masterySkinDefInfo.RootObject = modelTransform;

            masterySkinDefInfo.BaseSkins = new SkinDef[] { defaultSkinDef };
            masterySkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            masterySkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            masterySkinDefInfo.GameObjectActivations = getGameObjectActivations(beret);

            masterySkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                null,//"meshSniperDefault_Gun",
                null,//"meshSniperDefault_GunAlt",
                null,//"meshSniperDefault_CriticalHitCarl",
                null,//"meshSniperMastery_Beret",
                "meshSniperMastery"
                );

            masterySkinDefInfo.RendererInfos = new CharacterModel.RendererInfo[defaultSkinDef.rendererInfos.Length];
            defaultSkinDef.rendererInfos.CopyTo(masterySkinDefInfo.RendererInfos, 0);


            Material sniperMasterMat = Modules.Assets.CreateMaterial("matSniperMastery", 0.7f, Color.white);
            Material sniperMasterGunMat = Modules.Assets.CreateMaterial("matSniperMastery", 3f, Color.white);// new Color(152f / 255f, 169f / 255f, 216f / 255f));
            Material spotterMasterMat = Modules.Assets.CreateMaterial("matSniperMastery", 2f, Color.white);// new Color(1f, 163f / 255f, 92f / 255f));

            masterySkinDefInfo.RendererInfos[0].defaultMaterial = sniperMasterGunMat;
            masterySkinDefInfo.RendererInfos[1].defaultMaterial = sniperMasterGunMat;
            masterySkinDefInfo.RendererInfos[2].defaultMaterial = spotterMasterMat;
            masterySkinDefInfo.RendererInfos[3].defaultMaterial = sniperMasterMat;
            masterySkinDefInfo.RendererInfos[4].defaultMaterial = sniperMasterMat;

            SkinDef masterySkin = Skins.CreateSkinDef(masterySkinDefInfo);
            skinDefs.Add(masterySkin);
            #endregion

            #region MasteryAlt
            SkinDefInfo masteryAltSkinDefInfo = new SkinDefInfo();
            masteryAltSkinDefInfo.Name = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masteryAltSkinDefInfo.NameToken = "SNIPERCLASSIC_MASTERY_SKIN_NAME";
            masteryAltSkinDefInfo.Icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSniperSkinDefaultCursed");
            masteryAltSkinDefInfo.UnlockableDef = SniperUnlockables.MasteryUnlockableDef;
            masteryAltSkinDefInfo.RootObject = modelTransform;

            masteryAltSkinDefInfo.BaseSkins = new SkinDef[] { masterySkin };

            masteryAltSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            masteryAltSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            masteryAltSkinDefInfo.GameObjectActivations = getGameObjectActivations(beret);

            masteryAltSkinDefInfo.MeshReplacements = masterySkinDefInfo.MeshReplacements;

            masteryAltSkinDefInfo.RendererInfos = defaultSkinDef.rendererInfos;

            SkinDef masteryAltSkin = Skins.CreateSkinDef(masteryAltSkinDefInfo);
            if (Config.altMastery)
            {
                skinDefs.Add(masteryAltSkin);
            }
            #endregion

            #region Grandmastery
            SkinDefInfo grandmasterySkinDefInfo = new SkinDefInfo();
            grandmasterySkinDefInfo.Name = "SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME";
            grandmasterySkinDefInfo.NameToken = "SNIPERCLASSIC_GRANDMASTERY_SKIN_NAME";
            grandmasterySkinDefInfo.Icon = SniperContent.assetBundle.LoadAsset<Sprite>("texSniperSkinGrandmaster");
            grandmasterySkinDefInfo.UnlockableDef = SniperUnlockables.GrandMasteryUnlockableDef;
            grandmasterySkinDefInfo.RootObject = modelTransform;

            grandmasterySkinDefInfo.BaseSkins = new SkinDef[] { defaultSkinDef };
            grandmasterySkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            grandmasterySkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            grandmasterySkinDefInfo.GameObjectActivations = getGameObjectActivations();

            grandmasterySkinDefInfo.MeshReplacements = Skins.getMeshReplacements(characterModel.baseRendererInfos,
                "meshSniperQuentin_Gun",
                null,//"meshSniperDefault_GunAlt",
                "meshSniperQuentin_CriticalHitCarl",
                null,//"meshSniperMaster_Beret",
                "meshSniperQuentin"
                ); ;

            grandmasterySkinDefInfo.RendererInfos = new CharacterModel.RendererInfo[defaultSkinDef.rendererInfos.Length];
            defaultSkinDef.rendererInfos.CopyTo(grandmasterySkinDefInfo.RendererInfos, 0);

            Material snipergrandMasterMat = Modules.Assets.CreateMaterial("matSniperQuentin", 0.9f, Color.white);
            Material snipergrandMasterGunMat = Modules.Assets.CreateMaterial("matSniperQuentin", 3f, Color.white);// new Color(152f / 255f, 169f / 255f, 216f / 255f));
            Material spottergrandMasterMat = Modules.Assets.CreateMaterial("matSniperQuentin", 2f, Color.white);// 

            grandmasterySkinDefInfo.RendererInfos[0].defaultMaterial = snipergrandMasterGunMat;
            grandmasterySkinDefInfo.RendererInfos[1].defaultMaterial = snipergrandMasterGunMat;
            grandmasterySkinDefInfo.RendererInfos[2].defaultMaterial = spottergrandMasterMat;
            grandmasterySkinDefInfo.RendererInfos[3].defaultMaterial = snipergrandMasterMat;
            grandmasterySkinDefInfo.RendererInfos[4].defaultMaterial = snipergrandMasterMat;

            SkinDef grandmasterySkin = Skins.CreateSkinDef(grandmasterySkinDefInfo);
            skinDefs.Add(grandmasterySkin);
            #endregion

            skinController.skins = skinDefs.ToArray();
        }


        public static void AddClassicSkin()    //credits to rob
        {
            GameObject bodyPrefab = BanditReloaded.BanditBody;
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = null;
            if (model.GetComponent<ModelSkinController>())
                skinController = model.GetComponent<ModelSkinController>();
            else
                skinController = model.AddComponent<ModelSkinController>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            if (mainRenderer == null)
            {
                CharacterModel.RendererInfo[] bRI = characterModel.baseRendererInfos;
                if (bRI != null)
                {
                    foreach (CharacterModel.RendererInfo rendererInfo in bRI)
                    {
                        if (rendererInfo.renderer is SkinnedMeshRenderer)
                        {
                            mainRenderer = (SkinnedMeshRenderer)rendererInfo.renderer;
                            break;
                        }
                    }
                    if (mainRenderer != null)
                    {
                        characterModel.mainSkinnedMeshRenderer = mainRenderer;
                    }
                }
            }

            R2API.LanguageAPI.Add("BANDITRELOADEDBODY_DEFAULT_SKIN_NAME", "Default");

            R2API.LoadoutAPI.SkinDefInfo skinDefInfo = new R2API.LoadoutAPI.SkinDefInfo();
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(143f / 255f, 132f / 255f, 106f / 255f), Color.cyan, new Color(92f / 255f, 136f / 255f, 167f / 255f), new Color(25f / 255f, 50f / 255f, 57f / 255f));
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            skinDefInfo.Name = "BANDITRELOADEDBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "BANDITRELOADEDBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            SkinDef defaultSkin = R2API.LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[1]
            {
                defaultSkin,
            };
        }
    }
}