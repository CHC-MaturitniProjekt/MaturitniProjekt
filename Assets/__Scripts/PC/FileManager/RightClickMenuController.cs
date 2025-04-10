using UnityEngine;

public class RightClickMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject newFolderButton;
    [SerializeField] private GameObject renameButton;
    [SerializeField] private GameObject deleteButton;


    public void initMenu(string fileName = "")
    {
        if(fileName == "")
        {
            renameButton.SetActive(false);
            deleteButton.SetActive(false);
        }
        else
        {

            renameButton.SetActive(true);
            deleteButton.SetActive(true);
        }
    }
}
