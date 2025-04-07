using System.Collections.Generic;
using TMPro;
using UnityEngine;

class TobanScriptManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private Transform LogParrent;
    [SerializeField] private GameObject errorLogPrefab;
    [SerializeField] private GameObject printLogPrefab;

    private VirtualMachine debugVm;


    public void onStartDebugClick()
    {
        VirtualMachine vm = new VirtualMachine();

        Lexer lexer = new Lexer(codeText.text);
        List<Instruction> tokens = lexer.Tokenize();


        vm.LoadProgram(tokens);
        vm.OnError += (msg) => Debug.LogError("VM Error: " + msg);
        VirtualMachineRunner.Instance.Initialize(vm);
        VirtualMachineRunner.Instance.Run();
    }

    public void DebugNext()
    {
        VirtualMachine vm = new VirtualMachine();

        Lexer lexer = new Lexer(codeText.text);
        List<Instruction> tokens = lexer.Tokenize();


        vm.LoadProgram(tokens);
        vm.OnError += (msg) => Debug.LogError("VM Error: " + msg);
        VirtualMachineRunner.Instance.Initialize(vm);
        VirtualMachineRunner.Instance.Run();
    }

    public void onPlayClick()
    {
        VirtualMachine vm = new VirtualMachine();

        Lexer lexer = new Lexer(codeText.text);
        List<Instruction> tokens = lexer.Tokenize();


        vm.LoadProgram(tokens);
        vm.OnError += printErrorLog;
        vm.OnPrint += printPrintLog; 
        VirtualMachineRunner.Instance.Initialize(vm);
        VirtualMachineRunner.Instance.Run();
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
