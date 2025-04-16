using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class IndexKeypadModule : PuzzleModule
{
    [Header("References")]
    [SerializeField] private KeypadController keypadController;
    public override void setInput(int[] input)
    {
        for (int i = 0; i < keypadController.pass.Length && i < input.Length; i++)
        {
            int expectedDigit = keypadController.pass[i] - '0';
            if (input[i] == expectedDigit)
            {
                outputPorts[i] = 1;
            }
            else
            {
                outputPorts[i] = 0;
            }
        }

        OnOutputUpdate?.Invoke(outputPorts);
    }
}
