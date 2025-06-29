using System.Linq;
using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace BetterChatBox;

internal static class InputHandlers
{
    internal static void HandleTextManip(ref ChatManager chatManagerInstance, ref int cursorPos, ref string chatMessage)
    {
        if (Input.GetKeyUp(KeyCode.Backspace)) BetterChatBox.Instance.BackspaceHeld = false;
        if (Input.GetKeyUp(KeyCode.Delete)) BetterChatBox.Instance.DeleteHeld = false;

        if (chatMessage.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) && cursorPos > 0)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    int idx = GetPreviousWordIdxFromIdx(cursorPos, chatMessage, keepWhitespace: true);
                    chatMessage = chatMessage.Remove(idx, cursorPos - idx);
                    cursorPos = idx;
                }
                else
                {
                    BetterChatBox.Instance.BackspaceHeld = true;
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.InputHoldDelay.Value;
                    chatMessage = chatMessage.Remove(cursorPos - 1, 1);
                    cursorPos--;
                }
                chatManagerInstance.CharRemoveEffect();
            }
            else if (BetterChatBox.Instance.BackspaceHeld && cursorPos > 0)
            {
                BetterChatBox.Instance.DeleteTimer -= Time.deltaTime;
                if (BetterChatBox.Instance.DeleteTimer <= 0f)
                {
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.DeleteRepeatSpeed.Value;
                    chatMessage = chatMessage.Remove(cursorPos - 1, 1);
                    chatManagerInstance.CharRemoveEffect();
                    cursorPos--;
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete) && cursorPos < chatMessage.Length)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    int idx = GetNextWordIdxFromIdx(cursorPos, chatMessage, keepWhitespace: true);
                    chatMessage = chatMessage.Remove(cursorPos, idx - cursorPos);
                }
                else
                {
                    BetterChatBox.Instance.DeleteHeld = true;
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.InputHoldDelay.Value;
                    chatMessage = chatMessage.Remove(cursorPos, 1);
                }
                chatManagerInstance.CharRemoveEffect();
            }
            else if (BetterChatBox.Instance.DeleteHeld && cursorPos < chatMessage.Length)
            {
                BetterChatBox.Instance.DeleteTimer -= Time.deltaTime;
                if (BetterChatBox.Instance.DeleteTimer <= 0f)
                {
                    BetterChatBox.Instance.DeleteTimer = BetterChatBox.DeleteRepeatSpeed.Value;
                    chatMessage = chatMessage.Remove(cursorPos, 1);
                    chatManagerInstance.CharRemoveEffect();
                }
            }
        }

        foreach (var c in Input.inputString.Where(c => !char.IsControl(c)))
        {
            if (!BetterChatBox.ShouldLimitChatLength || chatMessage.Length < BetterChatBox.MaxChars)
            {
                chatManagerInstance.prevChatMessage = chatMessage;
                chatMessage = chatMessage.Insert(cursorPos, c.ToString());
                cursorPos++;
                chatManagerInstance.TypeEffect(Color.yellow);
            }
            else
            {
                ChatPatch.ChatFailInputEffect();
            }
        }
    }

    internal static void HandleHistoryNavigation(ref ChatManager chatManagerInstance, ref int cursorPos, ref string chatMessage)
    {
        if (chatManagerInstance.chatHistory.Count <= 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (chatManagerInstance.chatHistoryIndex > 0)
                chatManagerInstance.chatHistoryIndex--;
            else
                chatManagerInstance.chatHistoryIndex = chatManagerInstance.chatHistory.Count - 1;

            chatMessage = chatManagerInstance.chatHistory[chatManagerInstance.chatHistoryIndex];
            ChatPatch.ChatHistoryChangeEffect();
            cursorPos = chatMessage.Length;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (chatManagerInstance.chatHistoryIndex < chatManagerInstance.chatHistory.Count - 1)
                chatManagerInstance.chatHistoryIndex++;
            else
                chatManagerInstance.chatHistoryIndex = 0;

            chatMessage = chatManagerInstance.chatHistory[chatManagerInstance.chatHistoryIndex];
            ChatPatch.ChatHistoryChangeEffect();
            cursorPos = chatMessage.Length;
        }
    }

    internal static void HandleCursorNavigation(ref ChatManager chatManagerInstance, ref int cursorPos, ref string chatMessage)
    {
        if (Input.GetKeyUp(KeyCode.RightArrow)) BetterChatBox.Instance.RightArrowHeld = false;
        if (Input.GetKeyUp(KeyCode.LeftArrow)) BetterChatBox.Instance.LeftArrowHeld = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && cursorPos > 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                cursorPos = GetPreviousWordIdxFromIdx(cursorPos, chatMessage, keepWhitespace: false);
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
                cursorPos = GetNextWordIdxFromIdx(cursorPos, chatMessage, keepWhitespace: false);
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
    }

    internal static void HandleClipboardOperation(ref ChatManager chatManagerInstance, ref int cursorPos, ref string chatMessage)
    {
        if (!Input.GetKey(KeyCode.LeftControl)) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            GUIUtility.systemCopyBuffer = chatMessage;
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            string clipboardText = GUIUtility.systemCopyBuffer.WithAllWhitespaceStripped();
            string newText = chatMessage.Insert(cursorPos, clipboardText);
            if (clipboardText.IsNullOrWhiteSpace() || (BetterChatBox.ShouldLimitChatLength && newText.Length >= BetterChatBox.MaxChars))
            {
                ChatPatch.ChatFailInputEffect();
            }
            else
            {
                chatMessage = newText;
                cursorPos += clipboardText.Length;
                chatManagerInstance.TypeEffect(Color.yellow);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (BetterChatBox.CutToSystemClipboard.Value) GUIUtility.systemCopyBuffer = chatMessage;
            chatMessage = string.Empty;
            cursorPos = 0;
            chatManagerInstance.CharRemoveEffect();
        }
    }

    // TODO: I really need to rename the keepWhitespace flag,
    // I wrote these two functions at like 1am and now I
    // don't really understand how they both work correctly
    // (or if they even do, but I think they do...)
    private static int GetNextWordIdxFromIdx(int from, string str, bool keepWhitespace)
    {
        int idx = from;

        while (idx < str.Length
                && char.IsWhiteSpace(str[idx]))
            idx++;

        if (!char.IsWhiteSpace(str[from]))
            while (idx < str.Length
                    && !char.IsWhiteSpace(str[idx]))
                idx++;

        if (!keepWhitespace)
            while (idx < str.Length
                    && !char.IsWhiteSpace(str[idx]))
                idx++;

        return idx;
    }

    private static int GetPreviousWordIdxFromIdx(int from, string str, bool keepWhitespace)
    {
        int idx = from - 1;

        while (idx >= 0
                && char.IsWhiteSpace(str[idx]))
            idx--;

        if (!char.IsWhiteSpace(str[from - 1]))
            while (idx >= 0
                    && !char.IsWhiteSpace(str[idx]))
                idx--;

        if (!keepWhitespace)
            while (idx >= 0
                    && !char.IsWhiteSpace(str[idx]))
                idx--;

        return idx + 1;
    }
}
