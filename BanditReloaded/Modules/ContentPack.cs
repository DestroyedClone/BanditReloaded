using RoR2;
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
using BanditReloaded;
using BanditReloaded.Components;


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
            BuffDef LightsOutBuffDef = BuffDef.CreateInstance<BuffDef>();
            LightsOutBuffDef.buffColor = BanditReloaded.BanditColor;
            LightsOutBuffDef.canStack = false;
            LightsOutBuffDef.isDebuff = true;
            LightsOutBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffFullCritIcon");
            LightsOutBuffDef.name = "BanditReloadedMarkedForDeath";
            BanditContent.buffDefs.Add(LightsOutBuffDef);
            BanditContent.lightsOutBuff = LightsOutBuffDef;

            BuffDef ThermiteBuffDef = BuffDef.CreateInstance<BuffDef>();
            ThermiteBuffDef.buffColor = BanditReloaded.BanditColor;
            ThermiteBuffDef.canStack = true;
            ThermiteBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffOnFireIcon");
            ThermiteBuffDef.isDebuff = true;
            ThermiteBuffDef.name = "BanditReloadedThermite";
            BanditContent.buffDefs.Add(ThermiteBuffDef);
            BanditContent.thermiteBuff = ThermiteBuffDef;

            BuffDef cloakDamageBuffDef = BuffDef.CreateInstance<BuffDef>();
            cloakDamageBuffDef.buffColor = BanditReloaded.BanditColor;
            cloakDamageBuffDef.canStack = false;
            cloakDamageBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffFullCritIcon");
            cloakDamageBuffDef.name = "BanditReloadedCloakDamage";
            cloakDamageBuffDef.isDebuff = false;
            BanditContent.buffDefs.Add(cloakDamageBuffDef);
            BanditContent.cloakDamageBuff = cloakDamageBuffDef;

            BuffDef skullBuffDef = BuffDef.CreateInstance<BuffDef>();
            skullBuffDef.buffColor = BanditReloaded.BanditColor;
            skullBuffDef.canStack = true;
            skullBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffBanditSkullIcon");
            skullBuffDef.isDebuff = true;
            skullBuffDef.name = "BanditReloadedSkull";
            BanditContent.buffDefs.Add(skullBuffDef);
            BanditContent.skullBuff = skullBuffDef;
        }

    }
}
