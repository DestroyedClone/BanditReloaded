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

namespace BanditReloaded.Modules
{
    public class Config
    {
        #region cfg
        public static bool useOldModel;

        public static int blastStock;

        public static float thermiteBurnDuration, thermiteRadius, thermiteProcCoefficient, thermiteCooldown;
        public static int thermiteStock;

        public static float cloakCooldown;
        public static int cloakStock;

        public static float loCooldown;
        public static int loStock;

        public static int scatterStock;

        public static float acidRadius, acidProcCoefficient, acidCooldown;
        public static int acidStock;

        public static float asCooldown;
        public static int asStock;
        public static bool asEnabled;

        public static float cbRadius, cbBombletRadius, cbBombletProcCoefficient, cbCooldown;
        public static int cbStock, cbBombletCount;

        public static float reuCooldown;
        public static int reuStock;

        #endregion


        public static void ReadConfig(ConfigFile config)
        {
            useOldModel = config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Use Old Model (EVERYONE NEEDS SAME SETTING)"), true, new ConfigDescription(" Uses the Bandit model from Risk of Rain 2 alpha.")).Value;
            asEnabled = config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Enable unused Assassinate utility*"), false, new ConfigDescription("Enables the Assassinate Utility skill. This skill was disabled due to being poorly coded and not fitting Bandit's kit, but it's left in in case you want to use it. This skill can only be used if Assassinate is enabled on the host.")).Value;

            string blastSound = config.Bind<string>(new ConfigDefinition("01 - General Settings", "Blast Sound"), "vanilla", new ConfigDescription("Which sound Blast plays when firing. Accepted values are 'new', 'classic', 'alpha', and 'vanilla'.")).Value;
            switch (blastSound.ToLower())
            {
                case "classic":
                    Blast.attackSoundString = "Play_BanditReloaded_blast_classic";
                    break;
                case "new":
                    Blast.attackSoundString = "Play_BanditReloaded_blast";
                    break;
                case "alpha":
                    Blast.attackSoundString = "Play_Bandit_m1_shot";
                    break;
                default:
                    Blast.attackSoundString = "Play_bandit2_m1_rifle";
                    break;
            }
            string loSound = config.Bind<string>(new ConfigDefinition("01 - General Settings", "Lights Out Sound"), "vanilla", new ConfigDescription("Which sound Lights Out plays when firing. Accepted values are 'alpha' and 'vanilla'.")).Value;
            switch (loSound.ToLower())
            {
                case "alpha":
                    FireLightsOut.attackSoundString = "Play_Bandit_m2_shot";
                    FireLightsOutScepter.attackSoundString = "Play_Bandit_m2_shot";
                    break;
                default:
                    FireLightsOut.attackSoundString = "Play_bandit2_R_fire";
                    FireLightsOutScepter.attackSoundString = "Play_bandit2_R_fire";
                    break;
            }

            string reuSound = config.Bind<string>(new ConfigDefinition("01 - General Settings", "Rack em Up Sound"), "vanilla", new ConfigDescription("Which sound Rack em Up plays when firing. Accepted values are 'alpha' and 'vanilla'.")).Value;
            switch (reuSound.ToLower())
            {
                case "alpha":
                    FireBarrage.attackSoundString = "Play_Bandit_m2_shot";
                    FireBarrageScepter.attackSoundString = "Play_Bandit_m2_shot";
                    break;
                default:
                    FireBarrage.attackSoundString = "Play_bandit2_R_fire";
                    FireBarrageScepter.attackSoundString = "Play_bandit2_R_fire";
                    break;
            }

            Blast.damageCoefficient = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Damage"), 2.5f, new ConfigDescription("How much damage Blast deals.")).Value;
            Blast.baseMaxDuration = 0.3f;
            Blast.baseMinDuration = 0.2f;
            Blast.penetrateEnemies = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Penetrate Enemies"), true, new ConfigDescription("Shots pierce enemies.")).Value;
            Blast.bulletRadius = 0.4f;
            Blast.force = 600f;
            Blast.spreadBloomValue = 0.5f;
            Blast.recoilAmplitude = 1.4f;
            Blast.maxDistance = 300f;
            Blast.useFalloff = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Use Falloff"), false, new ConfigDescription("Shots deal less damage over range.")).Value;
            blastStock = config.Bind<int>(new ConfigDefinition("10 - Primary - Blast", "Stock"), 8, new ConfigDescription("How many shots can be fired before reloading.")).Value;
            Blast.noReload = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Disable Reload"), false, new ConfigDescription("Makes Blast never need to reload.")).Value;

            Scatter.damageCoefficient = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Damage"), 0.7f, new ConfigDescription("How much damage each pellet of Scatter deals.")).Value;
            Scatter.pelletCount = 8;
            Scatter.procCoefficient = 0.75f;
            Scatter.baseMaxDuration = 0.625f;
            Scatter.baseMinDuration = 0.416f;
            Scatter.penetrateEnemies = config.Bind<bool>(new ConfigDefinition("11 - Primary - Scatter", "Penetrate Enemies"), true, new ConfigDescription("Shots pierce enemies.")).Value;
            Scatter.bulletRadius = 0.4f;
            Scatter.force = 200f;
            Scatter.spreadBloomValue = 2.5f;
            Scatter.recoilAmplitude = 2.6f;
            Scatter.range = 200f;
            scatterStock = config.Bind<int>(new ConfigDefinition("11 - Primary - Scatter", "Stock"), 6, new ConfigDescription("How many shots Scatter can hold.")).Value;
            Scatter.noReload = config.Bind<bool>(new ConfigDefinition("11 - Primary - Scatter", "Disable Reload"), false, new ConfigDescription("Makes Scatter never need to reload.")).Value;

            ClusterBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Damage*"), 3.9f, new ConfigDescription("How much damage Dynamite Toss deals.")).Value;
            cbRadius = 8f;
            cbBombletCount = config.Bind<int>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Count*"), 6, new ConfigDescription("How many mini bombs Dynamite Toss releases.")).Value;
            ClusterBomb.bombletDamageCoefficient = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Damage*"), 1.2f, new ConfigDescription("How much damage Dynamite Toss Bomblets deals.")).Value;
            cbBombletRadius = 8f;
            cbBombletProcCoefficient = 0.6f;
            ClusterBomb.baseDuration = 0.4f;
            cbCooldown = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Cooldown"), 6f, new ConfigDescription("How long it takes for Dynamite Toss to recharge.")).Value;
            cbStock = config.Bind<int>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Stock"), 1, new ConfigDescription("How much Dynamite you start with.")).Value;

            AcidBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Damage"), 2.7f, new ConfigDescription("How much damage Acid Bomb deals.")).Value;
            AcidBomb.acidDamageCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Acid Pool Damage"), 0.4f, new ConfigDescription("How much damage Acid Bomb's acid pool deals per second.")).Value;
            acidRadius = 8f;
            acidProcCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Acid Proc Coefficient*"), 0.2f, new ConfigDescription("Affects the chance and power of Acid Bomb's procs.")).Value;
            AcidBomb.baseDuration = 0.4f;
            acidCooldown = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Cooldown"), 6f, new ConfigDescription("How long Acid Bomb takes to recharge.")).Value;
            acidStock = config.Bind<int>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Stock"), 1, new ConfigDescription("How many Acid Bombs you start with.")).Value;

            ThermiteBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Damage"), 4.8f, new ConfigDescription("How much damage Thermite Flare deals.")).Value;
            ThermiteBomb.burnDamageMult = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Damage*"), 0.6f, new ConfigDescription("How much damage Thermite Flare deals per second.")).Value;
            thermiteBurnDuration = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Duration*"), 7f, new ConfigDescription("How long the burn lasts for.")).Value;
            thermiteProcCoefficient = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Proc Coefficient*"), 0.4f, new ConfigDescription("Affects the chance and power of Thermite Flare's procs.")).Value;
            thermiteRadius = 10f;
            ThermiteBomb.baseDuration = 0.4f;
            thermiteCooldown = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Cooldown"), 6f, new ConfigDescription("How long Thermite Flare takes to recharge.")).Value;
            thermiteStock = config.Bind<int>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Stock"), 1, new ConfigDescription("How many Thermite Flares you start with.")).Value;

            CastSmokescreenNoDelay.damageCoefficient = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Damage*"), 2f, new ConfigDescription("How much damage Smokebomb deals.")).Value;
            CastSmokescreenNoDelay.radius = 12f;
            CastSmokescreenNoDelay.duration = 3f;
            CastSmokescreenNoDelay.minimumStateDuration = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Minimum Duration"), 0.3f, new ConfigDescription("Minimum amount of time Smokebomb lasts for.")).Value;
            CastSmokescreenNoDelay.nonLethal = config.Bind<bool>(new ConfigDefinition("30 - Utility - Smokebomb", "Nonlethal"), true, new ConfigDescription("Prevents Smokebomb from landing the killing blow on enemies.")).Value;
            CastSmokescreenNoDelay.procCoefficient = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Proc Coefficient"), 0.5f, new ConfigDescription("Affects the chance and power of Smokebomb's procs.")).Value;
            cloakCooldown = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Cooldown"), 9f, new ConfigDescription("How long Smokebomb takes to recharge.")).Value;
            cloakStock = config.Bind<int>(new ConfigDefinition("30 - Utility - Smokebomb", "Stock"), 1, new ConfigDescription("How many charges Smokebomb has.")).Value;

            GracePeriodComponent.graceDuration = config.Bind<float>(new ConfigDefinition("40 - Special Settings", "Grace Period Duration*"), 0.5f, new ConfigDescription("How long the cooldown reset grace period lasts.")).Value;
            TakeDamage.specialDebuffBonus = config.Bind<float>(new ConfigDefinition("40 - Special Settings", "Special Debuff Bonus Multiplier*"), 0.5f, new ConfigDescription("Multiplier for how big the debuff damage bonus should be for Bandit's specials.")).Value;
            TakeDamage.specialExecuteThreshold = config.Bind<float>(new ConfigDefinition("40 - Special Settings", "Special Execute Threshold*"), 0.1f, new ConfigDescription("Bandit's Specials instakill enemies that fall below this HP percentage. 0 = 0%, 1 = 100%")).Value;
            TakeDamage.specialExecuteBosses = config.Bind<bool>(new ConfigDefinition("40 - Special Settings", "Special Execute Bosses*"), true, new ConfigDescription("Allow bosses to be executed by Bandit's Specials if Execute is enabled.")).Value;

            FireLightsOut.damageCoefficient = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "Damage"), 6f, new ConfigDescription("How much damage Lights Out deals.")).Value;
            FireLightsOut.force = 2400f;
            PrepLightsOut.baseDuration = 0.6f;
            FireLightsOut.baseDuration = 0.2f;
            loCooldown = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "Cooldown"), 7f, new ConfigDescription("How long Lights Out takes to recharge.")).Value;
            loStock = config.Bind<int>(new ConfigDefinition("41 - Special - Lights Out", "Stock"), 1, new ConfigDescription("How many charges Lights Out has.")).Value;

            PrepLightsOutScepter.baseDuration = PrepLightsOut.baseDuration;
            FireLightsOutScepter.damageCoefficient = FireLightsOut.damageCoefficient * 2f;
            FireLightsOutScepter.force = FireLightsOut.force;
            FireLightsOutScepter.baseDuration = FireLightsOut.baseDuration;


            FireBarrage.damageCoefficient = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Damage"), 1f, new ConfigDescription("How much damage Rack em Up deals.")).Value;
            FireBarrage.maxBullets = config.Bind<int>(new ConfigDefinition("42 - Special - Rack em Up", "Total Shots"), 6, new ConfigDescription("How many shots are fired.")).Value;
            FireBarrage.force = 100f;
            PrepBarrage.baseDuration = 0.32f;
            FireBarrage.baseDuration = 0.13f;
            FireBarrage.endLag = 0.4f;
            FireBarrage.spread = 2.5f;
            FireBarrage.maxDistance = 200f;
            reuCooldown = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Cooldown"), 7f, new ConfigDescription("How long Rack em Up takes to recharge.")).Value;
            reuStock = config.Bind<int>(new ConfigDefinition("42 - Special - Rack em Up", "Stock"), 1, new ConfigDescription("How many charges Rack em Up has.")).Value;

            PrepBarrageScepter.baseDuration = PrepBarrage.baseDuration;

            FireBarrageScepter.maxBullets = FireBarrage.maxBullets * 2;
            FireBarrageScepter.damageCoefficient = FireBarrage.damageCoefficient;
            FireBarrageScepter.force = FireBarrage.force;
            FireBarrageScepter.baseDuration = FireBarrage.baseDuration;
            FireBarrageScepter.spread = FireBarrage.spread;
            FireBarrageScepter.endLag = FireBarrage.endLag;
            FireBarrageScepter.maxDistance = FireBarrage.maxDistance;

            FireChargeShot.minDamageCoefficient = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Minimum Damage"), 2.5f, new ConfigDescription("How much damage Assassinate deals at no charge.")).Value;
            FireChargeShot.maxDamageCoefficient = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Maximum Damage"), 17f, new ConfigDescription("How much damage Assassinate deals at max charge.")).Value;
            FireChargeShot.minRadius = 0.4f;
            FireChargeShot.maxRadius = 2.4f;
            FireChargeShot.minForce = 600f;
            FireChargeShot.maxForce = 2400f;
            FireChargeShot.selfForceMin = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Minimum Self Force"), 4500f, new ConfigDescription("How far back you are launched when firing at no charge.")).Value;
            FireChargeShot.selfForceMax = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Maximum Self Force"), 4500f, new ConfigDescription("How far back you are launched when firing at max charge.")).Value;
            Assassinate.baseChargeDuration = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Charge Duration"), 1.5f, new ConfigDescription("How long it takes to fully charge Assassinate.")).Value;
            FireChargeShot.baseDuration = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "End Lag"), 0.5f, new ConfigDescription("Delay after firing.")).Value;
            Assassinate.zoomFOV = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Zoom FOV"), -1f, new ConfigDescription("Zoom-in FOV when charging Assassinate. -1 disables.")).Value;
            asCooldown = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Cooldown"), 5f, new ConfigDescription("How long it takes Assassinate to recharge")).Value;
            asStock = config.Bind<int>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Stock"), 1, new ConfigDescription("How many charges Assassinate has.")).Value;
        }
    }
}
