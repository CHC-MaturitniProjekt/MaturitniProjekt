using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodeTextHistoryController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private TMP_InputField inputField;

    [Header("Settings")]
    [SerializeField] private const int MaxHistory = 1000;

    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();
    private string lastText = "";

    void Start()
    {
        inputField.onValueChanged.AddListener(OnInputChanged);
        input.PcOnUndo += Undo;
        input.PcOnRedo += Redo;
    }

    private void OnInputChanged(string newText)
    {
        if (newText != lastText)
        {
            PushUndo(lastText);
            lastText = newText;
            redoStack.Clear();
        }
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            redoStack.Push(inputField.text);
            string prevText = undoStack.Pop();
            SetInputFieldText(prevText);
            inputField.caretPosition = prevText.Length;
            lastText = prevText;
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(inputField.text);
            string nextText = redoStack.Pop();
            SetInputFieldText(nextText);
            inputField.caretPosition = nextText.Length;
            lastText = nextText;
        }
    }

    private void SetInputFieldText(string newText)
    {
        inputField.onValueChanged.RemoveListener(OnInputChanged);
        inputField.text = newText;
        inputField.onValueChanged.AddListener(OnInputChanged);
    }

    private void PushUndo(string text)
    {
        undoStack.Push(text);

        if (undoStack.Count > MaxHistory)
        {
            var temp = new Stack<string>(undoStack);
            var queue = new Queue<string>(temp);
            queue.Dequeue();
            undoStack = new Stack<string>(queue);
        }
    }

}
