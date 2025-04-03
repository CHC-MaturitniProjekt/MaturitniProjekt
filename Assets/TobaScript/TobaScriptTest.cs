using System.Collections.Generic;
using UnityEngine;

class TobanScriptTest : MonoBehaviour
{
    void Start()
    {
        VirtualMachine vm = new VirtualMachine();

        List<Instruction> program = new List<Instruction>
        {
            new Instruction(OpCode.MOVE, "R0", "0"),  // Fib(0) = 0
            new Instruction(OpCode.MOVE, "R1", "1"),  // Fib(1) = 1
            new Instruction(OpCode.MOVE, "R2", "10"), // Counter = 10
            new Instruction(OpCode.MOVE, "R3", "2"),  // i = 2
            new Instruction(OpCode.PRINT, "R0"),
            new Instruction(OpCode.PRINT, "R1"),

            new Instruction(OpCode.LABEL, "LOOP"),
            new Instruction(OpCode.COMPARE, "R3", "R2"), // if i >= 10, exit
            new Instruction(OpCode.JUMP_IF_EQUAL, "EXIT"),

            new Instruction(OpCode.ADD, "R0", "R1"), // Fib(n) = Fib(n-1) + Fib(n-2)
            new Instruction(OpCode.MOVE, "R4", "R1"),
            new Instruction(OpCode.MOVE, "R1", "R0"),
            new Instruction(OpCode.MOVE, "R0", "R4"),
            new Instruction(OpCode.PRINT, "R1"),
            new Instruction(OpCode.INC, "R3"), // i++
            new Instruction(OpCode.JUMP, "LOOP"),

            new Instruction(OpCode.LABEL, "EXIT"),
            new Instruction(OpCode.EXIT)
        };

        vm.LoadProgram(program);
        vm.OnError += (msg) => Debug.LogError("VM Error: " + msg);
        VirtualMachineRunner.Instance.Initialize(vm);
        VirtualMachineRunner.Instance.Run();
    }
}
