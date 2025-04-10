using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> windowTopBars;
    [SerializeField] private List<GameObject> appIcons;

    void Start()
    {
        for (int i = 0; i < appIcons.Count; i++)
        {
            int index = i;
            appIcons[i].GetComponent<Button>().onClick.AddListener(() => onAppIconClick(index));
        }

        for (int i = 0; i < windowTopBars.Count; i++)
        {
            int index = i;
            windowTopBars[i].transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => hideWindow(index));
        }
    }

    public void onAppIconClick(int index)
    {
        windowTopBars[index].transform.parent.gameObject.SetActive(true);
    }

    public void hideWindow(int index)
    {
        windowTopBars[index].transform.parent.gameObject.SetActive(false);
    }
}
