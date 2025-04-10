using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.EventTrigger;

public class FileManagerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private TMP_InputField pathInput;
    [SerializeField] private Transform filesParent;
    [SerializeField] private PcStorage pcStorage;
    [SerializeField] private GameObject fileSelectPrefab;
    [SerializeField] private GameObject folderSelectPrefab;
    [SerializeField] private GameObject couldNotFind;
    [SerializeField] private GameObject emptyFolder;
    [SerializeField] private GameObject rightClickMenu;
    [SerializeField] private RightClickMenuController rightClickMenuController;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera pcCamera;

    private string currentFileName = "";
    private GameObject currentRightClickedGameObject;
    private SelectType curretType;

    enum SelectType
    {
        FILE,
        FOLDER
    }

    private void Start()
    {
        pathInput.onValueChanged.AddListener(OnPathInputChange);
        input.PcLeftClickStart += onLeftClickStart;
        input.PcRightClickStart += onRightClickStart;
        RefreshDisplay();
    }

    private void onLeftClickStart()
    {
        if (!IsPointerOverUI(rightClickMenu))
        {
            rightClickMenu.SetActive(false);
            return;
        }
    }

    private void onRightClickStart()
    {
        if (!IsPointerOverUIByTag("File") && !IsPointerOverUIByTag("Folder") && IsPointerOverUI(gameObject))
        {
            currentFileName = "";
            showRightClickModal();
        }
    }


    private void showRightClickModal(string filename = "")
    {
        Vector2 targetPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
          canvasRect,
          UnityEngine.Input.mousePosition,
          pcCamera,
          out targetPosition
      );
        rightClickMenu.SetActive(true);
        rightClickMenu.GetComponent<RectTransform>().anchoredPosition = targetPosition;
        rightClickMenuController.initMenu(filename);
    }

    private void OnPathInputChange(string path)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        foreach (Transform child in filesParent)
        {
            Destroy(child.gameObject);
        }

        string path = pathInput.text;
        string[] entries = pcStorage.GetAllScriptNames(path);

        if (entries == null)
        {
            couldNotFind.SetActive(true);
            emptyFolder.SetActive(false);
            return;
        }
        else
            couldNotFind.SetActive(false);

        emptyFolder.SetActive(entries.Length == 0);

        foreach (var entry in entries)
        {
            GameObject go;

            if (entry.EndsWith(".tbs"))
            {
                go = Instantiate(fileSelectPrefab, filesParent);
                go.GetComponent<Button>().onClick.AddListener(() => onFileClick(entry));
                go.tag = "File";
                AddRightClickListener(go, SelectType.FILE, entry);
            }
            else
            {
                go = Instantiate(folderSelectPrefab, filesParent);
                go.tag = "Folder";
                go.GetComponent<Button>().onClick.AddListener(() => onFolderClick(entry));
                AddRightClickListener(go, SelectType.FOLDER, entry);
            }

            TMP_InputField label = go.GetComponentInChildren<TMP_InputField>();
            if (label != null)
            {
                label.text = entry;
            }
        }
    }

    public void onBackClick()
    {
        if (string.IsNullOrEmpty(pathInput.text))
            return;

        string path = pathInput.text.TrimEnd('/'); 
        int lastSlashIndex = path.LastIndexOf('/');

        if (lastSlashIndex > 0)
        {
            pathInput.text = path.Substring(0, lastSlashIndex + 1);
        }
        else
        {
            pathInput.text = "";
        }
    }

    private void onFolderClick(string name)
    {
        pathInput.text += $"{name}/";
    }

    private void onFileClick(string name)
    {

    }

    public void onDeleteClick()
    {
        if (currentFileName == "") return;

        switch (curretType)
        {
            case SelectType.FILE:
                pcStorage.DeleteScript(currentFileName,pathInput.text);
                break;
            case SelectType.FOLDER:
                pcStorage.DeleteFolder(pathInput.text + currentFileName);
                break;
        }
        rightClickMenu.SetActive(false);
        RefreshDisplay();
    }

    public void onNewFolderClick()
    {
        string newFolderName = pcStorage.CreateFolder(pathInput.text);
        rightClickMenu.SetActive(false);
        RefreshDisplay();
        foreach (Transform child in filesParent)
        {
            TMP_InputField inputField = child.GetComponentInChildren<TMP_InputField>();
            if (inputField != null && inputField.text == newFolderName)
            {
                inputField.readOnly = false;
                inputField.interactable = true;
                inputField.Select();
                inputField.ActivateInputField();

                inputField.onEndEdit.AddListener(newName =>
                {
                    if (!string.IsNullOrWhiteSpace(newName) && newName != newFolderName)
                    {
                        pcStorage.RenameFolder(newFolderName, newName);
                    }

                    inputField.interactable = false;
                    inputField.readOnly = true;
                    RefreshDisplay();
                });

                break;
            }
        }

        rightClickMenu.SetActive(false);
    }

    public void onRenameClick()
    {
        TMP_InputField inputField = currentRightClickedGameObject.GetComponentInChildren<TMP_InputField>();
        inputField.readOnly = false;
        inputField.interactable = true;
        inputField.Select();
        inputField.ActivateInputField();

        inputField.onEndEdit.AddListener(newName =>
        {
            if (!string.IsNullOrWhiteSpace(newName) && newName != currentFileName)
            {
                switch (curretType)
                { 
                    case SelectType.FILE:
                        pcStorage.RenameScript(pathInput.text + currentFileName.Replace(".tbs", ""), newName);
                        break;
                    case SelectType.FOLDER:
                        pcStorage.RenameFolder(pathInput.text + currentFileName, newName);
                        break;
                }
            }

            inputField.interactable = false;
            inputField.readOnly = true;
            RefreshDisplay();
        });
        rightClickMenu.SetActive(false);
    }

    private void AddRightClickListener(GameObject fileObject, SelectType selectType ,string fileName)
    {
        EventTrigger trigger = fileObject.transform.GetChild(1).gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryClick = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        entryClick.callback.AddListener((data) =>
        {
            PointerEventData pointerEventData = (PointerEventData)data;
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                switch (selectType)
                {
                    case SelectType.FILE:
                        OnFileRightClick((PointerEventData)data, fileName, fileObject);
                        break;
                    case SelectType.FOLDER:
                        OnFolderRightClick((PointerEventData)data, fileName, fileObject);
                        break;
                }
            }

            if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
                switch (selectType)
                {
                    case SelectType.FILE:
                        onFileClick(fileName);
                        break;
                    case SelectType.FOLDER:
                        onFolderClick(fileName);
                        break;
                }
            }

        });

        trigger.triggers.Add(entryClick);
    }

    private bool IsPointerOverUI(GameObject target)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = UnityEngine.Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == target)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointerOverUIByTag(string tag)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = UnityEngine.Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    private void OnFileRightClick(PointerEventData eventData, string fileName, GameObject fileObject)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            currentFileName = fileName;
            curretType = SelectType.FILE;
            currentRightClickedGameObject = fileObject;
            showRightClickModal(fileName);
        }
    }

    private void OnFolderRightClick(PointerEventData eventData, string fileName, GameObject fileObject)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            currentFileName = fileName;
            curretType = SelectType.FOLDER;
            currentRightClickedGameObject = fileObject;
            showRightClickModal(fileName);
        }
    }
}
