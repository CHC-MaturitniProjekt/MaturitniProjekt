using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class SyntaxHighlighter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI syntaxHighlighter;
    public SyntaxHighlighterSO syntaxHighlighterSO; // Odkaz na ScriptableObject

    private void Start()
    {
        inputField.onValueChanged.AddListener(UpdateSyntaxHighlighting);
    }

    void UpdateSyntaxHighlighting(string text)
    {
        string highlightedText = text;

        foreach (var syntaxWord in syntaxHighlighterSO.words)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(syntaxWord.color);
            highlightedText = Regex.Replace(highlightedText, $@"\b({syntaxWord.word})\b", $"<color=#{colorHex}>$1</color>");
        }

        syntaxHighlighter.text = highlightedText;
    }
}
