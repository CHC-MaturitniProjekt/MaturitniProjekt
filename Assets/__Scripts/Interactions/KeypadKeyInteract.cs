using System.Threading.Tasks;
using UnityEngine;

public class KeypadKeyInteract : InteractAction
{
    [SerializeField] private string character;

    public override void OnInteract()
    {
        GetComponentInParent<KeypadController>().enterCharacter(character);  
    }

    public override Task OnObjectiveInteract()
    {
        throw new System.NotImplementedException();
    }
}
