using ModBagman;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using SoG;
using Esprima.Ast;
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

    public static string Escape(string input)
    {
        return input.Replace("\"", "\\\"");
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
            var stats = new Dictionary<EquipmentInfo.StatEnum, string>
            {
                [EquipmentInfo.StatEnum.ATK] = "atk",
            };

            var stopwatch = new Stopwatch();
            Logger.LogInformation("Extracting items...");

            var ids = Enum.GetNames(typeof(ItemCodex.ItemTypes));
            Mod game = GetMod("SoG");

            StringBuilder builder = new();

            builder.AppendLine($"--[[ BEGIN AUTOGEN ITEM TABLE --]]");
            builder.AppendLine($"--[[ Game version: {Globals.GrindeaVersion} --]]");
            builder.AppendLine("local itemStats = {");
            builder.AppendLine();

            foreach (var id in ids)
            {
                var entry = game.GetItem(id);

                if (entry != null)
                {
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
                        builder.Append($"\"{Escape(Enum.GetName(typeof(ItemCodex.ItemCategories), cat))}\",");
                    }

                    if (builder[builder.Length - 1] == ',')
                    {
                        builder.Length -= 1;
                    }

                    builder.AppendLine(" },");
                    builder.AppendLine($" name = \"{Escape(entry.Name ?? "_NO_NAME_")}\",");
                    builder.AppendLine($" desc = \"{Escape(entry.Description ?? "_NO_DESC_")}\",");

                    if (entry.Value > 0)
                    {
                        builder.AppendLine($"value = {entry.Value},");
                    }

                    if (entry.ArcadeValueModifier != 1f)
                    {
                        builder.AppendLine($" arcademod = {entry.ArcadeValueModifier},");
                    }

                    bool hasStats = false;

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
                        if (entry[EquipmentInfo.StatEnum.MaxHP] != 0)
                        {
                            builder.AppendLine($" maxhp = {entry[EquipmentInfo.StatEnum.MaxHP]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.MaxEP] != 0)
                        {
                            builder.AppendLine($" epreg = {entry[EquipmentInfo.StatEnum.MaxEP]},");
                            hasStats = true;
                        }
                        if (entry[EquipmentInfo.StatEnum.EPRegen] != 0)
                        {
                            builder.AppendLine($" def = {entry[EquipmentInfo.StatEnum.EPRegen]},");
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
                            builder.AppendLine($" special = {Escape(Enum.GetName(typeof(EquipmentInfo.SpecialEffect), equip.lenSpecialEffects[0]))},");
                            hasStats = true;
                        }

                        string type = "unknown";

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

                        builder.AppendLine($" type = \"{Escape(type)}\""); // This one has no comma, must be last
                    }

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
