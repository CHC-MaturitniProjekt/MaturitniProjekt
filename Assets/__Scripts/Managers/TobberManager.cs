using PixelCrushers.DialogueSystem;
using UnityEngine;

public class TobberManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private GameObject tobber;
    [SerializeField] private CameraController cameraController;

    private bool show = false;

    void Start()
    {
        input.onTobber += onTobberDisplay; 
    }

    private void onTobberDisplay()
    {
        show = !show;

        if (show)
            Show();
        else
            Hide();
    }

    private void Show()
    {
        input.TobberInputEnable();
        tobber.SetActive(true);
        PlayerManager.Instance.isDisabled = true;
        cameraController.ResetCameraPosition();
        cameraController.AlignCameraForward();
    }

    private void Hide()
    {
        input.TobberInputDisable();
        tobber.SetActive(false);
        PlayerManager.Instance.isDisabled = false;
        cameraController.disableAligForward();
    }
}
