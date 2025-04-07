using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class SyntaxHighlighter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI syntaxHighlighter;
    public SyntaxHighlighterSO syntaxHighlighterSO;

    public Color numberColor;

    private void Start()
    {
        inputField.onValueChanged.AddListener(UpdateSyntaxHighlighting);
    }

    void UpdateSyntaxHighlighting(string text)
    {
        string highlightedText = text;

        highlightedText = Regex.Replace(highlightedText, @"(?<!<color=#[A-Fa-f0-9]{6}>)(\b\d+(\.\d+)?\b)", $"<color=#{ColorUtility.ToHtmlStringRGB(numberColor)}>$1</color>");

        foreach (var syntaxWord in syntaxHighlighterSO.words)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(syntaxWord.color);
            foreach (var word in syntaxWord.words)
            {
                highlightedText = Regex.Replace(highlightedText, $@"\b({word})\b", $"<color=#{colorHex}>$1</color>");
            }
        }

        syntaxHighlighter.text = highlightedText;
    }
}
