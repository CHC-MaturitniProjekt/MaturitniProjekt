using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class SyntaxHighlighter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI syntaxHighlighter;
    [SerializeField] private SyntaxHighlighterSO syntaxHighlighterSO;
    [Header("Settings")]
    [SerializeField] private Color numberColor;
    [SerializeField] private Color debugHighlightColor = Color.red;

    private int debugLineIndex = -1;

    private void Start()
    {
        inputField.onValueChanged.AddListener(UpdateSyntaxHighlighting);
    }

    public void SetDebugLine(int index)
    {
        debugLineIndex = index;
        UpdateSyntaxHighlighting(inputField.text);
    }

    void UpdateSyntaxHighlighting(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            line = Regex.Replace(line, @"(?<!<color=#[A-Fa-f0-9]{6}>)(\b\d+(\.\d+)?\b)", $"<color=#{ColorUtility.ToHtmlStringRGB(numberColor)}>$1</color>");

            foreach (var syntaxWord in syntaxHighlighterSO.words)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(syntaxWord.color);
                foreach (var word in syntaxWord.words)
                {
                    line = Regex.Replace(line, $@"\b({word})\b", $"<color=#{colorHex}>$1</color>");
                }
            }

            if (i == debugLineIndex)
            {
                string colorHex = ColorUtility.ToHtmlStringRGBA(debugHighlightColor);
                line = $"<mark=#{colorHex}>{line}</mark>";
            }

            lines[i] = line;
        }

        syntaxHighlighter.text = string.Join("\n", lines);
    }
}
