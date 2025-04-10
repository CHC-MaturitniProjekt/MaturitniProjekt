using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RegisterViewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<TextMeshProUGUI> registerTexts;

    public void updateRegisters(Dictionary<string, int> registers)
    {
        int i = 0;
        foreach (var reg in registers)
        {
            registerTexts[i].text = $"{reg.Key}: {reg.Value}";
            i++;
        }
    }

    public void resetRegisters()
    {
        for(int i = 0; i < registerTexts.Count; i++)
        {
            registerTexts[i].text = $"R{i}: 0";
        }
    }
}
