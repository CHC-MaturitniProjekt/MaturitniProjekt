using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class SyntaxHighlighter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI syntaxHighlighter;

    private void Start()
    {
        inputField.onValueChanged.AddListener(UpdateSyntaxHighlighting);
    }

    void UpdateSyntaxHighlighting(string text)
    {
        string highlightedText = text;

        // Příklad pro C# klíčová slova
        highlightedText = Regex.Replace(highlightedText, @"\b(public|private|void|class|new|if|else)\b", "<color=#ff6600>$1</color>");

        // Další pravidla (čísla, stringy, komentáře)
        highlightedText = Regex.Replace(highlightedText, @"(\"".*?\"")", "<color=#00ff00>$1</color>"); // stringy
        highlightedText = Regex.Replace(highlightedText, @"(//.*?$)", "<color=#808080>$1</color>", RegexOptions.Multiline); // komentáře

        syntaxHighlighter.text = highlightedText;
    }
}
