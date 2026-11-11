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
    public sealed partial class SharableBlueprintsPlugin : BaseUnityPlugin
    {
        static T Field<T>(object source, string name) where T : class
        {
            return typeof(GameUI).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as T
                ?? throw new MissingFieldException("Game UI changed: " + name);
        }
    }
}
