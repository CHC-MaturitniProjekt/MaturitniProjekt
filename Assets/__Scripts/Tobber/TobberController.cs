using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TobberController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Settings")]
    [SerializeField] private Color backgroundColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color highlightColor;

    [Header("Lists")]
    [SerializeField] private List<string> MainMenu;

    private int currentSelectedIndex = 0;
    private List<GameObject> currentListOfItems;

    void Start()
    {
        input.TobberOnUp += onButtonUp;
        input.TobberOnDown += onButtonDown;
        input.TobberOnEnter += onButtonEnter;
        input.TobberOnBack += onButtonBack;
    }

    private void onButtonBack()
    {
        throw new System.NotImplementedException();
    }

    private void onButtonEnter()
    {
        throw new System.NotImplementedException();
    }

    private void onButtonDown()
    {
        currentSelectedIndex++;
    }

    private void onButtonUp()
    {
        currentSelectedIndex--;
        if (currentSelectedIndex < 0)
            currentSelectedIndex = 0;
    }
}
