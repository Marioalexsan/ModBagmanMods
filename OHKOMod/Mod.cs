using HarmonyLib;
using ModBagman;
using SoG;
using System;

namespace OHKOMod
{
    [HarmonyPatch]
    public class OHKOMod : Mod
    {
        public override string Name => "OHKOMod";
        public override Version Version => new Version(1, 0, 0);

        public static OHKOMod Instance { get; private set; }

        public bool Enabled { get; private set; } = true;

        [ModCommand("Toggle")]
        public void ToggleFastTargeting(string[] args, int connection)
        {
            Enabled = !Enabled;

            CAS.AddChatMessage("One Hit KO mod is now " + (Enabled ? "On" : "Off") + '.');
        }

        public override void Load()
        {
            Instance = this;

            CreateCommands().AutoAddModCommands("OHKO");
        }

        public override void Unload()
        {
            Instance = null;
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Player_TakeDamage))]
        [HarmonyPrefix]
        private static void Instakill(ref int iInDamage, ref byte byType)
        {
            if (!Instance.Enabled)
                return;

            switch (byType)
            {
                case DamageTypes.Type1_ShieldDamage:
                case DamageTypes.Type2_ShieldDamage_PerfectGuard:
                case DamageTypes.Type101_BreakShield:
                case DamageTypes.Type11_BlockedByChicken:
                case DamageTypes.Type4_PerfectGuard:
                case DamageTypes.Type6_Stun:
                case DamageTypes.Type100_BlockedByUltimateGuard:
                case DamageTypes.Type102_BlockedByDodge:
                    // Don't kill player for these
                    return;

                default:
                    // Kill player instantly by modifying incoming damage
                    iInDamage = 999999;
                    byType = DamageTypes.Type103_HealthDamage;
                    return;
            }
        }
    }
}
