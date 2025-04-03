using System.Collections.Generic;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class SitInteract : InteractAction
{
    public enum SitType { Bench, Sofa, Bed, Ground }
    [SerializeField] private SitType sitType;
    
    [SerializeField] private Transform benchTransform;

    private Transform playerTransform;
    private Animator playerAnimator;
    private Movement playerMovement;
    private CameraController playerCamera;
    private Vector3 lastPosition;

    private bool isSitting;
    private string sitAnimation;
    
    private void Start()
    {
        playerMovement = FindFirstObjectByType<Movement>();
        playerTransform = playerMovement.transform;
        playerAnimator = playerMovement.GetComponent<Animator>();
        playerCamera = FindFirstObjectByType<CameraController>();

        switch (sitType)
        {
            case SitType.Bench:
                sitAnimation = "isBenchSitting";
                break;
            case SitType.Ground:
                sitAnimation = "isGroundSitting";
                break;
            case SitType.Sofa:
                sitAnimation = "isSofaSitting";
                break;
            case SitType.Bed:
                sitAnimation = "isBedSitting";
                break;
            default:
                break;
        }
    }

    public override void OnInteract()
    {
        if (playerMovement.isSitting)
        {
            playerCamera.ResetCameraPosition();
            PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Idle);
            playerAnimator.SetBool(sitAnimation, false);
            playerMovement.isSitting = false;
            playerTransform.position = lastPosition;
            playerMovement.enabled = true;

            if (sitType == SitType.Bed)             //just for fun - remove later
            {
                playerTransform.rotation = new Quaternion(0, 0, 0,0);
            }
        }
        else
        {
            playerCamera.AdjustCameraHeight(sitType);
            lastPosition = playerTransform.position;
            PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Sitting);
            playerAnimator.SetBool(sitAnimation, true);
            playerTransform.position = benchTransform.position;
            playerTransform.rotation = benchTransform.rotation;
            playerMovement.isSitting = true;
            playerMovement.enabled = false;
            
            if (sitType == SitType.Bed)          //just for fun - remove later
            {
                playerTransform.rotation = new Quaternion(0, 0, 50,0);
            }
        }
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }
}