using ModBagman;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using SoG;
using System.Collections.Generic;
using System.IO;

namespace Marioalexsan.WikiDataExtractor;

public class WikiMod : Mod
{
    public override string Name => "Marioalexsan-WikiDataExtractor";
    public override Version Version => new(0, 1, 0);

    public override void Load()
    {
        CreateCommands().AutoAddModCommands("wiki");
    }

    public static string ProcessString(string input)
    {
        string result = input.Replace("\"", "\\\"").Replace("\n", "\\n");

        // Handle potions because they have to be special for some reason
		result = result.Replace("[APOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionWealth_GoldIncrease).ToString());
		result = result.Replace("[APOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionWealth_BaseDuration).ToString());
		result = result.Replace("[ARPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionArrow_ArrowsGained).ToString());
		result = result.Replace("[CPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionChicken_BaseDuration).ToString());
		result = result.Replace("[PPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionCrit_Increase).ToString());
		result = result.Replace("[PPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionCrit_BaseDuration).ToString());
		result = result.Replace("[LPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionLoot_ChanceIncrease).ToString());
		result = result.Replace("[LPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionLoot_BaseDuration).ToString());
		result = result.Replace("[SPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionSpeed_Increase).ToString());
		result = result.Replace("[SPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionSpeed_BaseDuration).ToString());
		result = result.Replace("[EPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionEnergy_EPGainedFromPotionInPCT).ToString());
		result = result.Replace("[DPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionDamage_DMGIncreaseInPCT).ToString());
		result = result.Replace("[DPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionDamage_BaseDuration).ToString());
		result = result.Replace("[HPPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionHealth_HealthGainedFromPotionInPCT).ToString());
		result = result.Replace("[LPOT]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionLightning_SparksToSpawn).ToString());
		result = result.Replace("[LPOTS]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionLightning_SparksToSpawn_OnOtherPotion).ToString());
		result = result.Replace("[LPOTD]", SpellVariable.Get(SpellVariable.Handle.Misc_PotionLightning_BaseDuration).ToString());

        return result;
    }

    [ModCommand("extract")]
    public void ExtractData(string[] args, int connection)
    {
        var stopwatch = new Stopwatch();
        Logger.LogInformation("Starting process!");
        stopwatch.Start();

        ExtractEnemyData();
        ExtractItemData();

        stopwatch.Stop();
        Logger.LogInformation($"Process finished in {stopwatch.Elapsed.TotalSeconds} seconds!");
    }

    public void ExtractEnemyData()
    {
        var stopwatch = new Stopwatch();
        Logger.LogInformation("Extracting enemies...");

        stopwatch.Stop();
        Logger.LogInformation($"Finished in {stopwatch.Elapsed.TotalSeconds} seconds!");
    }

    public void ExtractItemData()
    {
        try
        {
            var petStats = new Dictionary<PetInfo.PetBonus, string>
            {
                [PetInfo.PetBonus.Damage] = "Damage",
                [PetInfo.PetBonus.SP] = "EP",
                [PetInfo.PetBonus.HP] = "HP",
                [PetInfo.PetBonus.Crit] = "Crit",
                [PetInfo.PetBonus.Speed] = "Speed",
            };

            var stopwatch = new Stopwatch();
            Logger.LogInformation("Extracting items...");

            var ids = Enum.GetNames(typeof(ItemCodex.ItemTypes));
            Mod game = GetMod("SoG");

            StringBuilder builder = new();

            builder.AppendLine($"--[[ BEGIN AUTOGEN ITEM TABLE --]]");
            builder.AppendLine($"--[[ Game version: {Globals.GrindeaVersion} --]]");
            builder.AppendLine("local stats = {");
            builder.AppendLine();

            foreach (var id in ids)
            {
                var entry = game.GetItem(id);

                if (entry != null)
                {
                    if (entry.ModID.Contains("OBSOLETE")) {
                        continue; // Skip over garbage
                    }

                    var desc = ItemCodex.GetItemDescription(entry.GameID);

                    var equip = EquipmentCodex.GetArmorInfo(entry.GameID);
                    equip ??= EquipmentCodex.GetShieldInfo(entry.GameID);
                    equip ??= EquipmentCodex.GetShoesInfo(entry.GameID);
                    equip ??= EquipmentCodex.GetAccessoryInfo(entry.GameID);
                    equip ??= WeaponCodex.GetWeaponInfo(entry.GameID);
                    equip ??= FacegearCodex.GetHatInfo(entry.GameID);
                    equip ??= HatCodex.GetHatInfo(entry.GameID);

                    if (id.Contains(" "))
                    {
                        Logger.LogWarning($"Item [{id}] has spaces in ID!");
                    }

                    builder.AppendLine($"{id} = {{");
                    builder.Append(" cat = {");

                    foreach (var cat in desc.lenCategory)
                    {
                        builder.Append($"\"{ProcessString(Enum.GetName(typeof(ItemCodex.ItemCategories), cat))}\",");
                    }

                    if (builder[builder.Length - 1] == ',')
                    {
                        builder.Length -= 1;
                    }

                    builder.AppendLine(" },");
                    builder.AppendLine($" name = \"{ProcessString(entry.Name ?? "")}\",");

                    // This might crash for unimplemented items
                    try {
                        builder.AppendLine($" desc = \"{ProcessString(Globals.Game.MiscTextLibrary_GetText("Items", desc.sDescriptionLibraryHandle))}\",");
                    }
                    catch {
                        Logger.LogWarning("No description available for " + entry.Name);
                    }
                    

                    if (entry.Value > 0)
                    {
                        builder.AppendLine($" value = {entry.Value},");
                    }

                    if (entry.ArcadeValueModifier != 1f)
                    {
                        builder.AppendLine($" arcademod = {entry.ArcadeValueModifier},");
                    }

                    bool hasStats = false;
                    string type = "unknown";

                    if (entry.EquipType != EquipmentType.None)
                    {
                        if (entry[EquipmentInfo.StatEnum.ATK] != 0)
                        {
                            builder.AppendLine($" atk = {entry[EquipmentInfo.StatEnum.ATK]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.MATK] != 0)
                        {
                            builder.AppendLine($" matk = {entry[EquipmentInfo.StatEnum.MATK]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.ASPD] != 0)
                        {
                            builder.AppendLine($" aspd = {entry[EquipmentInfo.StatEnum.ASPD]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.CSPD] != 0)
                        {
                            builder.AppendLine($" cspd = {entry[EquipmentInfo.StatEnum.CSPD]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.HP] != 0)
                        {
                            builder.AppendLine($" maxhp = {entry[EquipmentInfo.StatEnum.HP]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.EP] != 0)
                        {
                            builder.AppendLine($" maxep = {entry[EquipmentInfo.StatEnum.EP]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.EPRegen] != 0)
                        {
                            builder.AppendLine($" epreg = {entry[EquipmentInfo.StatEnum.EPRegen]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.DEF] != 0)
                        {
                            builder.AppendLine($" def = {entry[EquipmentInfo.StatEnum.DEF]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.ShldHP] != 0)
                        {
                            builder.AppendLine($" shldhp = {entry[EquipmentInfo.StatEnum.ShldHP]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.Crit] != 0)
                        {
                            builder.AppendLine($" crit = {entry[EquipmentInfo.StatEnum.Crit]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.CritDMG] != 0)
                        {
                            builder.AppendLine($" critdmg = {entry[EquipmentInfo.StatEnum.CritDMG]},");
                            hasStats = true;
                        }
                        if (equip != null && equip.lenSpecialEffects.Count > 0)
                        {
                            builder.AppendLine($" special = \"{ProcessString(Enum.GetName(typeof(EquipmentInfo.SpecialEffect), equip.lenSpecialEffects[0]))}\",");
                            hasStats = true;
                        }

                        if (entry.EquipType == EquipmentType.Weapon)
                        {
                            bool oneHanded = entry.WeaponType == WeaponInfo.WeaponCategory.OneHanded;
                            bool twoHanded = entry.WeaponType == WeaponInfo.WeaponCategory.TwoHanded;
                            bool magic = entry.MagicWeapon;
                            if (oneHanded && magic)
                            {
                                type = "1h-m";
                            }
                            else if (oneHanded && !magic)
                            {
                                type = "1h";
                            }
                            else if (twoHanded && magic)
                            {
                                type = "2h-m";
                            }
                            else if (twoHanded && !magic)
                            {
                                type = "2h";
                            }
                        }
                        else if (entry.EquipType == EquipmentType.Hat)
                        {
                            if (entry.HatDoubleSlot)
                            {
                                type = hasStats ? "mask" : "mask-style";
                            }
                            else
                            {
                                type = hasStats ? "hat" : "hat-style";
                            }
                        }
                        else if (entry.EquipType == EquipmentType.Facegear)
                        {
                            type = hasStats ? "facegear" : "facegear-style";
                        }
                        else if (entry.EquipType == EquipmentType.Armor)
                        {
                            type = "armor";
                        }
                        else if (entry.EquipType == EquipmentType.Shoes)
                        {
                            type = "shoes";
                        }
                        else if (entry.EquipType == EquipmentType.Accessory)
                        {
                            type = "accessory";
                        }
                        else if (entry.EquipType == EquipmentType.Shield)
                        {
                            type = "shield";
                        }
                    }
                    else {
                        if (desc.lenCategory.Contains(ItemCodex.ItemCategories.Usable))
                        {
                            type = "usable";
                        }
                        else if (id.Contains("_PotionType"))
                        {
                            type = "usable";
                        }
                        else if (desc.lenCategory.Contains(ItemCodex.ItemCategories.KeyItem))
                        {
                            type = "keyitem";
                        }
                        else if (desc.lenCategory.Contains(ItemCodex.ItemCategories.Misc))
                        {
                            type = "misc";
                        }
                        else if (desc.lenCategory.Contains(ItemCodex.ItemCategories.Bow))
                        {
                            type = "bow";
                        }
                        else if (desc.lenCategory.Contains(ItemCodex.ItemCategories.Furniture))
                        {
                            type = "furniture";
                        }
                        else if (desc.lenCategory.Contains(ItemCodex.ItemCategories.TreasureMap))
                        {
                            type = "treasuremap";
                        }
                        else if (id.Contains("_Usable"))
                        {
                            type = "usable";
                        }
                        else if (id.Contains("_Special"))
                        {
                            type = "special";
                        }
                        else if (id.Contains("_KeyItem"))
                        {
                            type = "keyitem";
                        }
                        else if (id.Contains("_Furniture"))
                        {
                            type = "furniture";
                        }
                        else if (id.Contains("_Misc"))
                        {
                            type = "misc";
                        }
                    }
                    
                    if (PetCodex.denxFoodInfo.ContainsKey(entry.GameID)) {
                        builder.AppendLine($" foodtype = \"{petStats[PetCodex.denxFoodInfo[entry.GameID].enBonusType]}\",");
                        builder.AppendLine($" foodexp = {PetCodex.denxFoodInfo[entry.GameID].iBonusValue},");
                    }

                    builder.AppendLine($" type = \"{ProcessString(type)}\""); // This one has no comma, must be last
                    builder.AppendLine("},");
                }
            }

            builder.AppendLine();
            builder.AppendLine("}");
            builder.AppendLine($"--[[ END AUTOGEN ITEM TABLE --]]");

            File.WriteAllText("items.lua", builder.ToString());
            Logger.LogInformation($"Wrote {builder.Length} characters.");

            stopwatch.Stop();
            Logger.LogInformation($"Finished in {stopwatch.Elapsed.TotalSeconds} seconds!");
        }
        catch (Exception e) {
            Logger.LogError(e.ToString());
        }
    }

    public override void Unload()
    {
    }
}
