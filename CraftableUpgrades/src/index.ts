import { Globals } from 'modbagman';

const name = "ModBagman-CraftableUpgrades";
const version = "0.1.0";
const dependencies = {
    "ModBagman": "0.1.0"
};

const loadMod = (mod) => {
    const CommandsEntry = mod.CreateCommands();
    
    const IceCrystalPendant_V2 = mod.CreateItem("IceCrystalPendant_V2");
    const LightningGlove_V2 = mod.CreateItem("LightningGlove_V2");
    const WispShield_V2 = mod.CreateItem("WispShield_V2");
    const CrystalShield_V2 = mod.CreateItem("CrystalShield_V2");
    const WintersGuard_V2 = mod.CreateItem("WintersGuard_V2");
    const ThornWormShield_V2 = mod.CreateItem("ThornWormShield_V2");
    const CogShield_V2 = mod.CreateItem("CogShield_V2");
    const ArchersApple_V2 = mod.CreateItem("ArchersApple_V2");
    const Stinger_V2 = mod.CreateItem("Stinger_V2");
    const Stinger_V3 = mod.CreateItem("Stinger_V3");
    const FlowerWhip_V2 = mod.CreateItem("FlowerWhip_V2");
    const FlowerWhip_V3 = mod.CreateItem("FlowerWhip_V3");
    const Smashlight_V2 = mod.CreateItem("Smashlight_V2");
    const Smashlight_V3 = mod.CreateItem("Smashlight_V3");
    const GiantIcicle_V2 = mod.CreateItem("GiantIcicle_V2");
    const GiantIcicle_V3 = mod.CreateItem("GiantIcicle_V3");
    const BladeOfEchoes_V2 = mod.CreateItem("BladeOfEchoes_V2");
    const CactusClub_V2 = mod.CreateItem("CactusClub_V2");
}

export const mod = {
    name,
    version,
    dependencies,
    Load: loadMod
}

