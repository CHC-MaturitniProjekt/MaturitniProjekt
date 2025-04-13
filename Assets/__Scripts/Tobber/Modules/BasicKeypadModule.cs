using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class BasicKeypadModule : PuzzleModule
{
    [Header("Settings")]
    [SerializeField] private string passkey = "1234";
    public override void setInput(int[] input)
    {
        for (int i = 0; i < passkey.Length && i < input.Length; i++)
        {
            int expectedDigit = passkey[i] - '0';
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
