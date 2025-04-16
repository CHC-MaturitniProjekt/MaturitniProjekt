using System;
using UnityEngine;
using UnityEngine.Events;


class ReverseNfcModule : PuzzleModule
{
    [Header("Inspector Events")]
    public VoidEvent OnValidReverseDetected;

    public override void setInput(int[] input)
    {
        inputPorts = input;

        bool isReversed = true;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] != outputPorts[input.Length - 1 - i])
            {
                isReversed = false;
                break;
            }
        }

        if (isReversed)
        {
            Debug.Log("Nfc Finished");
            OnValidReverseDetected?.Invoke();
        }
    }
}
