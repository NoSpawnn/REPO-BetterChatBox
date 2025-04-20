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

        InputHandlers.HandleCursorNavigation(ref __instance, ref cursorPos, chatMessage);
        InputHandlers.HandleClipboardOperation(ref __instance, ref cursorPos, ref chatMessage);
        InputHandlers.HandleHistoryNavigation(ref __instance, ref cursorPos, ref chatMessage);
        InputHandlers.HandleTextInput(ref __instance, ref cursorPos, ref chatMessage);

        SemiFunc.InputDisableMovement();

        __instance.chatText.text = chatMessage.Insert(cursorPos, Mathf.Sin(Time.time * 10f) > 0f ? BetterChatBox.CursorString : BetterChatBox.EmptyCursorString);
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

    [HarmonyPostfix, HarmonyPatch(nameof(ChatManager.StateSet))]
    public static void StateSetPostfix(ChatManager __instance)
    {
        if (BetterChatBox.SavePreviousChatMessage.Value)
        {
            __instance.chatMessage = __instance
                                        .chatText
                                        .text
                                        .Replace(BetterChatBox.CursorString, "")
                                        .Replace(BetterChatBox.EmptyCursorString, ""); // This is a bit yucky
        }
        else
        {
            __instance.chatMessage = "";
            __instance.chatText.text = "";
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