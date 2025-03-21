using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CodeIDE : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private float cursorBlinkInterval = 0.5f;
    [SerializeField] private float backspaceInitialDelay = 0.5f;
    [SerializeField] private float backspaceRepeatRate = 0.1f;

    private string codeText = "";
    private int cursorPosition;
    private int selectionStart = -1; // Počáteční index označeného textu
    private int selectionEnd = -1;   // Koncový index označeného textu
    private float lastBackspaceTime;
    private float lastCursorBlinkTime;
    private bool cursorVisible;
    private bool isSelecting = false;

    private void OnEnable()
    {
        Keyboard.current.onTextInput += HandleTextInput;
        cursorPosition = 0;
        UpdateDisplay();
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= HandleTextInput;
    }

    void Update()
    {
        HandleBackspace();
        HandleArrowNavigation();
        HandleEnter();
        HandleCursorBlink();
    }

    private void HandleTextInput(char c)
    {
        if (char.IsControl(c)) return;

        // Pokud je označený text, smažeme ho a nahradíme novým znakem
        if (HasSelection())
        {
            DeleteSelection();
        }

        codeText = codeText.Insert(cursorPosition, c.ToString());
        cursorPosition++;
        ResetSelection();
        UpdateDisplay();
    }

    private void HandleBackspace()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.backspaceKey.isPressed)
        {
            if (Time.realtimeSinceStartup - lastBackspaceTime > (keyboard.backspaceKey.wasPressedThisFrame ? backspaceInitialDelay : backspaceRepeatRate))
            {
                if (HasSelection())
                {
                    DeleteSelection();
                }
                else if (cursorPosition > 0 && codeText.Length > 0)
                {
                    codeText = codeText.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                }
                UpdateDisplay();
                lastBackspaceTime = Time.realtimeSinceStartup;
            }
        }
        else
        {
            lastBackspaceTime = 0;
        }
    }

    private void HandleArrowNavigation()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool shiftHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            MoveCursor(-1, shiftHeld);
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            MoveCursor(1, shiftHeld);
        }
    }

    private void MoveCursor(int direction, bool shiftHeld)
    {
        int newCursorPosition = Mathf.Clamp(cursorPosition + direction, 0, codeText.Length);

        if (shiftHeld)
        {
            if (!isSelecting)
            {
                selectionStart = cursorPosition;
                isSelecting = true;
            }
            selectionEnd = newCursorPosition;
        }
        else
        {
            ResetSelection();
        }

        cursorPosition = newCursorPosition;
        UpdateDisplay();
    }

    private void HandleEnter()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            if (HasSelection())
            {
                DeleteSelection();
            }

            codeText = codeText.Insert(cursorPosition, "\n");
            cursorPosition++;
            ResetSelection();
            UpdateDisplay();
        }
    }

    private void HandleCursorBlink()
    {
        if (Time.realtimeSinceStartup - lastCursorBlinkTime > cursorBlinkInterval)
        {
            cursorVisible = !cursorVisible;
            lastCursorBlinkTime = Time.realtimeSinceStartup;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        string leftPart = codeText.Substring(0, cursorPosition);
        string rightPart = codeText.Substring(cursorPosition);
        string highlightedText = codeText;

        if (HasSelection())
        {
            int start = Mathf.Min(selectionStart, selectionEnd);
            int end = Mathf.Max(selectionStart, selectionEnd);

            string before = codeText.Substring(0, start);
            string selected = codeText.Substring(start, end - start);
            string after = codeText.Substring(end);

            highlightedText = $"{before}<mark=#FFDD55>{selected}</mark>{after}";
        }

        textDisplay.text = highlightedText.Insert(cursorPosition, cursorVisible ? "|" : "");
    }

    private bool HasSelection()
    {
        return selectionStart != -1 && selectionEnd != -1 && selectionStart != selectionEnd;
    }

    private void ResetSelection()
    {
        selectionStart = -1;
        selectionEnd = -1;
        isSelecting = false;
    }

    private void DeleteSelection()
    {
        if (!HasSelection()) return;

        int start = Mathf.Min(selectionStart, selectionEnd);
        int end = Mathf.Max(selectionStart, selectionEnd);
        codeText = codeText.Remove(start, end - start);
        cursorPosition = start;

        ResetSelection();
    }
}
