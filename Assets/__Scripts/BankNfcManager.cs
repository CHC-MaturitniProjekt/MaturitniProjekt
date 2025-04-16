using UnityEngine;

public class BankNfcManager : MonoBehaviour
{
    public Animator animator;
    
    public void Unlock()
    {
        animator.SetTrigger("Open");
    }
}
