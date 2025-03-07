﻿using BanditReloaded.Components;
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
            Blast.baseMaxDuration = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Fire Rate"), 0.3f, new ConfigDescription("Time between shots.")).Value;
            Blast.baseMinDuration = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Min Duration"), 0.2f, new ConfigDescription("How soon you can fire another shot if you mash.")).Value;
            Blast.penetrateEnemies = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Penetrate Enemies"), true, new ConfigDescription("Shots pierce enemies.")).Value;
            Blast.bulletRadius = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Shot Radius"), 0.4f, new ConfigDescription("How wide Blast's shots are.")).Value;
            Blast.force = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Force"), 600f, new ConfigDescription("Push force per shot.")).Value;
            Blast.spreadBloomValue = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Spread"), 0.5f, new ConfigDescription("Amount of spread with added when mashing.")).Value;
            Blast.recoilAmplitude = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Recoil"), 1.4f, new ConfigDescription("How hard the gun kicks when shooting.")).Value;
            Blast.maxDistance = config.Bind<float>(new ConfigDefinition("10 - Primary - Blast", "Range"), 300f, new ConfigDescription("How far Blast can reach.")).Value;
            Blast.useFalloff = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Use Falloff"), false, new ConfigDescription("Shots deal less damage over range.")).Value;
            blastStock = config.Bind<int>(new ConfigDefinition("10 - Primary - Blast", "Stock"), 8, new ConfigDescription("How many shots can be fired before reloading.")).Value;
            Blast.noReload = config.Bind<bool>(new ConfigDefinition("10 - Primary - Blast", "Disable Reload"), false, new ConfigDescription("Makes Blast never need to reload.")).Value;

            Scatter.damageCoefficient = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Damage"), 0.7f, new ConfigDescription("How much damage each pellet of Scatter deals.")).Value;
            Scatter.pelletCount = config.Bind<uint>(new ConfigDefinition("11 - Primary - Scatter", "Pellets"), 8, new ConfigDescription("How many pellets Scatter shoots.")).Value;
            Scatter.procCoefficient = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Proc Coefficient"), 0.75f, new ConfigDescription("Affects the chance and power of each pellet's procs.")).Value;
            Scatter.baseMaxDuration = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Fire Rate"), 0.625f, new ConfigDescription("Time between shots.")).Value;
            Scatter.baseMinDuration = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Min Duration"), 0.416f, new ConfigDescription("How soon you can fire another shot if you mash.")).Value;
            Scatter.penetrateEnemies = config.Bind<bool>(new ConfigDefinition("11 - Primary - Scatter", "Penetrate Enemies"), true, new ConfigDescription("Shots pierce enemies.")).Value;
            Scatter.bulletRadius = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Shot Radius"), 0.4f, new ConfigDescription("How wide Scatter's pellets are.")).Value;
            Scatter.force = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Force"), 200f, new ConfigDescription("Push force per pellet.")).Value;
            Scatter.spreadBloomValue = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Spread"), 2.5f, new ConfigDescription("Size of the pellet spread.")).Value;
            Scatter.recoilAmplitude = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Recoil"), 2.6f, new ConfigDescription("How hard the gun kicks when shooting.")).Value;
            Scatter.range = config.Bind<float>(new ConfigDefinition("11 - Primary - Scatter", "Range"), 200f, new ConfigDescription("How far Scatter can reach.")).Value;
            scatterStock = config.Bind<int>(new ConfigDefinition("11 - Primary - Scatter", "Stock"), 6, new ConfigDescription("How many shots Scatter can hold.")).Value;
            Scatter.noReload = config.Bind<bool>(new ConfigDefinition("11 - Primary - Scatter", "Disable Reload"), false, new ConfigDescription("Makes Scatter never need to reload.")).Value;

            ClusterBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Damage*"), 3.9f, new ConfigDescription("How much damage Dynamite Toss deals.")).Value;
            cbRadius = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Radius*"), 8f, new ConfigDescription("How large the explosion is. Radius is doubled when shot out of the air.")).Value;
            cbBombletCount = config.Bind<int>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Count*"), 6, new ConfigDescription("How many mini bombs Dynamite Toss releases.")).Value;
            ClusterBomb.bombletDamageCoefficient = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Damage*"), 1.2f, new ConfigDescription("How much damage Dynamite Toss Bomblets deals.")).Value;
            cbBombletRadius = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Radius*"), 8f, new ConfigDescription("How large the mini explosions are.")).Value;
            cbBombletProcCoefficient = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Bomblet Proc Coefficient*"), 0.6f, new ConfigDescription("Affects the chance and power of Dynamite Toss Bomblet procs.")).Value;
            ClusterBomb.baseDuration = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Throw Duration"), 0.4f, new ConfigDescription("How long it takes to throw a Dynamite Bundle.")).Value;
            cbCooldown = config.Bind<float>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Cooldown"), 6f, new ConfigDescription("How long it takes for Dynamite Toss to recharge.")).Value;
            cbStock = config.Bind<int>(new ConfigDefinition("20 - Secondary - Dynamite Toss", "Stock"), 1, new ConfigDescription("How much Dynamite you start with.")).Value;

            AcidBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Damage"), 2.7f, new ConfigDescription("How much damage Acid Bomb deals.")).Value;
            AcidBomb.acidDamageCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Acid Pool Damage"), 0.4f, new ConfigDescription("How much damage Acid Bomb's acid pool deals per second.")).Value;
            acidRadius = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Radius*"), 8f, new ConfigDescription("How large the explosion is.")).Value;
            acidProcCoefficient = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Acid Proc Coefficient*"), 0.2f, new ConfigDescription("Affects the chance and power of Acid Bomb's procs.")).Value;
            AcidBomb.baseDuration = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Throw Duration"), 0.4f, new ConfigDescription("How long it takes to throw a Acid Bomb.")).Value;
            acidCooldown = config.Bind<float>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Cooldown"), 6f, new ConfigDescription("How long Acid Bomb takes to recharge.")).Value;
            acidStock = config.Bind<int>(new ConfigDefinition("21 - Secondary - Acid Bomb", "Stock"), 1, new ConfigDescription("How many Acid Bombs you start with.")).Value;

            ThermiteBomb.damageCoefficient = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Damage"), 4.8f, new ConfigDescription("How much damage Thermite Flare deals.")).Value;
            ThermiteBomb.burnDamageMult = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Damage*"), 0.6f, new ConfigDescription("How much damage Thermite Flare deals per second.")).Value;
            thermiteBurnDuration = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Duration*"), 7f, new ConfigDescription("How long the burn lasts for.")).Value;
            thermiteProcCoefficient = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Burn Proc Coefficient*"), 0.4f, new ConfigDescription("Affects the chance and power of Thermite Flare's procs.")).Value;
            thermiteRadius = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Radius*"), 10f, new ConfigDescription("How large the explosion is. Radius is halved if it doesn't stick to a target.")).Value;
            ThermiteBomb.baseDuration = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Throw Duration"), 0.4f, new ConfigDescription("How long it takes to shoot a Thermite Flare.")).Value;
            thermiteCooldown = config.Bind<float>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Cooldown"), 6f, new ConfigDescription("How long Thermite Flare takes to recharge.")).Value;
            thermiteStock = config.Bind<int>(new ConfigDefinition("22 - Secondary - Thermite Flare", "Stock"), 1, new ConfigDescription("How many Thermite Flares you start with.")).Value;

            CastSmokescreenNoDelay.damageCoefficient = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Damage*"), 2f, new ConfigDescription("How much damage Smokebomb deals.")).Value;
            CastSmokescreenNoDelay.radius = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Radius*"), 12f, new ConfigDescription("Size of the stun radius.")).Value;
            CastSmokescreenNoDelay.duration = config.Bind<float>(new ConfigDefinition("30 - Utility - Smokebomb", "Duration*"), 3f, new ConfigDescription("How long Smokebomb lasts.")).Value;
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
            FireLightsOut.force = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "Force"), 2400f, new ConfigDescription("Push force per shot.")).Value;
            PrepLightsOut.baseDuration = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "Draw Time"), 0.6f, new ConfigDescription("How long it takes to prepare Lights Out.")).Value;
            FireLightsOut.baseDuration = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "End Lag"), 0.2f, new ConfigDescription("Delay after firing.")).Value;
            loCooldown = config.Bind<float>(new ConfigDefinition("41 - Special - Lights Out", "Cooldown"), 7f, new ConfigDescription("How long Lights Out takes to recharge.")).Value;
            loStock = config.Bind<int>(new ConfigDefinition("41 - Special - Lights Out", "Stock"), 1, new ConfigDescription("How many charges Lights Out has.")).Value;

            PrepLightsOutScepter.baseDuration = PrepLightsOut.baseDuration;
            FireLightsOutScepter.damageCoefficient = FireLightsOut.damageCoefficient * 2f;
            FireLightsOutScepter.force = FireLightsOut.force;
            FireLightsOutScepter.baseDuration = FireLightsOut.baseDuration;


            FireBarrage.damageCoefficient = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Damage"), 1f, new ConfigDescription("How much damage Rack em Up deals.")).Value;
            FireBarrage.maxBullets = config.Bind<int>(new ConfigDefinition("42 - Special - Rack em Up", "Total Shots"), 6, new ConfigDescription("How many shots are fired.")).Value;
            FireBarrage.force = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Force"), 100f, new ConfigDescription("Push force per shot.")).Value;
            PrepBarrage.baseDuration = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Draw Time"), 0.32f, new ConfigDescription("How long it takes to prepare Rack em Up.")).Value;
            FireBarrage.baseDuration = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Fire Rate"), 0.13f, new ConfigDescription("Time it takes for Rack em Up to fire a single shot.")).Value;
            FireBarrage.endLag = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "End Lag"), 0.4f, new ConfigDescription("Delay after firing all shots.")).Value;
            FireBarrage.spread = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Spread"), 2.5f, new ConfigDescription("Size of the cone of fire.")).Value;
            FireBarrage.maxDistance = config.Bind<float>(new ConfigDefinition("42 - Special - Rack em Up", "Range"), 200f, new ConfigDescription("How far shots reach.")).Value;
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
            FireChargeShot.minRadius = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Minimum Radius"), 0.4f, new ConfigDescription("How large Assassinate's shot radius is at no charge.")).Value;
            FireChargeShot.maxRadius = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Maximum Radius"), 2.4f, new ConfigDescription("How large Assassinate's shot radius is at max charge.")).Value;
            FireChargeShot.minForce = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Minimum Force"), 600f, new ConfigDescription("Push force at no charge.")).Value;
            FireChargeShot.maxForce = config.Bind<float>(new ConfigDefinition("99 -  Deprecated - Assassinate", "Maximum Force"), 2400f, new ConfigDescription("Push force at max charge.")).Value;
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
