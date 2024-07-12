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

namespace Marioalexsan.GrindeaQoL
{
    public class QualityOfLifeMod : Mod
    {
        public override string Name => "Marioalexsan-GrindeaQoL";
        public override Version Version => new Version(1, 0, 0);

        public static QualityOfLifeMod Instance { get; private set; }

        private readonly List<(Action, Action)> _initCleanupList;

        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        public QualityOfLifeMod()
        {
            _initCleanupList = new List<(Action, Action)>
            {
                (BetterSpecialEffectNames.Init, null),
                (BetterLootChance.Init, null),
                (BerserkerStyleQoL.Init, BerserkerStyleQoL.CleanupMethod),
                (SummonPlantQoL.Init, SummonPlantQoL.CleanupMethod),
                (BetterBuffs.Init, BetterBuffs.CleanupMethod),
            };
        }

        public override void Load()
        {
            Instance = this;

            foreach (var (init, _) in _initCleanupList)
                init?.Invoke();
        }

        public override void Unload()
        {
            foreach (var (_, cleanup) in _initCleanupList)
                cleanup?.Invoke();

            Instance = null;
        }
    }
}
