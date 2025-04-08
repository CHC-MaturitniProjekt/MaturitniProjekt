using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

class TobanScriptManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader input;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private Transform LogParrent;
    [SerializeField] private GameObject errorLogPrefab;
    [SerializeField] private GameObject printLogPrefab;

    private VirtualMachine debugVm;
    public int debugVmCurrentStep;

    public void onStartDebugClick()
    {
        stopDebug();

        debugVm = new VirtualMachine();
        input.PcOnStep += DebugNext;

        try
        {
            Lexer lexer = new Lexer(codeText.text);
            List<Instruction> tokens = lexer.Tokenize();
            debugVm.LoadProgram(tokens);
            debugVm.OnError += printErrorLog;
            debugVm.OnPrint += printPrintLog;
            VirtualMachineRunner.Instance.Initialize(debugVm);
            debugVmCurrentStep = 0;
        }
        catch (Exception err)
        {
            printErrorLog(err.Message);
        }
    }

    public void DebugNext()
    {
        if (debugVm != null)
        {
            VirtualMachineRunner.Instance.Step();
            debugVmCurrentStep = VirtualMachineRunner.Instance.getCurrentInstruction();
            if (!VirtualMachineRunner.Instance.HasInstructions())
                stopDebug();
        }
    }

    public void stopDebug()
    {
        input.PcOnStep -= DebugNext;
        debugVm = null;
        debugVmCurrentStep = 0;
    }

    public void onPlayClick()
    {
        if (debugVm == null)
            return;

        VirtualMachine vm = new VirtualMachine();
        Lexer lexer = new Lexer(codeText.text);

        try
        {
            List<Instruction> tokens = lexer.Tokenize();
            vm.LoadProgram(tokens);
            vm.OnError += printErrorLog;
            vm.OnPrint += printPrintLog;
            VirtualMachineRunner.Instance.Initialize(vm);
            VirtualMachineRunner.Instance.Run();
        }
        catch(Exception err)
        {
            printErrorLog(err.Message);
        }
    }

    private void printPrintLog(string msg)
    {
        GameObject logObject = Instantiate(printLogPrefab, LogParrent);
        logObject.GetComponent<LogController>().textComponent.text = msg;
    }

    private void printErrorLog(string msg)
    {
        GameObject logObject = Instantiate(errorLogPrefab, LogParrent);
        logObject.GetComponent<LogController>().textComponent.text = msg;
    }
}
