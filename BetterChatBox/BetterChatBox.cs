using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BetterChatBox;

[BepInPlugin("NoSpawnn.BetterChatBox", "BetterChatBox", "1.0.1")]
public class BetterChatBox : BaseUnityPlugin
{
    internal static BetterChatBox Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    internal const string CursorString = "<b>|</b>";
    internal const string EmptyCursorString = "<b> </b>"; // Not sure if the bold tags are actually needed but /shruge
    internal const int MaxChars = 50;

    internal int CursorPos { get; set; }
    internal float DeleteTimer { get; set; }
    internal bool BackspaceHeld { get; set; }
    internal bool DeleteHeld { get; set; }
    internal bool LeftArrowHeld { get; set; }
    internal bool RightArrowHeld { get; set; }
    internal float CursorMoveTimer { get; set; }

    public static ConfigEntry<float> CursorMoveSpeed { get; private set; } = null!;
    public static ConfigEntry<float> DeleteRepeatSpeed { get; private set; } = null!;
    public static ConfigEntry<float> InputHoldDelay { get; private set; } = null!;
    public static ConfigEntry<bool> SavePreviousChatMessage { get; private set; } = null!;
    public static ConfigEntry<bool> SaveCursorPosition { get; private set; } = null!;
    public static ConfigEntry<bool> CutToSystemClipboard { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;

        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();
        BindConfigs();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    private void BindConfigs()
    {
        CursorMoveSpeed = Config.Bind("Input",
                "Cursor Tick Speed",
                defaultValue: 0.05f,
                new ConfigDescription("Cursor move speed when holding an arrow key", new AcceptableValueRange<float>(0.01f, 0.1f))
            );
        DeleteRepeatSpeed = Config.Bind("Input",
                "Delete Tick Speed",
                defaultValue: 0.05f,
                new ConfigDescription("Character delete speed when holding backspace or delete", new AcceptableValueRange<float>(0.01f, 0.1f))
            );
        InputHoldDelay = Config.Bind("Input",
                "Input Hold Delay",
                defaultValue: 0.5f,
                new ConfigDescription("How long you need to hold a key before it starts repeating", new AcceptableValueRange<float>(0.1f, 1.0f))
            );
        SavePreviousChatMessage = Config.Bind(
                "Behavior",
                "Save Last Chat Message",
                defaultValue: true,
                "Should the chat box save your unsent message on close"
            );
        SaveCursorPosition = Config.Bind(
                "Behavior",
                "Save Cursor Position",
                defaultValue: true,
                "Should the chat box save the cursor's position on close (this has no effect if the above is disabled)"
            );
        CutToSystemClipboard = Config.Bind(
                "Behavior",
                "Cut to system clipboard",
                defaultValue: false,
                "Should cutting (Ctrl + X) text be placed onto the system clipboard"
            );
    }
}