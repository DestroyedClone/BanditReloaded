using UnityEngine;

namespace BanditReloaded.Hooks
{
    public class RecalculateStats
    {
        public static void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(ModContentPack.thermiteBuff))
            {
                int tCount = sender.GetBuffCount(ModContentPack.thermiteBuff);
                args.moveSpeedMultAdd = Mathf.Pow(0.85f, tCount);
                //sender.moveSpeed *= Mathf.Pow(0.85f, tCount);
                args.armorAdd -= 2.5f * tCount;
            }
            if (sender.HasBuff(ModContentPack.skullBuff))
            {
                int skullCount = sender.GetBuffCount(ModContentPack.skullBuff);
                //sender.moveSpeed *= Mathf.Max(0.1f, 1f - 0.1f * skullCount);
                args.moveSpeedMultAdd += Mathf.Max(0.1f, 1f - 0.1f * skullCount);
            }
            if (sender.HasBuff(ModContentPack.cloakDamageBuff))
            {
                args.damageMultAdd += .5f;
            }
        }
    }
}
