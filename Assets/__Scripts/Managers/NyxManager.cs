using UnityEngine;

public class NyxManager : MonoBehaviour
{
    public static NyxManager Instance { get; private set; }

    public bool medicationComplete = false;
    public bool maloobchodComplete = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setMedication()
    {
        Instance.medicationComplete = true;
    }

    public void setMaloObchod()
    {
        Instance.maloobchodComplete = true;
    }
    
}
