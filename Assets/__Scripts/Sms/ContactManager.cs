using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> dialogs;
    [SerializeField] private List<Button> contacts;

    private void Start()
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            int idk = i;
            contacts[i].onClick.AddListener(() => onContactClick(idk));
        }
    }

    public void onContactClick(int index)
    {
        foreach (var item in dialogs)
        {
            item.gameObject.SetActive(false);
        }

        dialogs[index].gameObject.SetActive(true);
    }
}
