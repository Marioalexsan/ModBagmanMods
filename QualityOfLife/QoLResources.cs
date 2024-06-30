using ModBagman;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Marioalexsan.GrindeaQoL
{
    public static class QoLResources
    {
        public static void ReloadResources()
        {
            NormalPlusTitle?.Dispose();
            NormalPlusTitle = null;

            QualityOfLifeMod.Instance.Logger.LogInformation($"Loading resources from {QualityOfLifeMod.Instance.LoadedFrom}");

            using var archive = ZipFile.OpenRead(QualityOfLifeMod.Instance.LoadedFrom);
            using var stream = archive.GetEntry("QualityOfLife/difficulty_normalplus.png").Open();
            
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            NormalPlusTitle = Texture2D.FromStream(Globals.Game.GraphicsDevice, memoryStream)
                ?? throw new InvalidOperationException("Failed to load a resource.");
        }

        public static Texture2D NormalPlusTitle { get; private set; }
    }
}