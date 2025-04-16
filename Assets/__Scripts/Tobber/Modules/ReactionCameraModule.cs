using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ReactionCameraModule : PuzzleModule
{
    [Header("Inspector Events")]
    public VoidEvent OnSequenceSuccess;

    [SerializeField] private int sequenceLength = 5;

    private int currentStep = 0;
    private List<Rule> rules = new();

    private void Start()
    {
        GenerateSequence();
    }

    public override void setInput(int[] input)
    {
        if (input == null || input.Length == 0) return;

        int value = input[0];

        if (!rules[currentStep].Validate(value))
        {
            currentStep = 0;
            UpdateOutput();
            return;
        }

        currentStep++;

        if (currentStep >= sequenceLength)
        {
            OnSequenceSuccess?.Invoke();
            currentStep = 0;
            GenerateSequence();
        }

        UpdateOutput();
    }

    void GenerateSequence()
    {
        rules.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            rules.Add(Rule.Random());
        }

        UpdateOutput();
    }

    void UpdateOutput()
    {
        for (int i = 0; i < outputPorts.Length; i++)
            outputPorts[i] = 0;

        outputPorts[0] = (int)rules[currentStep].type;

        OnOutputUpdate?.Invoke(outputPorts);
    }

    [System.Serializable]
    public class Rule
    {
        public enum Type { ExpectEven = 0, ExpectOdd = 1, ExpectGreaterThan4 = 2, ExpectLessThan5 = 3 }

        public Type type;

        public static Rule Random()
        {
            return new Rule
            {
                type = (Type)UnityEngine.Random.Range(0, 4)
            };
        }

        public bool Validate(int input)
        {
            return type switch
            {
                Type.ExpectEven => input % 2 == 0,
                Type.ExpectOdd => input % 2 != 0,
                Type.ExpectGreaterThan4 => input > 4,
                Type.ExpectLessThan5 => input < 5,
                _ => false,
            };
        }

        public override string ToString() => type.ToString();
    }

}
