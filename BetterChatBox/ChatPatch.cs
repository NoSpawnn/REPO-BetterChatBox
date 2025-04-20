using System.Linq;
using BepInEx;
using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace BetterChatBox;

[HarmonyPatch(typeof(ChatManager))]
public class ChatPatch
{
    private const string CURSOR_STRING = "<b>|</b>";
    private const string EMPTY_CURSOR_STRING = "<b> </b>"; // Not sure if the bold tags are actually needed but /shruge
    private const int MAX_CHARS = 50;

    [HarmonyPrefix, HarmonyPatch(nameof(ChatManager.StateActive))]
    public static bool ChatInputPrefix(ChatManager __instance)
    {
        var cursorPos = BetterChatBox.Instance.CursorPos;
        var chatMessage = __instance.chatMessage;

        /* Cursor navigation */
        if (Input.GetKeyUp(KeyCode.RightArrow)) BetterChatBox.Instance.RightArrowHeld = false;
        if (Input.GetKeyUp(KeyCode.LeftArrow)) BetterChatBox.Instance.LeftArrowHeld = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && cursorPos > 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var idx = chatMessage.LastIndexOf(' ', cursorPos - 1);
                cursorPos = idx == -1 ? 0 : idx;
            }
            else
            {
                BetterChatBox.Instance.LeftArrowHeld = true;
                BetterChatBox.Instance.CursorMoveTimer = BetterChatBox.InputHoldDelay.Value;
                cursorPos--;
            }
        }
        else if (BetterChatBox.Instance.LeftArrowHeld && cursorPos > 0)
        {
            BetterChatBox.Instance.CursorMoveTimer -= Time.deltaTime;
            if (BetterChatBox.Instance.CursorMoveTimer <= 0f)
            {
                BetterChatBox.Instance.CursorMoveTimer = BetterChatBox.CursorMoveSpeed.Value;
                cursorPos--;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && cursorPos < chatMessage.Length)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var idx = chatMessage.IndexOf(' ', cursorPos + 1);
                cursorPos = idx == -1 ? chatMessage.Length : idx;
            }
            else
            {
                BetterChatBox.Instance.RightArrowHeld = true;
                BetterChatBox.Instance.CursorMoveTimer = BetterChatBox.InputHoldDelay.Value;
                cursorPos++;
            }
        }
        else if (BetterChatBox.Instance.RightArrowHeld && cursorPos < chatMessage.Length)
        {
            BetterChatBox.Instance.CursorMoveTimer -= Time.deltaTime;
            if (BetterChatBox.Instance.CursorMoveTimer <= 0f)
            {
                BetterChatBox.Instance.CursorMoveTimer = BetterChatBox.CursorMoveSpeed.Value;
                cursorPos++;
            }
        }
        /* ---------- */

        /* Copy/Paste/Cut */
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                GUIUtility.systemCopyBuffer = chatMessage;
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                string clipboardText = GUIUtility.systemCopyBuffer.WithAllWhitespaceStripped();
                string newText = chatMessage.Insert(cursorPos, clipboardText);
                if (clipboardText.IsNullOrWhiteSpace() || newText.Length >= MAX_CHARS)
                {
                    ChatFailInputEffect();
                }
                else
                {
                    chatMessage = newText;
                    __instance.TypeEffect(Color.yellow);
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (BetterChatBox.CutToSystemClipboard.Value) GUIUtility.systemCopyBuffer = chatMessage;
                chatMessage = "";
                cursorPos = 0;
                __instance.CharRemoveEffect();
            }
        }
        /* ---------- */

        /* Close chat box */
        if (SemiFunc.InputDown(InputKey.Back))
        {
            __instance.StateSet(ChatManager.ChatState.Inactive);
            ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
            ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
            MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
        }
        /* ---------- */

        /* History navigation */
        if (Input.GetKeyDown(KeyCode.UpArrow) && __instance.chatHistory.Count > 0)
        {
            if (__instance.chatHistoryIndex > 0)
                __instance.chatHistoryIndex--;
            else
                __instance.chatHistoryIndex = __instance.chatHistory.Count - 1;

            chatMessage = __instance.chatHistory[__instance.chatHistoryIndex];
            ChatHistoryChangeEffect();
            cursorPos = chatMessage.Length;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && __instance.chatHistory.Count > 0)
        {
            if (__instance.chatHistoryIndex < __instance.chatHistory.Count - 1)
                __instance.chatHistoryIndex++;
            else
                __instance.chatHistoryIndex = 0;

            chatMessage = __instance.chatHistory[__instance.chatHistoryIndex];
            ChatHistoryChangeEffect();
            cursorPos = chatMessage.Length;
        }
        /* ---------- */

        SemiFunc.InputDisableMovement();

        /* Text manipulation */
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            BetterChatBox.Instance.BackspaceHeld = false;
        }
        else if (Input.GetKeyUp(KeyCode.Delete))
        {
            BetterChatBox.Instance.DeleteHeld = false;
        }
        else if (chatMessage.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) && cursorPos > 0)
            {
                BetterChatBox.Instance.BackspaceHeld = true;
                BetterChatBox.Instance.DeleteTimer = BetterChatBox.InputHoldDelay.Value;
                chatMessage = chatMessage.Remove(cursorPos - 1, 1);
                __instance.CharRemoveEffect();
                cursorPos--;
            }
            else if (BetterChatBox.Instance.BackspaceHeld && cursorPos > 0)
            {
                BetterChatBox.Instance.DeleteTimer -= Time.deltaTime;
                if (BetterChatBox.Instance.DeleteTimer <= 0f)
                {
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.DeleteRepeatSpeed.Value;
                    chatMessage = chatMessage.Remove(cursorPos - 1, 1);
                    __instance.CharRemoveEffect();
                    cursorPos--;
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete) && cursorPos < chatMessage.Length)
            {
                BetterChatBox.Instance.DeleteHeld = true;
                BetterChatBox.Instance.DeleteTimer = BetterChatBox.InputHoldDelay.Value;
                chatMessage = chatMessage.Remove(cursorPos, 1);
                __instance.CharRemoveEffect();
            }
            else if (BetterChatBox.Instance.DeleteHeld && cursorPos < chatMessage.Length)
            {
                BetterChatBox.Instance.DeleteTimer -= Time.deltaTime;
                if (BetterChatBox.Instance.DeleteTimer <= 0f)
                {
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.DeleteRepeatSpeed.Value;
                    chatMessage = chatMessage.Remove(cursorPos, 1);
                    __instance.CharRemoveEffect();
                }
            }

            if (SemiFunc.InputDown(InputKey.Confirm))
            {
                __instance.StateSet(chatMessage.IsNullOrWhiteSpace() ? ChatManager.ChatState.Inactive : ChatManager.ChatState.Send);
            }
        }

        foreach (var c in Input.inputString.Where(c => !char.IsControl(c)))
        {
            if (chatMessage.Length < MAX_CHARS)
            {
                __instance.prevChatMessage = chatMessage;
                chatMessage = chatMessage.Insert(cursorPos, c.ToString());
                cursorPos++;
                __instance.TypeEffect(Color.yellow);
            }
            else
            {
                ChatFailInputEffect();
            }
        }
        /* ---------- */

        __instance.chatText.text = chatMessage.Insert(cursorPos, Mathf.Sin(Time.time * 10f) > 0f ? CURSOR_STRING : EMPTY_CURSOR_STRING);
        __instance.chatMessage = chatMessage;

        if (SemiFunc.InputDown(InputKey.Back))
            __instance.StateSet(ChatManager.ChatState.Inactive);

        BetterChatBox.Instance.CursorPos = cursorPos;

        return false;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ChatManager.StateSet))]
    public static void StateSetPostfix(ChatManager __instance, ChatManager.ChatState state)
    {
        if (BetterChatBox.SavePreviousChatMessage.Value)
        {
            __instance.chatMessage = __instance
                                        .chatText
                                        .text
                                        .Replace(CURSOR_STRING, "")
                                        .Replace(EMPTY_CURSOR_STRING, ""); // This is a bit yucky
            if (!BetterChatBox.SaveCursorPosition.Value) BetterChatBox.Instance.CursorPos = 0;
        }
        else
        {
            __instance.chatMessage = "";
            __instance.chatText.text = "";
            BetterChatBox.Instance.CursorPos = 0;
        }

    }

    private static void ChatHistoryChangeEffect()
    {
        ChatUI.instance.SemiUITextFlashColor(Color.cyan, 0.2f);
        ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 0.2f, soundOnly: true);
    }

    private static void ChatFailInputEffect()
    {
        ChatUI.instance.SemiUITextFlashColor(Color.red, 0.2f);
        ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
        ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
        MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
    }
}