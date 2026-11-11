using System;
using System.Collections.Generic;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SharableBlueprints
{
    public sealed partial class BlueprintPlacement : MonoBehaviour
    {
        void Stop(string message) { WorldFlowManager.Instance.StopBlueprintMode(); owner.ShowMessage(message); }
    }
}
