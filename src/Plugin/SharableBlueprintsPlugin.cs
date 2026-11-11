using System;
using System.IO;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.BuyMenu;
using Assets.Behaviour.Util;
using Assets.Source.Player;
using Behaviour.UI.Tooltip;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;
using HarmonyLib;
using KingdomFoundation.Api;

namespace SharableBlueprints
{
        [BepInPlugin(Id, "Sharable Blueprints", Version)]
        [BepInDependency("urakka.kingdominc.foundation", "0.11.0")]
    public sealed partial class SharableBlueprintsPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.sharableblueprints", Version = "0.5.2";
        Harmony patches;
        bool placementHooksReady;
        GameUI attachedUi;
        GameObject toolbarButton, activeBorder;
        AreaSelection selection;
        BlueprintLibraryPopup library;
        SelectionSummaryPanel summary;
        string captureBookId;
        Texture2D iconTexture;
        Sprite icon;
        float nextAttach;
        public string ExportDirectory => Path.Combine(Paths.ConfigPath, "SharableBlueprints", "Blueprints");
        internal Sprite BlueprintIcon => icon;

        void Awake()
        {
            patches = new Harmony(Id);
            try { patches.PatchAll(Assembly.GetExecutingAssembly()); placementHooksReady = true; }
            catch (Exception error) { Logger.LogError("Blueprint placement disabled: research guards could not be installed. " + error); }
            using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("medieval-schematic.png"))
            using (var bytes = new MemoryStream())
            {
                if (input == null) throw new InvalidOperationException("Missing schematic icon.");
                input.CopyTo(bytes);
                iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(iconTexture, bytes.ToArray())) throw new InvalidDataException("Invalid schematic icon.");
                iconTexture.filterMode = FilterMode.Point; iconTexture.wrapMode = TextureWrapMode.Clamp;
                icon = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(.5f, .5f), 100);
            }
            Logger.LogInfo("Sharable Blueprints ready. Area exports: " + ExportDirectory);
        }
    }
}
