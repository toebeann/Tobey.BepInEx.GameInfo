using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
#if IL2CPP
using BaseUnityPlugin = BepInEx.Unity.IL2CPP.BasePlugin;
#endif

namespace Tobey.BepInEx.GameInfo;

using System;
using Utility;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class GameInfo : BaseUnityPlugin
{
    private const string DefaultInfo = "UNKNOWN";

    public string GameName { get; private set; } = DefaultInfo;
    public string GameDeveloper { get; private set; } = DefaultInfo;
    public string GameVersion { get; private set; } = DefaultInfo;

#if IL2CPP
    public override void Load()
#else
    public void Awake()
#endif
    {
#if IL2CPP
        ManualLogSource logger = Log;
#else
        ManualLogSource logger = Logger;
#endif

        try
        {
            ProcessGameInfo();
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
        finally
        {
            logger.LogMessage($"Game Name: {GameName}");
            logger.LogMessage($"Game Developer: {GameDeveloper}");
            logger.LogMessage($"Game Version: {GameVersion}");
        }
    }

    private void ProcessGameInfo()
    {
        var application = TraverseHelper.SuppressHarmonyWarnings(() => Traverse.CreateWithType("UnityEngine.Application"));
        if (!application.TypeExists()) return;

        string getValue(string name) => TraverseHelper.SuppressHarmonyWarnings(() => application.Property(name)) switch
        {
            Traverse t when t.PropertyExists() => t.GetValue<string>(),
            _ => application.Field<string>(name).Value,
        };

        var gameVersion = getValue("version");
        if (!string.IsNullOrEmpty(gameVersion)) GameVersion = gameVersion;

        var gameName = getValue("productName");
        if (!string.IsNullOrEmpty(gameName)) GameName = gameName;

        var gameDeveloper = getValue("companyName");
        if (!string.IsNullOrEmpty(gameDeveloper)) GameDeveloper = gameDeveloper;

    }
}
