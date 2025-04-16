using TMPro;
using UnityEngine;

public class KeypadController : MonoBehaviour
{
    public string pass = "123";
    public string text = "";
    public int maxCharCount;
    public TextMeshPro textMesh;

    public void enterCharacter(string character)
    {
        if(text == "Err")
        {
            text = "";
        }
        else if (text == "Succes")
        {
            return;
        }
        if (character == "*")
        {
            if (text == pass)
            {
                text = "Succes";
            }
            else
            {
                text = "Err";
            }
        }
        else if (character == "#")
        {
            if (text != "")
                text = text.Remove(text.Length - 1, 1);
        }
        else
        {
            if (maxCharCount != text.Length)
                text += character;
        }

        textMesh.text = text;
    }

    
}
