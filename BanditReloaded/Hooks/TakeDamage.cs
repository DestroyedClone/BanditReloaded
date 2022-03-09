﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using BanditReloaded;
using BanditReloaded.Components;

namespace BanditReloaded.Hooks
{
    public class TakeDamage
    {
        public static float specialDebuffBonus;
        public static bool specialExecuteBosses;
        public static float specialExecuteThreshold;

        public static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool aliveBeforeHit = self.alive;
            bool resetCooldownsOnKill = false;
            bool isDynamiteBundle = false;
            bool isBarrage = false;
            float resetDuration = 0f;

            GracePeriodComponent graceComponent = self.gameObject.GetComponent<GracePeriodComponent>();
            if (!graceComponent)
            {
                graceComponent = self.gameObject.AddComponent<GracePeriodComponent>();
            }
            bool banditAttacker = false;
            AssignDynamiteTeamFilter ad = self.gameObject.GetComponent<AssignDynamiteTeamFilter>();
            if (ad)
            {
                isDynamiteBundle = true;
            }


            CharacterBody attackerCB = null;
            if (damageInfo.attacker)
            {
                attackerCB = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerCB)
                {
                    banditAttacker = attackerCB.baseNameToken == "BANDITRELOADED_BODY_NAME";
                }
            }

            if (banditAttacker && graceComponent)
            {
                if ((damageInfo.damageType & DamageType.ResetCooldownsOnKill) > 0)
                {
                    resetCooldownsOnKill = true;
                    if ((damageInfo.damageType & DamageType.SlowOnHit) > 0)
                    {
                        damageInfo.damageType &= ~DamageType.SlowOnHit;
                        isBarrage = true;
                    }

                    int debuffCount = 0;
                    DotController d = DotController.FindDotController(self.gameObject);

                    foreach (BuffIndex buffType in BuffCatalog.debuffBuffIndices)
                    {
                        if (self.body.HasBuff(buffType))
                        {
                            if (buffType != Modules.BanditContent.skullBuff.buffIndex)
                            {
                                debuffCount++;
                            }
                            else
                            {
                                debuffCount += self.body.GetBuffCount(buffType);
                            }
                        }
                    }
                    DotController dotController = DotController.FindDotController(self.gameObject);
                    if (dotController)
                    {
                        for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                        {
                            if (dotController.HasDotActive(dotIndex))
                            {
                                debuffCount++;
                            }
                        }
                    }

                    if (isBarrage && self.body.HasBuff(Modules.BanditContent.lightsOutBuff))
                    {
                        debuffCount--;
                    }

                    float buffDamage = 0f;
                    float buffBaseDamage = damageInfo.damage * specialDebuffBonus;
                    buffDamage = buffBaseDamage * debuffCount;
                    damageInfo.damage += buffDamage;

                    bool lightWeight = false;
                    if (self.body)
                    {
                        Rigidbody rb = self.body.rigidbody;
                        if (rb)
                        {
                            if (rb.mass < 50f)
                            {
                                lightWeight = true;
                            }
                        }
                    }

                    resetDuration = 3.6f;
                    if (!lightWeight)
                    {
                        resetDuration = GracePeriodComponent.graceDuration;
                    }
                }

                if (self.alive && (damageInfo.damageType & DamageType.ResetCooldownsOnKill) == 0 && graceComponent.HasReset(attackerCB))
                {
                    damageInfo.damageType |= DamageType.ResetCooldownsOnKill;
                }
            }

            if (isDynamiteBundle)
            {
                if (!ad.fired && banditAttacker && (damageInfo.damageType & DamageType.AOE) == 0 && damageInfo.procCoefficient > 0f)
                {
                    ad.fired = true;
                    damageInfo.crit = true;
                    damageInfo.procCoefficient = 0f;
                    ProjectileImpactExplosion pie = self.gameObject.GetComponent<ProjectileImpactExplosion>();
                    if (pie)
                    {
                        pie.blastRadius *= 2f;
                    }

                    ProjectileDamage pd = self.gameObject.GetComponent<ProjectileDamage>();
                    if (pd)
                    {
                        if (resetCooldownsOnKill)
                        {
                            pd.damage *= 2f;

                            BanditNetworkCommands bnc = damageInfo.attacker.GetComponent<BanditNetworkCommands>();
                            if (bnc)
                            {
                                bnc.RpcResetSpecialCooldown();
                            }
                        }
                        else
                        {
                            pd.damage *= 1.5f;
                        }
                    }
                }
                else
                {
                    damageInfo.rejected = true;
                }
            }

            orig(self, damageInfo);

            if (!self.alive && graceComponent)
            {
                if (aliveBeforeHit && self.globalDeathEventChanceCoefficient > 0f && self.body && self.body.master)
                {
                    graceComponent.TriggerEffects(attackerCB);
                }
            }
            else if (!damageInfo.rejected && self.alive)
            {
                if (banditAttacker)
                {
                    if (resetCooldownsOnKill)
                    {
                        if (isBarrage)
                        {
                            self.body.AddTimedBuff(Modules.BanditContent.skullBuff, 3f);
                        }
                        self.body.AddTimedBuff(Modules.BanditContent.lightsOutBuff, resetDuration);

                        if (graceComponent && resetDuration > 0f)
                        {
                            graceComponent.AddTimer(attackerCB, damageInfo.damageType, resetDuration);
                        }
                    }
                }

                if (self.body.HasBuff(Modules.BanditContent.lightsOutBuff) && specialExecuteThreshold > 0f)
                {
                    if (((self.body.bodyFlags & CharacterBody.BodyFlags.ImmuneToExecutes) == 0 && !self.body.isChampion) || specialExecuteBosses)
                    {
                        float executeThreshold = specialExecuteThreshold;
                        float executeToAdd = 0f;
                        if (self.body.isElite)
                        {
                            executeToAdd = damageInfo.inflictor.GetComponent<CharacterBody>().executeEliteHealthFraction;
                        }
                        if (self.isInFrozenState && executeToAdd < 0.3f)
                        {
                            executeToAdd = 0.3f;
                        }
                        executeThreshold += executeToAdd;

                        if (self.alive && (self.combinedHealthFraction < executeThreshold))
                        {
                            damageInfo.damage = self.combinedHealth / 2f + 1f;
                            damageInfo.damageType = (DamageType.ResetCooldownsOnKill | DamageType.BypassArmor);
                            damageInfo.procCoefficient = 0f;
                            damageInfo.crit = true;
                            damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                            orig(self, damageInfo);
                        }
                    }
                }

            }
        }
    }
}
