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
public class GameInfo : BaseUnityPlugin
{
#if IL2CPP
    internal ManualLogSource Logger => Log;
#endif

    private const string DefaultInfo = "UNKNOWN";

    public string GameName { get; private set; } = DefaultInfo;
    public string GameDeveloper { get; private set; } = DefaultInfo;
    public string GameVersion { get; private set; } = DefaultInfo;

    private Traverse GetApplication() => TraverseHelper.SuppressHarmonyWarnings(() => Traverse.CreateWithType("UnityEngine.Application"));

#if IL2CPP
    public override void Load()
#else
    public void Awake()
#endif
    {
        try
        {
            ProcessGameInfo();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
        finally
        {
            Logger.LogMessage($"Game Name: {GameName}");
            Logger.LogMessage($"Game Developer: {GameDeveloper}");
            Logger.LogMessage($"Game Version: {GameVersion}");
        }
    }

    private void ProcessGameInfo()
    {
        var application = GetApplication();
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
