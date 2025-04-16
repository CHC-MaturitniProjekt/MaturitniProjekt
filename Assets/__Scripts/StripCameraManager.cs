using UnityEngine;

public class StripCameraManager : MonoBehaviour
{
    [SerializeField] private GameObject cone;
    [SerializeField] private IndexKeypadModule keypadModule;
    public void turnOff()
    {
        cone.SetActive(false);
        keypadModule.enabled = true;
    }
}
