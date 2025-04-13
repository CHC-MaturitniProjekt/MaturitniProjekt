using System;
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
        Modules,
        RunningScript,
        ModulNotFound,
        FatalError
    }

    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("UI References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RectTransform itemParent;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private RegisterViewController registerViewController;
    [SerializeField] private GameObject MenuObject;
    [SerializeField] private GameObject ScriptIsRunningObject;
    [SerializeField] private TextMeshProUGUI LogText;
    [SerializeField] private TobberModule tobberModule;
    [SerializeField] private GameObject ModuleNotFoundScreen;
    [SerializeField] private GameObject FatalErrorScreen;

    [Header("Settings")]
    [SerializeField] private int itemsOnPage = 5;
    [SerializeField] private Color backgroundColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color moduleSelectColor;

    private List<string> mainMenu = new List<string> { "Scripts", "Modules" };
    private List<string> loadedScripts = new List<string>();
    private List<string> modules = new List<string> { "KeyPad", "Camera" };

    private MenuState currentState = MenuState.Main;

    private int selectedIndex = 0;
    private int? selectedModuleIndex = null;
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
            bool isModuleSelected = currentState == MenuState.Modules && selectedModuleIndex.HasValue && selectedModuleIndex.Value == i;
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

            if(isModuleSelected)
            {
                bgImage.color = moduleSelectColor;
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
        if (currentState == MenuState.RunningScript) return;

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
        if (currentState == MenuState.RunningScript) return;

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
        if (currentState == MenuState.RunningScript) 
        {
            Instance_OnEnd();
            return;
        }

        if(currentState == MenuState.ModulNotFound)
        {
            MenuObject.SetActive(true);
            ModuleNotFoundScreen.SetActive(false);
            currentState = MenuState.Scripts;
            return;
        }

        if (currentState == MenuState.FatalError)
        {
            MenuObject.SetActive(true);
            FatalErrorScreen.SetActive(false);
            currentState = MenuState.Scripts;
            return;
        }

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
                if(selectedModuleIndex != null)
                {
                    bool objectFound = tobberModule.FindModule(modules[selectedModuleIndex.Value]);
                    if(!objectFound)
                    {
                        currentState = MenuState.ModulNotFound;
                        MenuObject.SetActive(false);
                        ModuleNotFoundScreen.SetActive(true);
                        return;
                    }

                }
                ScriptStart();
                break;

            case MenuState.Modules:
                if (selectedModuleIndex.HasValue && selectedModuleIndex.Value == selectedIndex)
                {
                    selectedModuleIndex = null;
                }
                else
                {
                    selectedModuleIndex = selectedIndex;
                }

                UpdateSelection();
                break;
        }
    }

    private void ScriptStart()
    {
        MenuObject.SetActive(false);
        ScriptIsRunningObject.SetActive(true);
        currentState = MenuState.RunningScript;
        registerViewController.resetRegisters();
        LogText.text = "";

        string code = "";
        string path = Path.Combine(TobberDirectory, loadedScripts[selectedIndex] + ".tbs");
        if (File.Exists(path))
        {
            code = File.ReadAllText(path);
        }

        VirtualMachine vm = new VirtualMachine();
        Lexer lexer = new Lexer(code);

        List<Instruction> tokens = lexer.Tokenize();
        vm.LoadProgram(tokens);
        vm.OnTick += Vm_OnTick;
        vm.OnError += onError;
        vm.OnPrint += onLog;
        vm.OnOutput += OnOutput;
        VirtualMachineRunner.Instance.Initialize(vm);
        VirtualMachineRunner.Instance.OnEnd += Instance_OnEnd;
        VirtualMachineRunner.Instance.Run();
    }

    public void OnOutput(int[] output)
    {
        tobberModule.setInput(output);
    }

    private void Instance_OnEnd()
    {

        MenuObject.SetActive(true);
        currentState = MenuState.Scripts;
        ScriptIsRunningObject.SetActive(false);
        VirtualMachineRunner.Instance.Stop();
    }

    private void onError(string err)
    {
        ScriptIsRunningObject.SetActive(false);
        FatalErrorScreen.SetActive(true);
        currentState = MenuState.FatalError;
        VirtualMachineRunner.Instance.Stop();

        FatalErrorScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = err;
    }

    private void onLog(string msg)
    {
        LogText.text = msg;
    }

    private void Vm_OnTick()
    {
        int[] register = tobberModule.getOutput();
        Dictionary<string, int> outputArray = new Dictionary<string,int>();
        for (int i = 0; i < register.Length; i++)
        {
            outputArray.Add($"I{i}", register[i]);
        }
        VirtualMachineRunner.Instance.setIpnutRegister(outputArray);
        registerViewController.updateRegisters(VirtualMachineRunner.Instance.getRegisters());
    }

    private void GoBack()
    {
        if (currentState == MenuState.RunningScript) return;

        if (currentState != MenuState.Main)
        {
            LoadMenu(mainMenu, MenuState.Main);
        }
    }
}
