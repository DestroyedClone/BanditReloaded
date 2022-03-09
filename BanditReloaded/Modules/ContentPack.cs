﻿using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using R2API;
using RoR2.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;
using RoR2.ContentManagement;
using System.Collections;


namespace BanditReloaded.Modules
{
    public class BanditContent : IContentPackProvider
    {
        internal static ContentPack contentPack = new ContentPack();

        public static AssetBundle assetBundle;

        public static BuffDef lightsOutBuff;
        public static BuffDef thermiteBuff;
        public static BuffDef cloakDamageBuff;
        public static BuffDef skullBuff;

        //public static UnlockableDef masteryUnlock;

        //public static SurvivorDef banditReloadedSurvivor;

        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<Type> entityStates = new List<Type>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();

        public string identifier => "BanditReloaded.content";

        public static void LoadResources()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BanditReloaded.banditbundle"))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
            }

            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BanditReloaded.BanditReloaded.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                R2API.SoundAPI.SoundBanks.Add(bytes);
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            CreateBuffs();
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());
            contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.survivorDefs.Add(survivorDefs.ToArray());
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void FixScriptableObjectName(BuffDef buff)
        {
            (buff as ScriptableObject).name = buff.name;
        }

        public void CreateBuffs()
        {
            BuffDef spotterDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterDef.buffColor = new Color(1f, 1f, 1f);
            spotterDef.canStack = false;
            spotterDef.isDebuff = false;
            spotterDef.name = "SniperClassicSpotted";
            spotterDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterReady.png");
            FixScriptableObjectName(spotterDef);
            SniperContent.buffDefs.Add(spotterDef);
            SniperContent.spotterBuff = spotterDef;

            BuffDef spotterCooldownDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterCooldownDef.buffColor = new Color(1f, 1f, 1f);
            spotterCooldownDef.canStack = true;
            spotterCooldownDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterCooldown.png");
            spotterCooldownDef.isDebuff = false;
            spotterCooldownDef.name = "SniperClassicSpottedCooldown";
            FixScriptableObjectName(spotterCooldownDef);
            SniperContent.buffDefs.Add(spotterCooldownDef);
            SniperContent.spotterCooldownBuff = spotterCooldownDef;

            BuffDef spotterStatDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterStatDebuffDef.buffColor = new Color(0.8392157f, 0.227450982f, 0.227450982f);
            spotterStatDebuffDef.canStack = false;
            spotterStatDebuffDef.iconSprite = LegacyResourcesAPI.Load<Sprite>("textures/bufficons/texbuffweakicon");
            spotterStatDebuffDef.isDebuff = true;
            spotterStatDebuffDef.name = "SniperClassicSpottedStatDebuff";
            FixScriptableObjectName(spotterStatDebuffDef);
            SniperContent.buffDefs.Add(spotterStatDebuffDef);
            SniperContent.spotterStatDebuff = spotterStatDebuffDef;

            BuffDef spotterScepterDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterScepterDef.buffColor = new Color(78f * 2f / 255f, 80f * 2f / 255f, 111f * 2f / 255f);
            spotterScepterDef.canStack = false;
            spotterScepterDef.isDebuff = false;
            spotterScepterDef.name = "SniperClassicSpottedScepter";
            spotterScepterDef.iconSprite = LegacyResourcesAPI.Load<Sprite>("textures/bufficons/texbuffcloakicon");
            FixScriptableObjectName(spotterScepterDef);
            SniperContent.buffDefs.Add(spotterScepterDef);
            SniperContent.spotterScepterBuff = spotterScepterDef;

            BuffDef spotterPlayerReadyDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterPlayerReadyDef.buffColor = new Color(1f, 1f, 1f);
            spotterPlayerReadyDef.canStack = false;
            spotterPlayerReadyDef.isDebuff = false;
            spotterPlayerReadyDef.name = "SniperClassicSpotterPlayerReady";
            spotterPlayerReadyDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterReady.png");
            FixScriptableObjectName(spotterPlayerReadyDef);
            SniperContent.buffDefs.Add(spotterPlayerReadyDef);
            SniperContent.spotterPlayerReadyBuff = spotterPlayerReadyDef;

            BuffDef spotterPlayerCooldownDef = ScriptableObject.CreateInstance<BuffDef>();
            spotterPlayerCooldownDef.buffColor = new Color(1f, 1f, 1f);
            spotterPlayerCooldownDef.canStack = true;
            spotterPlayerCooldownDef.iconSprite = SniperContent.assetBundle.LoadAsset<Sprite>("BuffSpotterCooldown.png");
            spotterPlayerCooldownDef.isCooldown = true;
            spotterPlayerCooldownDef.isDebuff = false;
            spotterPlayerCooldownDef.name = "SniperClassicSpotterPlayerCooldown";
            FixScriptableObjectName(spotterPlayerCooldownDef);
            SniperContent.buffDefs.Add(spotterPlayerCooldownDef);
            SniperContent.spotterPlayerCooldownBuff = spotterPlayerCooldownDef;
        }

    }
}
