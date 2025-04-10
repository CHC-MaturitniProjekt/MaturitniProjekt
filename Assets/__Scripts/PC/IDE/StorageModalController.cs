using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageModalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject storageModalObject;
    [SerializeField] private CodeStorage codeStorage;
    [SerializeField] private TMP_InputField codeTextInput;
    [SerializeField] private TMP_InputField newFileInput;
    [SerializeField] private Transform fileSelectParent;

    [SerializeField] private GameObject fileSelectPrefab;

    private string[] allScripts;
    private ModalType type = ModalType.NONE;

    public enum ModalType
    {
        NONE,
        SAVE,
        LOAD,
    }

    public void modalInitialization(ModalType type)
    {
        storageModalObject.SetActive(true);
        this.type = type;
        loadFiles();
    }

    public void loadFiles()
    {
        allScripts = codeStorage.GetAllScriptNames();
        foreach (var script in allScripts)
        {
            spawnFileSelectObject(script);
        }
    }

    public void onNewClick()
    {
        if (type == ModalType.SAVE)
            codeStorage.SaveCodeAs(newFileInput.text, codeTextInput.text);
        else if (type == ModalType.LOAD)
        {
            codeStorage.SaveCodeAs(newFileInput.text, "");
            codeTextInput.text = "";
        }
        closeModal();
    }

    public void onCancleClick()
    {
        closeModal();
    }

    private void onLoadFileClick(string fileName)
    {
        if (type == ModalType.SAVE)
            codeStorage.SaveCodeAs(fileName, codeTextInput.text);
        else if (type == ModalType.LOAD)
        {
            string code = codeStorage.LoadCodeNamed(fileName);
            codeTextInput.text = code;
        }

        closeModal();
    }

    private void onDeleteFileClick(string fileName)
    {
        codeStorage.DeleteScript(fileName);
        foreach (Transform child in fileSelectParent)
        {
            Destroy(child.gameObject);
        }
        loadFiles();
    }

    private void spawnFileSelectObject(string fileName)
    {
        GameObject temp = Instantiate(fileSelectPrefab, fileSelectParent);
        temp.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = fileName;

        Button selectButton = temp.transform.GetChild(0).GetComponent<Button>();
        Button deleteButton = temp.transform.GetChild(1).GetComponent<Button>();

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => onLoadFileClick(fileName));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => onDeleteFileClick(fileName));
        }
    }

    public void closeModal()
    {
        foreach (Transform child in fileSelectParent)
        {
            Destroy(child.gameObject);
        }
        newFileInput.text = "";
        type = ModalType.NONE;
        allScripts = new string[0];
        storageModalObject.SetActive(false);
    }

}
