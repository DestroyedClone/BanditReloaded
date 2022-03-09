using BanditReloaded.Components;
using BanditReloaded.Hooks;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.BanditReloadedSkills;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using R2API.Utils;
using R2API;
using RoR2.ContentManagement;
using BanditReloaded.Modules;

namespace BanditReloaded
{
    [BepInDependency("com.bepis.r2api")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(SoundAPI))]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.BanditReloaded_v4", "Bandit Reloaded v4", "4.0.4")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    class BanditReloaded : BaseUnityPlugin
    {
        public static SurvivorDef BanditSurvivorDef;

        public ReloadSkillDef primaryBlastDef, primaryScatterDef;
        public SkillDef utilityDefA, utilityAltDef, thermiteDef, acidBombDef, specialLightsOutDef, clusterBombDef, specialBarrageDef, specialBarrageScepterDef, specialLightsOutScepterDef;

        public static GameObject BanditBody = null;
        GameObject BanditDisplay = null;
        public GameObject BanditMonsterMaster = null;

        public GameObject AcidBombObject = null;
        public GameObject ThermiteObject = null;
        public GameObject ClusterBombObject = null;
        public GameObject ClusterBombletObject = null;

        public GameObject AcidBombGhostObject = null;
        public GameObject ThermiteGhostObject = null;
        public GameObject ClusterBombGhostObject = null;
        public GameObject ClusterBombletGhostObject = null;

        public static Color BanditColor = new Color(0.8039216f, 0.482352942f, 0.843137264f);
        String BanditBodyName = "";

        readonly Shader hotpoo = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/hgstandard");
        public static bool scepterInstalled = false;
        public static PluginInfo pluginInfo;

        public void Awake()
        {
            pluginInfo = Info;
            Setup();
            //Nemesis.Setup();
            AddHooks();
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;

        }
        private void CompatSetup()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
            {
                scepterInstalled = true;
            }
            /*if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Kingpinush.KingKombatArena") && arenaNerf)
            {
                arenaPluginLoaded = true;
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2"))
            {
                starstormInstalled = true;
            }*/
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void SetupScepter()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(specialLightsOutScepterDef, BanditBodyName, SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(specialBarrageScepterDef, BanditBodyName, SkillSlot.Special, 1);
        }
        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new BanditContent());
        }

        public void Start()
        {
            CastSmokescreenNoDelay.destealthMaterial = EntityStates.Commando.CommandoWeapon.CastSmokescreenNoDelay.destealthMaterial;
            Assassinate.chargeupVfxPrefab = EntityStates.Toolbot.ChargeSpear.chargeupVfxPrefab;
            Assassinate.holdChargeVfxPrefab = EntityStates.Toolbot.ChargeSpear.holdChargeVfxPrefab;

            if (Modules.Config.useOldModel)
            {
                Modules.ItemDisplays.RegisterDisplays();
            }
            Modules.ItemDisplays.RegisterDisplays();
        }

        public void Setup()
        {
            CompatSetup();
            Modules.Config.ReadConfig(base.Config);
            //LoadResources();
            BanditContent.LoadResources();
            //Modules.Assets.InitializeAssets();
            CreatePrefab();
            CreateDisplayPrefab();
            SetupStats();
            if (Modules.Config.useOldModel) EnableStatusConditions();
            SetupEffects();
            //Modules.Achievements.SniperUnlockables.RegisterUnlockables();
            Modules.BanditSkins.RegisterSkins();
            AssignSkills();
            if (scepterInstalled) SetupScepter();
            RegisterSurvivor();
            Modules.Tokens.RegisterLanguageTokens();
            CreateMaster();
            BuildProjectiles();
            //SniperContent.SpotterDebuffOnHit = DamageAPI.ReserveDamageType();
            //SniperContent.Shock5sNoDamage = DamageAPI.ReserveDamageType();

            AssignSkills();
            CreateMaster();

            BanditBody.GetComponent<CharacterBody>().preferredPodPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/survivorpod");
        }

        private void BuildProjectiles()
        {
            SetupThermite();
            SetupAcidBomb();
            SetupClusterBomb();
            SetupClusterBomblet();
        }

        private void AddHooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats.RecalculateStatsAPI_GetStatCoefficients;
            //On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy.GlobalEventManager_OnHitEnemy;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage.HealthComponent_TakeDamage;
            //new DetectArenaMode();
            //new ScopeNeedleRifle();
            //new AIDrawAggro();
            //new StealBuffVisuals();
            // FixReloadMenuUI();
        }

        private void SetupEffects()
        {
            Logger.LogWarning("SetupEffects() is empty, maybe you should fix that?");
        }

        private void CreatePrefab()
        {
            BanditBody = R2API.PrefabAPI.InstantiateClone(Modules.Config.useOldModel ? Resources.Load<GameObject>("prefabs/characterbodies/banditbody") : Resources.Load<GameObject>("prefabs/characterbodies/bandit2body"), "BanditReloadedBody", true);
            BanditBodyName = BanditBody.name;

            BanditContent.bodyPrefabs.Add(BanditBody);
        }
        private void CreateDisplayPrefab()
        {
            if (!Modules.Config.useOldModel)
            {
                BanditDisplay = Resources.Load<GameObject>("Prefabs/CharacterDisplays/Bandit2Display");
            }
            else
            {
                BanditDisplay = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<ModelLocator>().modelTransform.gameObject;
            }
        }

        public void RegisterSurvivor()
        {
            BanditSurvivorDef = SurvivorDef.CreateInstance<SurvivorDef>();
            BanditSurvivorDef.cachedName = "BanditReloaded";
            BanditSurvivorDef.bodyPrefab = BanditBody;
            BanditSurvivorDef.descriptionToken = "BANDITRELOADED_BODY_DESC";
            BanditSurvivorDef.displayPrefab = BanditDisplay;
            BanditSurvivorDef.primaryColor = BanditColor;
            BanditSurvivorDef.outroFlavorToken = "BANDITRELOADED_OUTRO_FLAVOR";
            BanditSurvivorDef.desiredSortPosition = 100f;
            BanditContent.survivorDefs.Add(BanditSurvivorDef);
        }

        public void SetupStats()
        {
            if (BanditBody)
            {
                BanditBody.AddComponent<BanditCrosshairComponent>();
                BanditBody.AddComponent<BanditNetworkCommands>();
                CharacterBody cb = BanditBody.GetComponent<CharacterBody>();
                if (cb)
                {
                    BanditBody.tag = "Player";
                    cb.portraitIcon = BanditContent.assetBundle.LoadAsset<Texture>("texBanditIcon.png");
                    cb.subtitleNameToken = "BANDITRELOADED_BODY_SUBTITLE";
                    cb.baseNameToken = "BANDITRELOADED_BODY_NAME";
                    cb.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
                    cb.baseMaxHealth = 100f;
                    cb.baseRegen = 1f;
                    cb.baseMaxShield = 0f;
                    cb.baseMoveSpeed = 7f;
                    cb.baseAcceleration = 80f;
                    cb.baseJumpPower = 15f;
                    cb.baseDamage = 12f;
                    cb.baseAttackSpeed = 1f;
                    cb.baseCrit = 1f;
                    cb.baseArmor = 0f;
                    cb.baseJumpCount = 1;
                    cb.bodyColor = BanditColor;

                    cb.autoCalculateLevelStats = true;
                    cb.levelMaxHealth = cb.baseMaxHealth * 0.3f;
                    cb.levelRegen = cb.baseRegen * 0.2f;
                    cb.levelMaxShield = 0f;
                    cb.levelMoveSpeed = 0f;
                    cb.levelJumpPower = 0f;
                    cb.levelDamage = cb.baseDamage * 0.2f;//
                    cb.levelAttackSpeed = 0f;
                    cb.levelCrit = 0f;
                    cb.levelArmor = 0f;

                    cb.hideCrosshair = false;
                    cb._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/crosshair/banditcrosshair");
                }
            }
        }

        private void AssignSkills()
        {
            if (BanditBody)
            {
                SkillLocator sk = BanditBody.GetComponent<SkillLocator>();
                if (sk)
                {
                    AssignPrimary(sk);
                    AssignSecondary(sk);
                    AssignUtility(sk);
                    AssignSpecial(sk);

                    sk.passiveSkill.enabled = true;
                    sk.passiveSkill.skillNameToken = "BANDITRELOADED_PASSIVE_NAME";
                    sk.passiveSkill.skillDescriptionToken = "BANDITRELOADED_PASSIVE_DESCRIPTION";
                    sk.passiveSkill.icon = BanditContent.assetBundle.LoadAsset<Sprite>("quickdraw.png");
                }
            }
        }

        private void FixScriptableObjectName(SkillDef sk)
        {
            (sk as ScriptableObject).name = sk.skillName;
        }

        public void AssignPrimary(SkillLocator sk)
        {
            //BanditContent.entityStates.Add(typeof(AIReload)); sniper


            SkillFamily primarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            primarySkillFamily.defaultVariantIndex = 0u;
            primarySkillFamily.variants = new SkillFamily.Variant[1];
            sk.primary._skillFamily = primarySkillFamily;


            #region Blast
            primaryBlastDef = ReloadSkillDef.CreateInstance<ReloadSkillDef>();
            primaryBlastDef.activationState = new SerializableEntityStateType(typeof(Blast));
            primaryBlastDef.baseRechargeInterval = 0f;
            if (!Blast.noReload)
            {
                primaryBlastDef.baseMaxStock = Modules.Config.blastStock;
                primaryBlastDef.rechargeStock = 0;
            }
            else
            {
                primaryBlastDef.baseRechargeInterval = 0f;
                primaryBlastDef.baseMaxStock = 1;
                primaryBlastDef.rechargeStock = 1;
            }
            primaryBlastDef.skillDescriptionToken = "";
            primaryBlastDef.skillDescriptionToken += "Fire a slug " + (Blast.penetrateEnemies ? "that pierces " : "") + "for <style=cIsDamage>" + Blast.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            if (primaryBlastDef.baseRechargeInterval > 0f)
            {
                primaryBlastDef.skillDescriptionToken += " Can hold up to " + primaryBlastDef.baseMaxStock + " bullets.";
            }
            primaryBlastDef.skillDescriptionToken += Environment.NewLine;

            primaryBlastDef.skillName = "FireSlug";
            primaryBlastDef.skillNameToken = "BANDITRELOADED_PRIMARY_NAME";
            primaryBlastDef.activationStateMachineName = "Weapon";
            primaryBlastDef.beginSkillCooldownOnSkillEnd = false;
            primaryBlastDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            primaryBlastDef.isCombatSkill = true;
            primaryBlastDef.cancelSprintingOnActivation = true;
            primaryBlastDef.canceledFromSprinting = false;
            primaryBlastDef.mustKeyPress = false;
            primaryBlastDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill1.png");

            primaryBlastDef.requiredStock = 1;
            primaryBlastDef.stockToConsume = 1;

            primaryBlastDef.reloadInterruptPriority = InterruptPriority.Any;
            primaryBlastDef.reloadState = new SerializableEntityStateType(typeof(EntityStates.Bandit2.Weapon.EnterReload));
            primaryBlastDef.graceDuration = 0.5f;
            FixScriptableObjectName(primaryBlastDef);
            BanditContent.entityStates.Add(typeof(Blast));
            BanditContent.skillDefs.Add(primaryBlastDef);
            #endregion

            #region scatter
            primaryScatterDef = ReloadSkillDef.CreateInstance<ReloadSkillDef>();
            primaryScatterDef.activationState = new SerializableEntityStateType(typeof(Scatter));
            primaryScatterDef.baseRechargeInterval = 0f;
            if (!Scatter.noReload)
            {
                primaryScatterDef.baseMaxStock = Modules.Config.scatterStock;
                primaryScatterDef.rechargeStock = 0;
            }
            else
            {
                primaryScatterDef.baseRechargeInterval = 0f;
                primaryScatterDef.baseMaxStock = 1;
                primaryScatterDef.rechargeStock = 1;
            }

            primaryScatterDef.skillName = "FireScatter";
            primaryScatterDef.skillNameToken = "BANDITRELOADED_PRIMARY_ALT_NAME";
            primaryScatterDef.skillDescriptionToken = "";
            primaryScatterDef.skillDescriptionToken += "Fire a volley of " + (Scatter.penetrateEnemies ? "piercing flechettes " : "buckshot ") + "for <style=cIsDamage>" + Scatter.pelletCount + "x" + Scatter.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            if (primaryScatterDef.baseRechargeInterval > 0f)
            {
                primaryScatterDef.skillDescriptionToken += " Can hold up to " + primaryScatterDef.baseMaxStock + " shells.";
            }
            primaryScatterDef.skillDescriptionToken += Environment.NewLine;
            primaryScatterDef.activationStateMachineName = "Weapon";
            primaryScatterDef.beginSkillCooldownOnSkillEnd = false;
            primaryScatterDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            primaryScatterDef.isCombatSkill = true;
            primaryScatterDef.cancelSprintingOnActivation = true;
            primaryScatterDef.canceledFromSprinting = false;
            primaryScatterDef.mustKeyPress = false;
            primaryScatterDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill1a.png");
            primaryScatterDef.requiredStock = 1;
            primaryScatterDef.stockToConsume = 1;

            primaryScatterDef.reloadInterruptPriority = InterruptPriority.Any;
            primaryScatterDef.reloadState = new SerializableEntityStateType(typeof(EntityStates.Bandit2.Weapon.EnterReload));
            primaryScatterDef.graceDuration = 0.5f;
            BanditContent.entityStates.Add(typeof(Scatter));
            FixScriptableObjectName(primaryScatterDef);

            BanditContent.skillDefs.Add(primaryScatterDef);
            #endregion

            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primaryBlastDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryBlastDef.skillNameToken, false)
            };
            Array.Resize(ref primarySkillFamily.variants, primarySkillFamily.variants.Length + 1);
            primarySkillFamily.variants[primarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = primaryScatterDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primaryScatterDef.skillNameToken, false)
            };
            BanditContent.skillFamilies.Add(primarySkillFamily);

        }

        public void AssignSecondary(SkillLocator sk)
        {
            SkillFamily secondarySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            secondarySkillFamily.defaultVariantIndex = 0u;
            secondarySkillFamily.variants = new SkillFamily.Variant[1];
            sk.secondary._skillFamily = secondarySkillFamily;


            #region Thermite Bomb
            thermiteDef = SkillDef.CreateInstance<SkillDef>();
            thermiteDef.activationState = new SerializableEntityStateType(typeof(ThermiteBomb));
            thermiteDef.baseRechargeInterval = Modules.Config.thermiteCooldown;
            thermiteDef.skillNameToken = "BANDITRELOADED_SECONDARY_ALT_NAME";
            thermiteDef.skillDescriptionToken = "Fire a flare that coats enemies in <color=#cd7bd7>Thermite</color>, dealing <style=cIsDamage>" + ThermiteBomb.burnDamageMult.ToString("P0").Replace(" ", "").Replace(",", "") + " damage per second</style>."
                + " Explodes for <style=cIsDamage>" + ThermiteBomb.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>. New flares <style=cIsUtility>reset the burn timer</style>.";
            thermiteDef.skillDescriptionToken += Environment.NewLine;
            thermiteDef.skillName = "Thermite";
            thermiteDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill2.png");
            thermiteDef.baseMaxStock = Modules.Config.thermiteStock;
            thermiteDef.rechargeStock = 1;
            thermiteDef.beginSkillCooldownOnSkillEnd = false;
            thermiteDef.activationStateMachineName = "Weapon";
            thermiteDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            thermiteDef.isCombatSkill = true;
            thermiteDef.cancelSprintingOnActivation = false;
            thermiteDef.canceledFromSprinting = false;
            thermiteDef.mustKeyPress = false;
            thermiteDef.requiredStock = 1;
            thermiteDef.stockToConsume = 1;
            thermiteDef.keywordTokens = new string[] { "KEYWORD_BANDITRELOADED_THERMITE" };
            FixScriptableObjectName(thermiteDef);
            BanditContent.entityStates.Add(typeof(ThermiteBomb));
            BanditContent.skillDefs.Add(thermiteDef);
            #endregion

            #region Acid Bomb

            acidBombDef = SkillDef.CreateInstance<SkillDef>();
            acidBombDef.activationState = new SerializableEntityStateType(typeof(AcidBomb));
            acidBombDef.baseRechargeInterval = Modules.Config.acidCooldown;
            acidBombDef.skillNameToken = "BANDITRELOADED_SECONDARY_ALT2_NAME";
            acidBombDef.skillDescriptionToken = "Toss a grenade that <style=cIsHealing>Weakens</style> for <style=cIsDamage>" + (AcidBomb.damageCoefficient).ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>."
                + " Leaves acid that deals <style=cIsDamage>" + AcidBomb.acidDamageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage per second</style>.";
            acidBombDef.skillDescriptionToken += Environment.NewLine;
            acidBombDef.skillName = "AcidGrenade";
            acidBombDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill2a.png");
            acidBombDef.baseMaxStock = Modules.Config.acidStock;
            acidBombDef.rechargeStock = 1;
            acidBombDef.beginSkillCooldownOnSkillEnd = false;
            acidBombDef.activationStateMachineName = "Weapon";
            acidBombDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            acidBombDef.isCombatSkill = true;
            acidBombDef.cancelSprintingOnActivation = false;
            acidBombDef.canceledFromSprinting = false;
            acidBombDef.mustKeyPress = false;
            acidBombDef.requiredStock = 1;
            acidBombDef.stockToConsume = 1;
            acidBombDef.keywordTokens = new string[] { "KEYWORD_WEAK" };
            FixScriptableObjectName(acidBombDef);
            BanditContent.entityStates.Add(typeof(AcidBomb));
            BanditContent.skillDefs.Add(acidBombDef);
            #endregion

            #region Cluster Bomb
            clusterBombDef = SkillDef.CreateInstance<SkillDef>();
            clusterBombDef.activationState = new SerializableEntityStateType(typeof(ClusterBomb));
            clusterBombDef.baseRechargeInterval = Modules.Config.cbCooldown;
            clusterBombDef.skillNameToken = "BANDITRELOADED_SECONDARY_NAME";
            clusterBombDef.skillDescriptionToken = "Toss a bomb that <style=cIsDamage>ignites</style> for <style=cIsDamage>" + (ClusterBomb.damageCoefficient).ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>."
                + " Drops bomblets for <style=cIsDamage>" + Modules.Config.cbBombletCount + "x" + (ClusterBomb.bombletDamageCoefficient).ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>."
                + " Can be shot midair for <style=cIsDamage>bonus damage</style>.";
            clusterBombDef.skillDescriptionToken += Environment.NewLine;
            clusterBombDef.skillName = "Dynamite";
            clusterBombDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("dynamite_red.png");
            clusterBombDef.baseMaxStock = Modules.Config.cbStock;
            clusterBombDef.rechargeStock = 1;
            clusterBombDef.beginSkillCooldownOnSkillEnd = false;
            clusterBombDef.activationStateMachineName = "Weapon";
            clusterBombDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            clusterBombDef.isCombatSkill = true;
            clusterBombDef.cancelSprintingOnActivation = false;
            clusterBombDef.canceledFromSprinting = false;
            clusterBombDef.mustKeyPress = false;
            clusterBombDef.requiredStock = 1;
            clusterBombDef.stockToConsume = 1;
            clusterBombDef.keywordTokens = new string[] { };
            BanditContent.entityStates.Add(typeof(ClusterBomb));
            FixScriptableObjectName(clusterBombDef);
            BanditContent.skillDefs.Add(clusterBombDef);
            #endregion

            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = clusterBombDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(clusterBombDef.skillNameToken, false)
            };
            Array.Resize(ref secondarySkillFamily.variants, secondarySkillFamily.variants.Length + 1);
            secondarySkillFamily.variants[secondarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = thermiteDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(thermiteDef.skillNameToken, false)
            };
            Array.Resize(ref secondarySkillFamily.variants, secondarySkillFamily.variants.Length + 1);
            secondarySkillFamily.variants[secondarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = acidBombDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(acidBombDef.skillNameToken, false)
            };

            BanditContent.skillFamilies.Add(secondarySkillFamily);
        }

        public void AssignUtility(SkillLocator sk)
        {
            SkillFamily utilitySkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            utilitySkillFamily.defaultVariantIndex = 0u;
            utilitySkillFamily.variants = new SkillFamily.Variant[1];
            sk.utility._skillFamily = utilitySkillFamily;

            #region CastSmokescreen
            utilityDefA = SkillDef.CreateInstance<SkillDef>();
            utilityDefA.activationState = new SerializableEntityStateType(typeof(CastSmokescreenNoDelay));
            utilityDefA.baseRechargeInterval = Modules.Config.cloakCooldown;
            utilityDefA.skillName = "CloakBanditReloaded";
            utilityDefA.skillNameToken = "BANDITRELOADED_UTILITY_NAME";
            utilityDefA.skillDescriptionToken = "<style=cIsDamage>Stunning</style>. Deal <style=cIsDamage>" + CastSmokescreenNoDelay.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>, become"
                   + " <style=cIsUtility>invisible</style>, then deal <style=cIsDamage>" + CastSmokescreenNoDelay.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style> again.";
            utilityDefA.skillDescriptionToken += " Your next attack gains <style=cIsDamage>+50% TOTAL damage</style>.";
            utilityDefA.skillDescriptionToken += Environment.NewLine;
            utilityDefA.baseMaxStock = Modules.Config.cloakStock;
            utilityDefA.rechargeStock = 1;
            utilityDefA.beginSkillCooldownOnSkillEnd = false;
            utilityDefA.activationStateMachineName = "Weapon";
            utilityDefA.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            utilityDefA.isCombatSkill = false;
            utilityDefA.cancelSprintingOnActivation = false;
            utilityDefA.canceledFromSprinting = false;
            utilityDefA.mustKeyPress = false;
            utilityDefA.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill3.png");
            utilityDefA.requiredStock = 1;
            utilityDefA.stockToConsume = 1;
            utilityDefA.forceSprintDuringState = false;
            utilityDefA.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            FixScriptableObjectName(utilityDefA);
            BanditContent.entityStates.Add(typeof(CastSmokescreenNoDelay));
            BanditContent.skillDefs.Add(utilityDefA);

            #region Assassinate
            utilityAltDef = SkillDef.CreateInstance<SkillDef>();
            utilityAltDef.activationState = new SerializableEntityStateType(typeof(Assassinate));

            utilityAltDef.baseRechargeInterval = Modules.Config.asCooldown;
            utilityAltDef.skillName = "Assassinate";
            utilityAltDef.skillNameToken = "Assassinate";
            utilityAltDef.skillDescriptionToken = "Charge up your gun and fire a high caliber shot that pierces enemies for <style=cIsDamage>" + FireChargeShot.minDamageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + "-" + FireChargeShot.maxDamageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>";
            utilityAltDef.skillDescriptionToken += ".";
            if (FireChargeShot.selfForceMax != 0 && FireChargeShot.selfForceMin != 0)
            {
                utilityAltDef.skillDescriptionToken += " <style=cIsUtility>Pushes you backwards</style> if you are airborn.";
            }
            utilityAltDef.skillDescriptionToken += Environment.NewLine;
            utilityAltDef.baseMaxStock = Modules.Config.asStock;
            utilityAltDef.rechargeStock = 1;
            utilityAltDef.beginSkillCooldownOnSkillEnd = true;
            utilityAltDef.activationStateMachineName = "Weapon";
            utilityAltDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;//should be .Skill if utility
            utilityAltDef.isCombatSkill = true;
            utilityAltDef.cancelSprintingOnActivation = true;
            utilityAltDef.canceledFromSprinting = false;
            utilityAltDef.mustKeyPress = false;
            utilityAltDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill3a.png");
            utilityAltDef.requiredStock = 1;
            utilityAltDef.stockToConsume = 1;
            FixScriptableObjectName(utilityAltDef); 
            BanditContent.entityStates.Add(typeof(FireChargeShot));
            BanditContent.entityStates.Add(typeof(Assassinate));
            BanditContent.skillDefs.Add(utilityAltDef);
            #endregion

            BanditContent.skillFamilies.Add(utilitySkillFamily);

            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = utilityDefA,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(utilityDefA.skillNameToken, false)
            };
            if (Modules.Config.asEnabled)
            {
                Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
                utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
                {
                    skillDef = utilityAltDef,
                    unlockableName = "",
                    viewableNode = new ViewablesCatalog.Node(utilityAltDef.skillNameToken, false)
                };
            }
            #endregion
        }

        public void AssignSpecial(SkillLocator sk)
        {
            SkillFamily specialSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            specialSkillFamily.defaultVariantIndex = 0u;
            specialSkillFamily.variants = new SkillFamily.Variant[1];




            #region LightsOut
            specialLightsOutDef = SkillDef.CreateInstance<SkillDef>();
            specialLightsOutDef.activationState = new SerializableEntityStateType(typeof(PrepLightsOut));

            specialLightsOutDef.skillDescriptionToken = "";
            List<String> kwlLO = new List<String>();
            if (TakeDamage.specialExecuteThreshold > 0f)
            {
                kwlLO.Add("KEYWORD_BANDITRELOADED_EXECUTE");
                specialLightsOutDef.skillDescriptionToken += "<style=cIsDamage>Executing</style>. ";
            }
            if (TakeDamage.specialDebuffBonus > 0f)
            {
                kwlLO.Add("KEYWORD_BANDITRELOADED_DEBUFFBOOST");
                specialLightsOutDef.skillDescriptionToken += "<style=cIsDamage>Debuff Boosted</style>. ";
            }
            specialLightsOutDef.keywordTokens = kwlLO.ToArray();

            specialLightsOutDef.skillDescriptionToken += "Fire a persuader shot for <style=cIsDamage>" + FireLightsOut.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            specialLightsOutDef.skillDescriptionToken += " Kills <style=cIsUtility>reset all your cooldowns</style>.";
            specialLightsOutDef.skillDescriptionToken += Environment.NewLine;
            specialLightsOutDef.baseRechargeInterval = Modules.Config.loCooldown;
            specialLightsOutDef.skillNameToken = "BANDITRELOADED_SPECIAL_NAME";
            specialLightsOutDef.skillName = "LightsOut";
            specialLightsOutDef.baseMaxStock = Modules.Config.loStock;
            specialLightsOutDef.rechargeStock = 1;

            specialLightsOutDef.activationStateMachineName = "Weapon";
            specialLightsOutDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill4.png");
            specialLightsOutDef.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialLightsOutDef.beginSkillCooldownOnSkillEnd = true;
            specialLightsOutDef.isCombatSkill = true;
            specialLightsOutDef.canceledFromSprinting = false;
            specialLightsOutDef.cancelSprintingOnActivation = true;
            specialLightsOutDef.mustKeyPress = false;
            specialLightsOutDef.requiredStock = 1;
            specialLightsOutDef.stockToConsume = 1;
            FixScriptableObjectName(specialLightsOutDef);
            BanditContent.entityStates.Add(typeof(PrepLightsOut));
            BanditContent.entityStates.Add(typeof(FireLightsOut));
            BanditContent.skillDefs.Add(specialLightsOutDef);
            #endregion

            #region barrage
            specialBarrageDef = SkillDef.CreateInstance<SkillDef>();
            specialBarrageDef.activationState = new SerializableEntityStateType(typeof(PrepBarrage));
            specialBarrageDef.skillDescriptionToken = "";
            List<string> kwlBarrage = new List<string>();
            if (TakeDamage.specialExecuteThreshold > 0f)
            {
                kwlBarrage.Add("KEYWORD_BANDITRELOADED_EXECUTE");
                specialBarrageDef.skillDescriptionToken += "<style=cIsDamage>Executing</style>. ";
            }
            if (TakeDamage.specialDebuffBonus > 0f)
            {
                kwlBarrage.Add("KEYWORD_BANDITRELOADED_DEBUFFBOOST");
                specialBarrageDef.skillDescriptionToken += "<style=cIsDamage>Debuff Boosted</style>. ";
            }
            specialBarrageDef.keywordTokens = kwlBarrage.ToArray();

            float barrageBonusDamage = FireBarrage.damageCoefficient * TakeDamage.specialDebuffBonus;
            specialBarrageDef.skillDescriptionToken += "Rapidly fire shots for <style=cIsDamage>" + FireBarrage.maxBullets + "x" + FireBarrage.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            specialBarrageDef.skillDescriptionToken += " Repeated hits deal <style=cIsDamage>+" + barrageBonusDamage.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style> each.";
            specialBarrageDef.skillDescriptionToken += " Kills <style=cIsUtility>reset all your cooldowns</style>.";
            specialBarrageDef.skillDescriptionToken += Environment.NewLine;
            specialBarrageDef.baseRechargeInterval = Modules.Config.reuCooldown;
            specialBarrageDef.skillNameToken = "BANDITRELOADED_SPECIAL_ALT_NAME";
            specialBarrageDef.skillName = "BanditBarrage";
            specialBarrageDef.baseMaxStock = Modules.Config.reuStock;
            specialBarrageDef.rechargeStock = 1;
            specialBarrageDef.activationStateMachineName = "Weapon";
            specialBarrageDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("skill3a.png");
            specialBarrageDef.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialBarrageDef.beginSkillCooldownOnSkillEnd = true;
            specialBarrageDef.isCombatSkill = true;
            specialBarrageDef.canceledFromSprinting = false;
            specialBarrageDef.cancelSprintingOnActivation = true;
            specialBarrageDef.mustKeyPress = false;
            specialBarrageDef.requiredStock = 1;
            specialBarrageDef.stockToConsume = 1;
            BanditContent.entityStates.Add(typeof(PrepBarrage));
            BanditContent.entityStates.Add(typeof(FireBarrage));
            FixScriptableObjectName(specialBarrageDef);
            BanditContent.skillDefs.Add(specialBarrageDef);

            specialBarrageScepterDef = SkillDef.CreateInstance<SkillDef>();
            specialBarrageScepterDef.activationState = new SerializableEntityStateType(typeof(PrepBarrageScepter));

            specialBarrageScepterDef.skillDescriptionToken = "";
            if (TakeDamage.specialExecuteThreshold > 0f)
            {
                specialBarrageScepterDef.skillDescriptionToken += "<style=cIsDamage>Executing</style>. ";
            }
            if (TakeDamage.specialDebuffBonus > 0f)
            {
                specialBarrageScepterDef.skillDescriptionToken += "<style=cIsDamage>Debuff Boosted</style>. ";
            }
            specialBarrageDef.keywordTokens = kwlBarrage.ToArray();
            float barrageScepterBonusDamage = FireBarrageScepter.damageCoefficient * TakeDamage.specialDebuffBonus;
            specialBarrageScepterDef.skillDescriptionToken += "Rapidly fire shots for <style=cIsDamage>" + FireBarrageScepter.maxBullets + "x" + FireBarrageScepter.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            specialBarrageScepterDef.skillDescriptionToken += " Repeated hits deal <style=cIsDamage>+" + barrageScepterBonusDamage.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style> each.";
            specialBarrageScepterDef.skillDescriptionToken += " Kills <style=cIsUtility>reset all your cooldowns</style>.";
            specialBarrageScepterDef.skillDescriptionToken += Environment.NewLine;
            specialBarrageScepterDef.baseRechargeInterval = Modules.Config.reuCooldown;
            specialBarrageScepterDef.skillNameToken = "BANDITRELOADED_SPECIAL_ALT_SCEPTER_NAME";
            specialBarrageScepterDef.skillName = "BanditBarrageScepter";
            specialBarrageScepterDef.baseMaxStock = Modules.Config.reuStock;
            specialBarrageScepterDef.rechargeStock = 1;
            specialBarrageScepterDef.activationStateMachineName = "Weapon";
            specialBarrageScepterDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("reu_scepter.png");
            specialBarrageScepterDef.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialBarrageScepterDef.beginSkillCooldownOnSkillEnd = true;
            specialBarrageScepterDef.isCombatSkill = true;
            specialBarrageScepterDef.canceledFromSprinting = false;
            specialBarrageScepterDef.cancelSprintingOnActivation = true;
            specialBarrageScepterDef.mustKeyPress = false;
            specialBarrageScepterDef.requiredStock = 1;
            specialBarrageScepterDef.stockToConsume = 1;
            FixScriptableObjectName(specialBarrageScepterDef);
            BanditContent.entityStates.Add(typeof(FireBarrageScepter));
            BanditContent.entityStates.Add(typeof(PrepBarrageScepter));
            BanditContent.skillDefs.Add(specialBarrageScepterDef);

            specialLightsOutScepterDef = SkillDef.CreateInstance<SkillDef>();
            specialLightsOutScepterDef.activationState = new SerializableEntityStateType(typeof(PrepLightsOutScepter));

            specialLightsOutScepterDef.skillDescriptionToken = "";
            if (TakeDamage.specialExecuteThreshold > 0f)
            {
                specialLightsOutScepterDef.skillDescriptionToken += "<style=cIsDamage>Executing</style>. ";
            }
            if (TakeDamage.specialDebuffBonus > 0f)
            {
                specialLightsOutScepterDef.skillDescriptionToken += "<style=cIsDamage>Debuff Boosted</style>. ";
            }
            specialLightsOutScepterDef.keywordTokens = kwlBarrage.ToArray();

            specialLightsOutScepterDef.skillDescriptionToken += "Fire a persuader shot for <style=cIsDamage>" + FireLightsOutScepter.damageCoefficient.ToString("P0").Replace(" ", "").Replace(",", "") + " damage</style>.";
            specialLightsOutScepterDef.skillDescriptionToken += " Kills <style=cIsUtility>reset all your cooldowns</style>.";

            specialLightsOutScepterDef.skillDescriptionToken += Environment.NewLine;
            specialLightsOutScepterDef.baseRechargeInterval = Modules.Config.loCooldown;
            specialLightsOutScepterDef.skillNameToken = "BANDITRELOADED_SPECIAL_SCEPTER_NAME";
            specialLightsOutScepterDef.skillName = "LightsOutScepter";
            specialLightsOutScepterDef.baseMaxStock = Modules.Config.loStock;
            specialLightsOutScepterDef.rechargeStock = 1;
            specialLightsOutScepterDef.activationStateMachineName = "Weapon";
            specialLightsOutScepterDef.icon = BanditContent.assetBundle.LoadAsset<Sprite>("lo_scepter.png");
            specialLightsOutScepterDef.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialLightsOutScepterDef.beginSkillCooldownOnSkillEnd = true;
            specialLightsOutScepterDef.isCombatSkill = true;
            specialLightsOutScepterDef.canceledFromSprinting = false;
            specialLightsOutScepterDef.cancelSprintingOnActivation = true;
            specialLightsOutScepterDef.mustKeyPress = false;
            specialLightsOutScepterDef.requiredStock = 1;
            specialLightsOutScepterDef.stockToConsume = 1;
            FixScriptableObjectName(specialLightsOutScepterDef);
            BanditContent.entityStates.Add(typeof(FireLightsOutScepter));
            BanditContent.entityStates.Add(typeof(PrepLightsOutScepter));
            BanditContent.skillDefs.Add(specialLightsOutScepterDef);

            BanditContent.entityStates.Add(typeof(ExitRevolver));

            BanditContent.skillFamilies.Add(specialSkillFamily);
            #endregion

            specialSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = specialLightsOutDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(specialLightsOutDef.skillNameToken, false)
            };

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = specialBarrageDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(specialBarrageDef.skillNameToken, false)
            };
        }

        private void SetupAcidBomb()
        {
            AcidBombObject = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/banditgrenadeprojectile"), "BanditReloadedAcidBomb", true);
            AcidBombGhostObject = R2API.PrefabAPI.InstantiateClone(AcidBombObject.GetComponent<ProjectileController>().ghostPrefab, "BanditReloadedAcidBombGhost", false);
            BanditContent.projectilePrefabs.Add(AcidBombObject);
            AcidBombObject.GetComponent<ProjectileController>().ghostPrefab = AcidBombGhostObject;

            GameObject puddleObject = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/crocoleapacid"), "BanditReloadedAcidBombPuddle", true);
            BanditContent.projectilePrefabs.Add(puddleObject);
            ProjectileDamage puddleDamage = puddleObject.GetComponent<ProjectileDamage>();
            puddleDamage.damageType = DamageType.WeakOnHit;
            ProjectileDotZone pdz = puddleObject.GetComponent<ProjectileDotZone>();
            pdz.attackerFiltering = AttackerFiltering.Default;
            pdz.overlapProcCoefficient = Modules.Config.acidProcCoefficient;
            pdz.lifetime = 5f;
            pdz.damageCoefficient = AcidBomb.acidDamageCoefficient / AcidBomb.damageCoefficient;

            GameObject abImpact = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/impacteffects/engimineexplosion"), "BanditReloadedAcidEffect", false);
            EffectComponent ec = abImpact.GetComponent<EffectComponent>();
            //ec.applyScale = true;
            //ec.disregardZScale = false;
            ec.soundName = "Play_acrid_shift_land";
            BanditContent.effectDefs.Add(new EffectDef(abImpact));

            AcidBombObject.GetComponent<ProjectileSimple>().desiredForwardSpeed = 60f;
            ProjectileImpactExplosion abPIE = AcidBombObject.GetComponent<ProjectileImpactExplosion>();
            abPIE.blastRadius = Modules.Config.acidRadius;
            AcidBombObject.GetComponent<ProjectileDamage>().damageType = DamageType.WeakOnHit;
            abPIE.blastProcCoefficient = 1f;
            abPIE.falloffModel = BlastAttack.FalloffModel.None;
            abPIE.lifetime = 20f;
            abPIE.impactEffect = abImpact;
            abPIE.explosionSoundString = "";
            abPIE.timerAfterImpact = false;
            abPIE.lifetimeAfterImpact = 0f;
            abPIE.destroyOnWorld = true;
            abPIE.destroyOnEnemy = true;
            abPIE.fireChildren = true;
            abPIE.childrenCount = 1;
            abPIE.childrenDamageCoefficient = 1f;
            abPIE.childrenProjectilePrefab = puddleObject;
            Destroy(AcidBombObject.GetComponent<ProjectileStickOnImpact>());
            AcidBombObject.GetComponent<Rigidbody>().useGravity = true;

            AcidBomb.projectilePrefab = AcidBombObject;
        }

        private void SetupThermite()
        {
            ThermiteObject = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/thermite"), "BanditReloadedThermite", true);
            ThermiteGhostObject = R2API.PrefabAPI.InstantiateClone(ThermiteObject.GetComponent<ProjectileController>().ghostPrefab, "BanditReloadedThermiteGhost", false);
            BanditContent.projectilePrefabs.Add(ThermiteObject);
            ThermiteObject.GetComponent<ProjectileController>().ghostPrefab = ThermiteGhostObject;

            ProjectileImpactExplosion tPIE = ThermiteObject.GetComponent<ProjectileImpactExplosion>();
            tPIE.blastRadius = Modules.Config.thermiteRadius / 2f;
            tPIE.blastProcCoefficient = 1f;
            tPIE.blastDamageCoefficient = 1f;
            tPIE.falloffModel = BlastAttack.FalloffModel.None;
            tPIE.timerAfterImpact = false;
            tPIE.lifetime = 20f;
            tPIE.destroyOnEnemy = false;
            tPIE.destroyOnWorld = true;
            ThermiteObject.GetComponent<ProjectileDamage>().damageType = DamageType.Stun1s;

            ProjectileSimple ps = ThermiteObject.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 100f;
            ps.lifetime = 20f;

            ProjectileDamage pd = ThermiteObject.GetComponent<ProjectileDamage>();
            pd.damage = 1f;
            pd.damageColorIndex = DamageColorIndex.Default;

            SphereCollider thermiteSphere = ThermiteObject.GetComponent<SphereCollider>();
            if (thermiteSphere == null)
            {
                thermiteSphere = ThermiteObject.AddComponent<SphereCollider>();
            }
            thermiteSphere.radius = 0.6f; //old radius: 0.25f

            ProjectileController tPC = ThermiteObject.GetComponent<ProjectileController>();
            if (tPC == null)
            {
                tPC = ThermiteObject.AddComponent<ProjectileController>();
            }
            tPC.procCoefficient = 1f;

            Destroy(ThermiteObject.GetComponent<ProjectileIntervalOverlapAttack>());

            BootlegThermiteOverlapAttack bootlegPOA = ThermiteObject.AddComponent<BootlegThermiteOverlapAttack>();
            bootlegPOA.damageCoefficient = 0.5f * ThermiteBomb.burnDamageMult / ThermiteBomb.damageCoefficient;
            bootlegPOA.procCoefficient = Modules.Config.thermiteProcCoefficient;
            bootlegPOA.damageInterval = 0.5f;
            bootlegPOA.lifetimeAfterImpact = Modules.Config.thermiteBurnDuration;

            ThermiteBomb.projectilePrefab = ThermiteObject;

            GameObject thermiteBurnEffect = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/impacteffects/missileexplosionvfx"), "BanditReloadedThermiteBurnEffect", false);
            thermiteBurnEffect.GetComponent<EffectComponent>().soundName = "Play_BanditReloaded_burn";
            BanditContent.effectDefs.Add(new EffectDef(thermiteBurnEffect));
            BootlegThermiteOverlapAttack.burnEffectPrefab = thermiteBurnEffect;
            ThermiteBomb.projectilePrefab = ThermiteObject;
        }

        private void SetupClusterBomb()
        {
            ClusterBombObject = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/BanditClusterBombSeed"), "BanditReloadedClusterBomb", true);
            BanditContent.projectilePrefabs.Add(ClusterBombObject);

            ClusterBombGhostObject = R2API.PrefabAPI.InstantiateClone(BanditContent.assetBundle.LoadAsset<GameObject>("DynamiteBundle.prefab"), "BanditReloadedClusterBombGhost", true);
            ClusterBombGhostObject.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;
            ClusterBombGhostObject.AddComponent<ProjectileGhostController>();

            ClusterBombObject.AddComponent<DynamiteRotation>();

            ClusterBombObject.GetComponent<ProjectileController>().ghostPrefab = ClusterBombGhostObject;


            float trueBombletDamage = ClusterBomb.bombletDamageCoefficient / ClusterBomb.damageCoefficient;
            SphereCollider sc = ClusterBombObject.AddComponent<SphereCollider>();
            sc.radius = 0.6f;
            sc.contactOffset = 0.01f;

            TeamComponent tc = ClusterBombObject.AddComponent<TeamComponent>();
            tc.hideAllyCardDisplay = false;
            ClusterBombObject.AddComponent<SkillLocator>();

            CharacterBody cb = ClusterBombObject.AddComponent<CharacterBody>();
            cb.rootMotionInMainState = false;
            cb.bodyFlags = CharacterBody.BodyFlags.Masterless;
            cb.baseMaxHealth = 1f;
            cb.baseCrit = 0f;
            cb.baseAcceleration = 0f;
            cb.baseArmor = 0f;
            cb.baseAttackSpeed = 0f;
            cb.baseDamage = 0f;
            cb.baseJumpCount = 0;
            cb.baseJumpPower = 0f;
            cb.baseMoveSpeed = 0f;
            cb.baseMaxShield = 0f;
            cb.baseRegen = 0f;
            cb.autoCalculateLevelStats = true;
            cb.levelArmor = 0f;
            cb.levelAttackSpeed = 0f;
            cb.levelCrit = 0f;
            cb.levelDamage = 0f;
            cb.levelJumpPower = 0f;
            cb.levelMaxHealth = 0f;
            cb.levelMaxShield = 0f;
            cb.levelMoveSpeed = 0f;
            cb.levelRegen = 0f;
            cb.hullClassification = HullClassification.Human;

            HealthComponent hc = ClusterBombObject.AddComponent<HealthComponent>();
            hc.globalDeathEventChanceCoefficient = 0f;
            hc.body = cb;

            ClusterBombObject.AddComponent<AssignDynamiteTeamFilter>();

            ProjectileImpactExplosion pie = ClusterBombObject.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = Modules.Config.cbRadius;
            pie.falloffModel = BlastAttack.FalloffModel.None;
            pie.lifetime = 25f;
            pie.lifetimeAfterImpact = 1.5f;
            pie.destroyOnEnemy = true;
            pie.destroyOnWorld = false;
            pie.childrenCount = Modules.Config.cbBombletCount;
            pie.childrenDamageCoefficient = trueBombletDamage;
            pie.blastProcCoefficient = 1f;
            pie.impactEffect = SetupDynamiteExplosion();

            pie.explosionSoundString = null;
            pie.lifetimeExpiredSound = null;
            pie.projectileHealthComponent = hc;
            pie.transformSpace = ProjectileImpactExplosion.TransformSpace.World;

            Destroy(ClusterBombObject.GetComponent<ProjectileStickOnImpact>());

            ProjectileSimple ps = ClusterBombObject.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 60f;
            ps.lifetime = 25f;

            ClusterBombObject.GetComponent<Rigidbody>().useGravity = true;

            ProjectileDamage pd = ClusterBombObject.GetComponent<ProjectileDamage>();
            pd.damageType = DamageType.IgniteOnHit;


            AddDynamiteHurtbox(ClusterBombObject);

            ClusterBomb.projectilePrefab = ClusterBombObject;
        }

        private void AddDynamiteHurtbox(GameObject go)
        {
            GameObject hbObject = new GameObject();
            hbObject.transform.parent = go.transform;
            //GameObject hbObject = go;

            hbObject.layer = LayerIndex.entityPrecise.intVal;
            SphereCollider goCollider = hbObject.AddComponent<SphereCollider>();
            goCollider.radius = 0.9f;

            HurtBoxGroup goHurtBoxGroup = hbObject.AddComponent<HurtBoxGroup>();
            HurtBox goHurtBox = hbObject.AddComponent<HurtBox>();
            goHurtBox.isBullseye = false;
            goHurtBox.healthComponent = go.GetComponent<HealthComponent>();
            goHurtBox.damageModifier = HurtBox.DamageModifier.Normal;
            goHurtBox.hurtBoxGroup = goHurtBoxGroup;
            goHurtBox.indexInGroup = 0;

            HurtBox[] goHurtBoxArray = new HurtBox[]
            {
                goHurtBox
            };

            goHurtBoxGroup.bullseyeCount = 0;
            goHurtBoxGroup.hurtBoxes = goHurtBoxArray;
            goHurtBoxGroup.mainHurtBox = goHurtBox;

            DisableCollisionsBetweenColliders dc = go.AddComponent<DisableCollisionsBetweenColliders>();
            dc.collidersA = go.GetComponents<Collider>();
            dc.collidersB = hbObject.GetComponents<Collider>();
        }

        private GameObject SetupDynamiteExplosion()
        {
            GameObject dynamiteExplosion = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/omnieffect/omniexplosionvfx"), "BanditReloadedDynamiteExplosion", false);
            ShakeEmitter se = dynamiteExplosion.AddComponent<ShakeEmitter>();
            se.shakeOnStart = true;
            se.duration = 0.5f;
            se.scaleShakeRadiusWithLocalScale = false;
            se.radius = 75f;
            se.wave = new Wave()
            {
                amplitude = 1f,
                cycleOffset = 0f,
                frequency = 40f
            };

            EffectComponent ec = dynamiteExplosion.GetComponent<EffectComponent>();
            ec.soundName = "Play_BanditReloaded_dynamite";

            BanditContent.effectDefs.Add(new EffectDef(dynamiteExplosion));
            return dynamiteExplosion;
        }

        private void SetupClusterBomblet()
        {
            ClusterBombletObject = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/BanditClusterGrenadeProjectile"), "BanditReloadedClusterBomblet", true);
            BanditContent.projectilePrefabs.Add(ClusterBombletObject);

            ClusterBombletGhostObject = R2API.PrefabAPI.InstantiateClone(BanditContent.assetBundle.LoadAsset<GameObject>("DynamiteStick.prefab"), "BanditReloadedClusterBombletGhost", true);
            ClusterBombletGhostObject.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;
            ClusterBombletGhostObject.AddComponent<ProjectileGhostController>();

            ClusterBombObject.GetComponent<ProjectileImpactExplosion>().childrenProjectilePrefab = ClusterBombletObject;

            ClusterBombletObject.AddComponent<SphereCollider>();
            ClusterBombletObject.GetComponent<ProjectileController>().ghostPrefab = ClusterBombletGhostObject;

            ProjectileImpactExplosion pie = ClusterBombletObject.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = Modules.Config.cbBombletRadius;
            pie.falloffModel = BlastAttack.FalloffModel.None;
            pie.destroyOnEnemy = false;
            pie.destroyOnWorld = false;
            pie.lifetime = 1.5f;
            pie.timerAfterImpact = false;
            pie.blastProcCoefficient = Modules.Config.cbBombletProcCoefficient;
            pie.explosionSoundString = null;
            pie.impactEffect = SetupDynamiteBombletExplosion();

            Destroy(ClusterBombletObject.GetComponent<ProjectileStickOnImpact>());

            ProjectileSimple ps = ClusterBombletObject.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 12f;

            ProjectileDamage pd = ClusterBombletObject.GetComponent<ProjectileDamage>();
            pd.damageType = DamageType.IgniteOnHit;
        }

        private GameObject SetupDynamiteBombletExplosion()
        {
            GameObject dynamiteExplosion = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/impacteffects/explosionvfx"), "BanditReloadedDynamiteBombletExplosion", false);

            EffectComponent ec = dynamiteExplosion.GetComponent<EffectComponent>();
            ec.soundName = "Play_engi_M2_explo";

            BanditContent.effectDefs.Add(new EffectDef(dynamiteExplosion));
            return dynamiteExplosion;
        }

        private void CreateMaster()
        {
            BanditMonsterMaster = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "BanditReloadedMonsterMaster", true);
            BanditContent.masterPrefabs.Add(BanditMonsterMaster);

            CharacterMaster cm = BanditMonsterMaster.GetComponent<CharacterMaster>();
            cm.bodyPrefab = BanditBody;

            Component[] toDelete = BanditMonsterMaster.GetComponents<AISkillDriver>();
            foreach (AISkillDriver asd in toDelete)
            {
                Destroy(asd);
            }

            AISkillDriver ass = BanditMonsterMaster.AddComponent<AISkillDriver>();
            ass.skillSlot = SkillSlot.Utility;
            ass.requiredSkill = utilityAltDef;
            ass.requireSkillReady = true;
            ass.requireEquipmentReady = false;
            ass.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            ass.minDistance = 0f;
            ass.maxDistance = float.PositiveInfinity;
            ass.selectionRequiresTargetLoS = true;
            ass.activationRequiresTargetLoS = true;
            ass.activationRequiresAimConfirmation = true;
            ass.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            ass.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            ass.ignoreNodeGraph = false;
            ass.driverUpdateTimerOverride = 2f;
            ass.noRepeat = false;
            ass.shouldSprint = false;
            ass.shouldFireEquipment = false;
            ass.shouldTapButton = false;
            AISkillDriver reu = BanditMonsterMaster.AddComponent<AISkillDriver>();
            reu.skillSlot = SkillSlot.Special;
            reu.requiredSkill = specialBarrageDef;
            reu.requireSkillReady = true;
            reu.requireEquipmentReady = false;
            reu.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            reu.minDistance = 0f;
            reu.maxDistance = 30f;
            reu.selectionRequiresTargetLoS = true;
            reu.activationRequiresTargetLoS = true;
            reu.activationRequiresAimConfirmation = true;
            reu.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            reu.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            reu.ignoreNodeGraph = false;
            reu.driverUpdateTimerOverride = 0.6f;
            reu.noRepeat = false;
            reu.shouldSprint = false;
            reu.shouldFireEquipment = false;
            reu.shouldTapButton = false;

            AISkillDriver reposition = BanditMonsterMaster.AddComponent<AISkillDriver>();
            reposition.skillSlot = SkillSlot.Utility;
            reposition.requireSkillReady = true;
            reposition.requireEquipmentReady = false;
            reposition.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            reposition.minDistance = 0f;
            reposition.maxDistance = 30f;
            reposition.selectionRequiresTargetLoS = false;
            reposition.activationRequiresTargetLoS = false;
            reposition.activationRequiresAimConfirmation = false;
            reposition.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            reposition.aimType = AISkillDriver.AimType.None;
            reposition.ignoreNodeGraph = false;
            reposition.driverUpdateTimerOverride = 1.6f;
            reposition.noRepeat = true;
            reposition.shouldSprint = true;
            reposition.shouldFireEquipment = false;
            reposition.shouldTapButton = true;

            AISkillDriver cloakchase = BanditMonsterMaster.AddComponent<AISkillDriver>();
            cloakchase.skillSlot = SkillSlot.Utility;
            cloakchase.requireSkillReady = true;
            cloakchase.requireEquipmentReady = false;
            cloakchase.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            cloakchase.minDistance = 0f;
            cloakchase.maxDistance = float.PositiveInfinity;
            cloakchase.selectionRequiresTargetLoS = false;
            cloakchase.activationRequiresTargetLoS = false;
            cloakchase.activationRequiresAimConfirmation = false;
            cloakchase.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            cloakchase.aimType = AISkillDriver.AimType.None;
            cloakchase.ignoreNodeGraph = false;
            cloakchase.driverUpdateTimerOverride = 3f;
            cloakchase.noRepeat = true;
            cloakchase.shouldSprint = true;
            cloakchase.shouldFireEquipment = false;
            cloakchase.shouldTapButton = true;

            AISkillDriver lo = BanditMonsterMaster.AddComponent<AISkillDriver>();
            lo.skillSlot = SkillSlot.Special;
            lo.requiredSkill = specialLightsOutDef;
            lo.requireSkillReady = true;
            lo.requireEquipmentReady = false;
            lo.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            lo.minDistance = 0f;
            lo.maxDistance = 50f;
            lo.selectionRequiresTargetLoS = true;
            lo.activationRequiresTargetLoS = true;
            lo.activationRequiresAimConfirmation = true;
            lo.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            lo.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            lo.ignoreNodeGraph = false;
            lo.driverUpdateTimerOverride = 0.6f;
            lo.noRepeat = false;
            lo.shouldSprint = false;
            lo.shouldFireEquipment = false;
            lo.shouldTapButton = false;

            AISkillDriver grenade = BanditMonsterMaster.AddComponent<AISkillDriver>();
            grenade.skillSlot = SkillSlot.Secondary;
            grenade.requireSkillReady = true;
            grenade.requireEquipmentReady = false;
            grenade.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            grenade.minDistance = 0f;
            grenade.maxDistance = 50f;
            grenade.selectionRequiresTargetLoS = true;
            grenade.activationRequiresTargetLoS = false;
            grenade.activationRequiresAimConfirmation = false;
            grenade.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            grenade.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            grenade.ignoreNodeGraph = false;
            grenade.driverUpdateTimerOverride = 0.5f;
            grenade.noRepeat = false;
            grenade.shouldSprint = false;
            grenade.shouldFireEquipment = false;
            grenade.shouldTapButton = false;

            AISkillDriver scatterAggressive = BanditMonsterMaster.AddComponent<AISkillDriver>();
            scatterAggressive.skillSlot = SkillSlot.Primary;
            scatterAggressive.requiredSkill = primaryScatterDef;
            scatterAggressive.requireSkillReady = true;
            scatterAggressive.requireEquipmentReady = false;
            scatterAggressive.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scatterAggressive.minDistance = 0f;
            scatterAggressive.maxDistance = 20f;
            scatterAggressive.selectionRequiresTargetLoS = true;
            scatterAggressive.activationRequiresTargetLoS = false;
            scatterAggressive.activationRequiresAimConfirmation = false;
            scatterAggressive.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            scatterAggressive.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scatterAggressive.ignoreNodeGraph = false;
            scatterAggressive.driverUpdateTimerOverride = 0.7f;
            scatterAggressive.noRepeat = false;
            scatterAggressive.shouldSprint = false;
            scatterAggressive.shouldFireEquipment = false;
            scatterAggressive.shouldTapButton = false;
            scatterAggressive.minUserHealthFraction = 0.4f;

            AISkillDriver scatter = BanditMonsterMaster.AddComponent<AISkillDriver>();
            scatter.skillSlot = SkillSlot.Primary;
            scatter.requiredSkill = primaryScatterDef;
            scatter.requireSkillReady = true;
            scatter.requireEquipmentReady = false;
            scatter.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            scatter.minDistance = 0f;
            scatter.maxDistance = 20f;
            scatter.selectionRequiresTargetLoS = true;
            scatter.activationRequiresTargetLoS = false;
            scatter.activationRequiresAimConfirmation = false;
            scatter.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            scatter.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            scatter.ignoreNodeGraph = false;
            scatter.driverUpdateTimerOverride = 0.7f;
            scatter.noRepeat = false;
            scatter.shouldSprint = false;
            scatter.shouldFireEquipment = false;
            scatter.shouldTapButton = false;

            AISkillDriver blast = BanditMonsterMaster.AddComponent<AISkillDriver>();
            blast.skillSlot = SkillSlot.Primary;
            blast.requireSkillReady = true;
            blast.requireEquipmentReady = false;
            blast.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            blast.minDistance = 8f;
            blast.maxDistance = 30f;
            blast.selectionRequiresTargetLoS = true;
            blast.activationRequiresTargetLoS = false;
            blast.activationRequiresAimConfirmation = false;
            blast.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            blast.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            blast.ignoreNodeGraph = false;
            blast.driverUpdateTimerOverride = 0.6f;
            blast.noRepeat = false;
            blast.shouldSprint = false;
            blast.shouldFireEquipment = false;
            blast.shouldTapButton = false;

            AISkillDriver chase = BanditMonsterMaster.AddComponent<AISkillDriver>();
            chase.skillSlot = SkillSlot.None;
            chase.requireSkillReady = false;
            chase.requireEquipmentReady = false;
            chase.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chase.minDistance = 0f;
            chase.maxDistance = float.PositiveInfinity;
            chase.selectionRequiresTargetLoS = false;
            chase.activationRequiresTargetLoS = false;
            chase.activationRequiresAimConfirmation = false;
            chase.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chase.aimType = AISkillDriver.AimType.AtMoveTarget;
            chase.ignoreNodeGraph = false;
            chase.driverUpdateTimerOverride = -1f;
            chase.noRepeat = false;
            chase.shouldSprint = true;
            chase.shouldFireEquipment = false;
            chase.shouldTapButton = false;

            AISkillDriver afk = BanditMonsterMaster.AddComponent<AISkillDriver>();
            afk.skillSlot = SkillSlot.None;
            afk.requireSkillReady = false;
            afk.requireEquipmentReady = false;
            afk.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            afk.minDistance = 0f;
            afk.maxDistance = float.PositiveInfinity;
            afk.selectionRequiresTargetLoS = false;
            afk.activationRequiresTargetLoS = false;
            afk.activationRequiresAimConfirmation = false;
            afk.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk.aimType = AISkillDriver.AimType.MoveDirection;
            afk.ignoreNodeGraph = false;
            afk.driverUpdateTimerOverride = -1f;
            afk.noRepeat = false;
            afk.shouldSprint = true;
            afk.shouldFireEquipment = false;
            afk.shouldTapButton = false;
        }

        //https://github.com/Moffein/RiskyMod/blob/533e5f847d950482e102387d35662fe6d477f1b3/RiskyMod/Enemies/Mobs/Lunar/LunarWisp.cs#L24
        private void EnableStatusConditions()
        {
            SetStateOnHurt ssoh = BanditBody.GetComponent<SetStateOnHurt>();
            if (!ssoh)
            {
                ssoh = BanditBody.AddComponent<SetStateOnHurt>();
            }
            ssoh.hitThreshold = 0.5f;
            ssoh.canBeHitStunned = false;
            ssoh.canBeStunned = false;
            ssoh.canBeFrozen = true;

            EntityStateMachine body = null;
            EntityStateMachine weapon = null;
            EntityStateMachine[] stateMachines = BanditBody.GetComponents<EntityStateMachine>();
            foreach (EntityStateMachine esm in stateMachines)
            {
                switch (esm.customName)
                {
                    case "Body":
                        body = esm;
                        break;
                    case "Weapon":
                        weapon = esm;
                        break;
                    default:
                        break;
                }
            }

            ssoh.targetStateMachine = body;
            ssoh.idleStateMachine = new EntityStateMachine[] { weapon };
            ssoh.hurtState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.HurtStateFlyer));
        }
    }
}
