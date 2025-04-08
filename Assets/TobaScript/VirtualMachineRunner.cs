using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualMachineRunner : MonoBehaviour
{
    private static VirtualMachineRunner instance;
    private VirtualMachine vm;
    private float tickInterval = 0.1f;
    private bool isRunning = false;

    public static VirtualMachineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("VirtualMachineRunner");
                instance = obj.AddComponent<VirtualMachineRunner>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    public void Initialize(VirtualMachine virtualMachine, float interval = 0.1f)
    {
        vm = virtualMachine;
        tickInterval = interval;
    }

    public void Run()
    {
        if (vm == null)
        {
            Debug.LogError("VirtualMachine is not initialized!");
            return;
        }
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(RunVM());
        }
    }

    public void Stop()
    {
        isRunning = false;
    }

    private IEnumerator RunVM()
    {
        while (isRunning)
        {
            if (vm == null || !isRunning)
                yield break;

            if (vm.HasInstructions())
            {
                vm.Tick();
                yield return new WaitForSeconds(tickInterval);
            }
            else
            {
                isRunning = false;
            }
        }
    }

    public Dictionary<string, int> getRegisters()
    {
        return vm.getRegisters();
    }

    public int getCurrentInstruction()
    {
        return vm.currentInstruction();
    }

    public bool HasInstructions()
    {
        return vm.HasInstructions();
    }

    public void Step()
    {
        if (vm == null)
            return;

        if (vm.HasInstructions())
        {
            vm.Tick();
        }
    }
}