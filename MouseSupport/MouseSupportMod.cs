using Behaviours;
using ModBagman;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static SoG.Inventory;
using static SoG.Inventory.PreviewPair;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaMouseSupport
{
    public class MouseSupportMod : Mod
    {
        internal static WeakReference<Equipment.SpellSlot> LocalSpellSlot;
        internal static WeakReference<Equipment.ItemSlot> LocalItemSlotActivation;

        public override string Name => "Marioalexsan-GrindeaMouseSupport";
        public override Version Version => new Version(1, 0, 0);

        public static MouseSupportMod Instance { get; private set; }

        public bool InstantTargeting { get; private set; } = true;  // Mouse controls are much more usable this way
        public bool UseAllKeys { get; private set; } = false;
        public bool ButtonHitboxes { get; private set; } = false;
        public bool MoveWhileTargeting { get; private set; } = true;

        private readonly List<(Action, Action)> _initCleanupList;

        [ModCommand("InstantTargeting")]
        public void ToggleFastTargeting(string[] args, int connection)
        {
            InstantTargeting = !InstantTargeting;

            CAS.AddChatMessage("Instant Targeting is now " + (InstantTargeting ? "On" : "Off") + '.');
        }

        [ModCommand("UseAllKeys")]
        public void ToggleUseAllKeys(string[] args, int connection)
        {
            UseAllKeys = !UseAllKeys;

            CAS.AddChatMessage("The mod now uses " + (UseAllKeys ? "all slots" : "slots bound to mouse only") + '.');
        }

        [ModCommand("ButtonHitboxes")]
        public void ToggleButtonHitboxes(string[] args, int connection)
        {
            ButtonHitboxes = !ButtonHitboxes;

            CAS.AddChatMessage("Button hitboxes are now " + (ButtonHitboxes ? "on" : "off") + '.');
        }

        [ModCommand("MoveWhileTargeting")]
        public void ToggleMoveWhileTargeting(string[] args, int connection)
        {
            MoveWhileTargeting = !MoveWhileTargeting;

            CAS.AddChatMessage("The mod now " + (MoveWhileTargeting ? "allows" : "disallows") + " moving while targeting.");
        }

        public MouseSupportMod()
        {
            MouseSupportResources.ReloadResources();

            _initCleanupList = new List<(Action, Action)>
            {

            };
        }

        public override void Load()
        {
            Instance = this;

            foreach (var (init, _) in _initCleanupList)
                init?.Invoke();

            CreateCommands().AutoAddModCommands("Mouse");
        }

        public override void Unload()
        {
            foreach (var (_, cleanup) in _initCleanupList)
                cleanup?.Invoke();

            Instance = null;
        }
    }
}
