using UnityEngine;

namespace BanditReloaded.Hooks
{
    public class RecalculateStats
    {
        public static void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Modules.BanditContent.thermiteBuff))
            {
                int tCount = sender.GetBuffCount(Modules.BanditContent.thermiteBuff);
                args.moveSpeedMultAdd = Mathf.Pow(0.85f, tCount);
                //sender.moveSpeed *= Mathf.Pow(0.85f, tCount);
                args.armorAdd -= 2.5f * tCount;
            }
            if (sender.HasBuff(Modules.BanditContent.skullBuff))
            {
                int skullCount = sender.GetBuffCount(Modules.BanditContent.skullBuff);
                //sender.moveSpeed *= Mathf.Max(0.1f, 1f - 0.1f * skullCount);
                args.moveSpeedMultAdd += Mathf.Max(0.1f, 1f - 0.1f * skullCount);
            }
            if (sender.HasBuff(Modules.BanditContent.cloakDamageBuff))
            {
                //damageStat *= 1.5f
                args.damageMultAdd += .5f;
            }
        }
    }
}
