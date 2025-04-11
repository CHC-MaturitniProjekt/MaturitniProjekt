using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TobberController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject itemParent;

    [Header("Settings")]
    [SerializeField] private int itemsOnPage = 5;
    [SerializeField] private Color backgroundColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color highlightColor;

    [Header("Lists")]
    [SerializeField] private List<string> MainMenu;

    private int currentSelectedIndex = 0;
    private int scrollIndex;
    private List<GameObject> currentListOfItems = new List<GameObject>();

    void Start()
    {
        input.TobberOnUp += onButtonUp;
        input.TobberOnDown += onButtonDown;
        input.TobberOnEnter += onButtonEnter;
        input.TobberOnBack += onButtonBack;
        input.TobberInputEnable();

        loadList(MainMenu);
        updateSelection();
    }

    private void loadList(List<string> list)
    {
        foreach (Transform child in itemParent.transform)
        {
            Destroy(child.gameObject);
        }
        currentListOfItems.Clear();

        foreach (var item in list)
        {
            GameObject temp = Instantiate(itemPrefab, itemParent.transform);
            temp.GetComponentInChildren<TMP_Text>().text = item;
            currentListOfItems.Add(temp);
        }
    }

    private void updateSelection()
    {
        if (currentListOfItems == null) return;

        for(int i = 0; i < currentListOfItems.Count; i++)
        {
            if(i == currentSelectedIndex)
            {
                currentListOfItems[i].GetComponent<Image>().color = highlightColor;
                currentListOfItems[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                currentListOfItems[i].transform.GetChild(1).GetComponent<TMP_Text>().color = backgroundColor;
            }
            else
            {
                currentListOfItems[i].GetComponent<Image>().color = backgroundColor;
                currentListOfItems[i].transform.GetChild(0).GetComponent<Image>().color = highlightColor;
                currentListOfItems[i].transform.GetChild(1).GetComponent<TMP_Text>().color = textColor;
            }
        }
    }
    
    private void applyScroll()
    {
        Vector3 temp = itemParent.GetComponent<RectTransform>().anchoredPosition;
        temp.y = scrollIndex * (itemPrefab.GetComponent<RectTransform>().rect.height + itemParent.GetComponent<VerticalLayoutGroup>().spacing);
        itemParent.GetComponent<RectTransform>().anchoredPosition = temp; 
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
        if (currentSelectedIndex > currentListOfItems.Count - 1)
            currentSelectedIndex = currentListOfItems.Count - 1;

        if (currentSelectedIndex > scrollIndex + (itemsOnPage - 2))
            scrollIndex++;

        if (currentSelectedIndex == currentListOfItems.Count - 1)
            scrollIndex--;

        updateSelection();
        applyScroll();
    }

    private void onButtonUp()
    {
        currentSelectedIndex--;
        if (currentSelectedIndex < 0)
            currentSelectedIndex = 0;

        if (currentSelectedIndex == scrollIndex)
            scrollIndex--;

        if (currentSelectedIndex == 0)
            scrollIndex++;

        updateSelection();
        applyScroll();
    }
}
