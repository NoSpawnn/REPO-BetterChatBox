using System;
using System.Linq;
using BepInEx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace BetterChatBox;

internal static class InputHandlers
{
    internal static void HandleTextInput(ref ChatManager chatManagerInstance, ref int cursorPos, ref string chatMessage)
    {
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
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    int deleteStart = cursorPos - 1;

                    while (deleteStart >= 0
                            && char.IsWhiteSpace(chatMessage[deleteStart]))
                        deleteStart--;

                    if (!char.IsWhiteSpace(chatMessage[cursorPos - 1]))
                        while (deleteStart >= 0
                                && !char.IsWhiteSpace(chatMessage[deleteStart]))
                            deleteStart--;

                    deleteStart += 1;
                    chatMessage = chatMessage.Remove(deleteStart, cursorPos - deleteStart);
                    cursorPos = deleteStart;
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
                    int deleteEnd = cursorPos;

                    while (deleteEnd < chatMessage.Length
                            && char.IsWhiteSpace(chatMessage[deleteEnd]))
                        deleteEnd++;

                    if (!char.IsWhiteSpace(chatMessage[cursorPos]))
                        while (deleteEnd < chatMessage.Length
                                && !char.IsWhiteSpace(chatMessage[deleteEnd]))
                            deleteEnd++;

                    chatMessage = chatMessage.Remove(cursorPos, deleteEnd - cursorPos);
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
            if (chatMessage.Length < BetterChatBox.MAX_CHARS)
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
        if (Input.GetKeyDown(KeyCode.UpArrow) && chatManagerInstance.chatHistory.Count > 0)
        {
            if (chatManagerInstance.chatHistoryIndex > 0)
                chatManagerInstance.chatHistoryIndex--;
            else
                chatManagerInstance.chatHistoryIndex = chatManagerInstance.chatHistory.Count - 1;

            chatMessage = chatManagerInstance.chatHistory[chatManagerInstance.chatHistoryIndex];
            ChatPatch.ChatHistoryChangeEffect();
            cursorPos = chatMessage.Length;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && chatManagerInstance.chatHistory.Count > 0)
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
    internal static void HandleCursorNavigation(ref ChatManager chatManagerInstance, ref int cursorPos, string chatMessage)
    {
        if (Input.GetKeyUp(KeyCode.RightArrow)) BetterChatBox.Instance.RightArrowHeld = false;
        if (Input.GetKeyUp(KeyCode.LeftArrow)) BetterChatBox.Instance.LeftArrowHeld = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && cursorPos > 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                int nextCursorPos = cursorPos - 1;
                while (nextCursorPos >= 0
                        && char.IsWhiteSpace(chatMessage[nextCursorPos]))
                    nextCursorPos--;
                while (nextCursorPos >= 0
                        && !char.IsWhiteSpace(chatMessage[nextCursorPos]))
                    nextCursorPos--;
                cursorPos = nextCursorPos + 1;
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
                int nextCursorPos = cursorPos;
                while (nextCursorPos < chatMessage.Length
                        && char.IsWhiteSpace(chatMessage[nextCursorPos]))
                    nextCursorPos++;
                while (nextCursorPos < chatMessage.Length
                        && !char.IsWhiteSpace(chatMessage[nextCursorPos]))
                    nextCursorPos++;
                cursorPos = nextCursorPos;
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
                if (clipboardText.IsNullOrWhiteSpace() || newText.Length >= BetterChatBox.MAX_CHARS)
                {
                    ChatPatch.ChatFailInputEffect();
                }
                else
                {
                    chatMessage = newText;
                    cursorPos = newText.Length;
                    chatManagerInstance.TypeEffect(Color.yellow);
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (BetterChatBox.CutToSystemClipboard.Value) GUIUtility.systemCopyBuffer = chatMessage;
                chatMessage = "";
                cursorPos = 0;
                chatManagerInstance.CharRemoveEffect();
            }
        }
    }
}
