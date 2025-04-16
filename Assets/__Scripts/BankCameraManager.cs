using UnityEngine;

public class BankCameraManager : MonoBehaviour
{
    public Animator animator;
    public GameObject cone;
    public void Unlock()
    {
        animator.SetTrigger("Open");
        cone.SetActive(false);
    }
}
