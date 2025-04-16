using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

class FibonacciCameraModule : PuzzleModule
{
    [Header("Inspector Events")]
    public VoidEvent OnValidFibonacci;

    public int sequenceLength = 15;

    private List<int> sequence = new List<int>();

    public override void setInput(int[] input)
    {
        if (input == null || input.Length == 0)
            return;

        int next = input[0];
        sequence.Add(next);

        int count = sequence.Count;

        if (count == 1)
        {
            if (next != 0) ResetSequence();
        }
        else if (count == 2)
        {
            if (next != 1) ResetSequence();
        }
        else
        {
            int expected = sequence[count - 2] + sequence[count - 3];
            if (next != expected)
            {
                ResetSequence();
                return;
            }
        }

        if (count == sequenceLength)
        {
            OnValidFibonacci?.Invoke();
            sequence.Clear();
        }
    }

    private void ResetSequence()
    {
        sequence.Clear();
    }
}
