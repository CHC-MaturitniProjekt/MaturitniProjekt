using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

class BruteforceKeypadModule : PuzzleModule
{
    [Header("Inspector Events")]
    public VoidEvent OnValidBruteForce;

    public string pass = "12";

    public override void setInput(int[] input)
    {
        if(input.Length < pass.Length)
            return;

        for (int i = 0; i < pass.Length; i++)
        {
            if (input[i] != pass[i] - '0')
                return;
        }
        OnValidBruteForce?.Invoke();
    }
}
