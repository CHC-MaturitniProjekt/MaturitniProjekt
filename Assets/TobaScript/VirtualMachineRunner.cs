using System;
using System.Collections.Generic;
using UnityEngine;

public class VirtualMachineRunner : MonoBehaviour
{
    private static VirtualMachineRunner instance;
    private VirtualMachine vm;
    private float tickInterval = 0.1f;
    private float tickTimer = 0f;
    private bool isRunning = false;

    public event Action OnEnd;

    public static VirtualMachineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VirtualMachineRunner>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("VirtualMachineRunner");
                    instance = obj.AddComponent<VirtualMachineRunner>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public void Initialize(VirtualMachine virtualMachine, float interval = 0.1f)
    {
        if (isRunning)
        {
            Debug.LogWarning("Trying to initialize while VM is running. Ignoring.");
            return;
        }

        vm = virtualMachine;
        tickInterval = interval;
        tickTimer = 0f;
    }

    public void Run()
    {
        if (vm == null)
        {
            Debug.LogError("VirtualMachine is not initialized!");
            return;
        }

        if (isRunning)
        {
            Debug.LogWarning("VirtualMachine is already running.");
            return;
        }

        isRunning = true;
        tickTimer = 0f;
    }

    public void Stop()
    {
        isRunning = false;
        vm = null;
    }

    void Update()
    {
        if (!isRunning || vm == null || !vm.HasInstructions())
            return;

        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval)
        {
            vm.Tick();
            tickTimer -= tickInterval;

            if (!vm.HasInstructions())
            {
                isRunning = false;
                OnEnd?.Invoke(); // ✅ správné spuštění eventu
                vm = null;
                break;
            }
        }
    }

    public void setIpnutRegister(Dictionary<string, int> register)
    {
        vm?.setInputRegisters(register);
    }

    public Dictionary<string, int> getRegisters()
    {
        return vm?.getRegisters();
    }

    public int getCurrentInstruction()
    {
        return vm?.currentInstruction() ?? -1;
    }

    public bool HasInstructions()
    {
        return vm?.HasInstructions() ?? false;
    }

    public void Step()
    {
        if (vm != null && vm.HasInstructions())
        {
            vm.Tick();

            if (!vm.HasInstructions())
            {
                isRunning = false;
                OnEnd?.Invoke();
                vm = null;
            }
        }
    }
}
