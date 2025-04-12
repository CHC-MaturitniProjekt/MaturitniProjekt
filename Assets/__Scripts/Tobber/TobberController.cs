using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TobberController : MonoBehaviour
{
    private enum MenuState
    {
        Main,
        Scripts,
        Modules
    }

    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("UI References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RectTransform itemParent;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private RegisterViewController registerViewController;

    [Header("Settings")]
    [SerializeField] private int itemsOnPage = 5;
    [SerializeField] private Color backgroundColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color highlightColor;

    private List<string> mainMenu = new List<string> { "Scripts", "Modules" };
    private List<string> loadedScripts = new List<string>();
    private List<string> modules = new List<string> { "Lock", "Camera" };

    private MenuState currentState = MenuState.Main;

    private int selectedIndex = 0;
    private int scrollIndex = 0;
    private List<GameObject> itemObjects = new List<GameObject>();

    private string TobberDirectory => Path.Combine(Application.persistentDataPath, "Tobber");

    void Start()
    {
        input.TobberOnUp += MoveUp;
        input.TobberOnDown += MoveDown;
        input.TobberOnEnter += SelectItem;
        input.TobberOnBack += GoBack;
        input.TobberInputEnable();

        LoadMenu(mainMenu, MenuState.Main);
    }

    private void LoadMenu(List<string> items, MenuState newState)
    {
        currentState = newState;
        ClearMenu();

        foreach (var item in items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemParent);
            newItem.GetComponentInChildren<TMP_Text>().text = item;
            itemObjects.Add(newItem);
        }

        selectedIndex = 0;
        scrollIndex = 0;
        UpdateSelection();
    }

    private void ClearMenu()
    {
        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }
        itemObjects.Clear();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < itemObjects.Count; i++)
        {
            Image bgImage = itemObjects[i].GetComponent<Image>();
            Image icon = itemObjects[i].transform.GetChild(0).GetComponent<Image>();
            TMP_Text text = itemObjects[i].transform.GetChild(1).GetComponent<TMP_Text>();

            if (i == selectedIndex)
            {
                bgImage.color = highlightColor;
                icon.color = backgroundColor;
                text.color = backgroundColor;
            }
            else
            {
                bgImage.color = backgroundColor;
                icon.color = highlightColor;
                text.color = textColor;
            }
        }

        ApplyScroll();
    }

    private void ApplyScroll()
    {
        float itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height + layoutGroup.spacing;
        itemParent.anchoredPosition = new Vector2(0, scrollIndex * itemHeight);
    }

    private void MoveDown()
    {
        if (selectedIndex < itemObjects.Count - 1)
        {
            selectedIndex++;
            if (selectedIndex >= scrollIndex + itemsOnPage)
                scrollIndex++;
            UpdateSelection();
        }
    }

    private void MoveUp()
    {
        if (selectedIndex > 0)
        {
            selectedIndex--;
            if (selectedIndex < scrollIndex)
                scrollIndex--;
            UpdateSelection();
        }
    }

    private void LoadScripts()
    {
        loadedScripts = new List<string>();
        if (Directory.Exists(TobberDirectory))
        {
            string[] files = Directory.GetFiles(TobberDirectory);
            loadedScripts = files.Select(Path.GetFileNameWithoutExtension).ToList();
        }
    }

    private void SelectItem()
    {
        if (selectedIndex >= itemObjects.Count) return;

        switch (currentState)
        {
            case MenuState.Main:
                string selected = mainMenu[selectedIndex];
                if (selected == "Scripts")
                {
                    LoadScripts();
                    LoadMenu(loadedScripts, MenuState.Scripts);
                }
                else if (selected == "Modules")
                {
                    LoadMenu(modules, MenuState.Modules);
                }
                break;

            case MenuState.Scripts:
                Debug.Log($"Run script: {loadedScripts[selectedIndex]}");
                break;

            case MenuState.Modules:
                Debug.Log($"Activate module: {modules[selectedIndex]}");
                break;
        }
    }

    private void ScriptStart()
    {

    }

    private void GoBack()
    {
        if (currentState != MenuState.Main)
        {
            LoadMenu(mainMenu, MenuState.Main);
        }
    }
}
