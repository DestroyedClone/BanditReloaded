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
            ModelSkinController skinController = modelTransform.GetComponent<ModelSkinController>();
            ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            List<SkinDef> skinDefs = new List<SkinDef>();

            if (!skinController)
                skinController = modelTransform.AddComponent<ModelSkinController>();

            if (Modules.Config.useOldModel)
            {
                //BanditBody.AddComponent<ClassicMenuAnimComponent>();  //seems to be broken
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

            #region GameobjectActivation

            #endregion

            #region default
            if (Modules.Config.useOldModel)
            {
                // credits to rob
                SkinDefInfo defaultSkinDefInfo = new SkinDefInfo();
                defaultSkinDefInfo.Name = "DEFAULT_SKIN";
                defaultSkinDefInfo.NameToken = "DEFAULT_SKIN";         //actual skin icon coming soon
                defaultSkinDefInfo.Icon = R2API.LoadoutAPI.CreateSkinIcon(new Color(143f / 255f, 132f / 255f, 106f / 255f), Color.cyan, new Color(92f / 255f, 136f / 255f, 167f / 255f), new Color(25f / 255f, 50f / 255f, 57f / 255f));
                defaultSkinDefInfo.RootObject = modelTransform;

                defaultSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
                defaultSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
                defaultSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

                defaultSkinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();

                //enter a list of strings in order of rendererinfos
                //null or empty strings simply skips mesh replacement
                defaultSkinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        renderer = mainRenderer,
                        mesh = mainRenderer.sharedMesh
                    }
                };

                defaultSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;

                SkinDef defaultSkinDef = Skins.CreateSkinDef(defaultSkinDefInfo);
                skinDefs.Add(defaultSkinDef);
            }

            #endregion

            skinController.skins = skinDefs.ToArray();
        }
    }
}