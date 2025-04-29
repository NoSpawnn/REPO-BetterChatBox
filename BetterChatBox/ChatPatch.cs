using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BetterChatBox;

[HarmonyPatch(typeof(ChatManager))]
public class ChatPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(ChatManager.StateActive))]
    public static bool StateActivePrefix(ChatManager __instance)
    {
        var cursorPos = BetterChatBox.Instance.CursorPos;
        var chatMessage = __instance.chatMessage;

        SemiFunc.InputDisableMovement();

        InputHandlers.HandleCursorNavigation(ref __instance, ref cursorPos, ref chatMessage);
        InputHandlers.HandleClipboardOperation(ref __instance, ref cursorPos, ref chatMessage);
        InputHandlers.HandleHistoryNavigation(ref __instance, ref cursorPos, ref chatMessage);
        InputHandlers.HandleTextManip(ref __instance, ref cursorPos, ref chatMessage);

        var cursorChar = Mathf.Sin(Time.time * 10f) > 0f
                            ? BetterChatBox.CursorString
                            : BetterChatBox.EmptyCursorString;
        __instance.chatText.text = chatMessage.Insert(cursorPos, cursorChar);
        __instance.chatMessage = chatMessage;

        if (SemiFunc.InputDown(InputKey.Confirm))
            __instance.StateSet(chatMessage.IsNullOrWhiteSpace() ? ChatManager.ChatState.Inactive : ChatManager.ChatState.Send);
        if (SemiFunc.InputDown(InputKey.Back))
        {
            __instance.StateSet(ChatManager.ChatState.Inactive);
            ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
            ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
            MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
            __instance.StateSet(ChatManager.ChatState.Inactive);
        }

        BetterChatBox.Instance.CursorPos = cursorPos;

        return false;
    }

    // Needed so that the saved chat message (if any) 
    // isn't cleared when the chat is closed
    [HarmonyPrefix, HarmonyPatch(nameof(ChatManager.StateInactive))]
    public static bool StateInactivePrefix(ChatManager __instance)
    {
        ChatUI.instance.Hide();
        __instance.chatActive = false;

        MenuManager mmInstance = MenuManager.instance;
        MenuPage currentMenuPage = mmInstance.currentMenuPage;
        // TODO: figure out a name for this condition
        bool cond = currentMenuPage is null ||
                    (currentMenuPage.menuPageIndex != MenuPageIndex.Escape
                    && currentMenuPage.menuPageIndex != MenuPageIndex.Settings);
        if (cond && SemiFunc.InputDown(InputKey.Chat))
        {
            TutorialDirector.instance.playerChatted = true;
            mmInstance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, null, 1f, 1f, soundOnly: true);
            __instance.chatActive = !__instance.chatActive;
            __instance.StateSet(ChatManager.ChatState.Active);
        }

        return false;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ChatManager.ChatReset))]
    private static void ChatResetPostfix(ChatManager __instance)
    {
        BetterChatBox.Instance.CursorPos = 0;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ChatManager.StateSet))]
    public static void StateSetPostfix(ChatManager __instance, ChatManager.ChatState state)
    {
        if (
            __instance is null
            || __instance.chatText is null
            || state != ChatManager.ChatState.Active
        )
            return;

        BetterChatBox.Instance.ResetButtonsHeldState();

        if (BetterChatBox.SavePreviousChatMessage.Value)
        {
            __instance.chatMessage = __instance
                                        .chatText
                                        .text
                                        .Replace(BetterChatBox.CursorString, string.Empty)
                                        .Replace(BetterChatBox.EmptyCursorString, string.Empty); // This is a bit yucky
        }
        else
        {
            __instance.chatMessage = string.Empty;
            __instance.chatText.text = string.Empty;
        }

        if (!BetterChatBox.SaveCursorPosition.Value || __instance.chatMessage.Length == 0)
            BetterChatBox.Instance.CursorPos = __instance.chatMessage.Length;
    }

    internal static void ChatHistoryChangeEffect()
    {
        ChatUI.instance.SemiUITextFlashColor(Color.cyan, 0.2f);
        ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 0.2f, soundOnly: true);
    }

    internal static void ChatFailInputEffect()
    {
        ChatUI.instance.SemiUITextFlashColor(Color.red, 0.2f);
        ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
        ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
    }

}