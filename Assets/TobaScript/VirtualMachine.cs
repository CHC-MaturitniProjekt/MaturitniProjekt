using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirtualMachine
{
    private int registerSize = 10;
    private int stackSize = 10;
    private int[] stack = new int[10];
    private int callStackSize = 10;
    private int[] callStack = new int[10];
    private int callStackPointer = -1;
    private int stackPointer = -1;
    private Dictionary<string, int> registers = new Dictionary<string, int>
    {
        { "R0", 0 }, { "R1", 0 }, { "R2", 0 }, { "R3", 0 }, { "R4", 0 },
        { "R5", 0 }, { "R6", 0 }, { "R7", 0 }, { "R8", 0 }, { "R9", 0 },
    };

    private Dictionary<string, int> outputRegisters = new Dictionary<string, int>
    {
        { "O0", 0 }, { "O1", 0 }, { "O2", 0 }, { "O3", 0 }, { "O4", 0 },
        { "O5", 0 }, { "O6", 0 }, { "O7", 0 }, { "O8", 0 }, { "O9", 0 },
    };

    private Dictionary<string, int> inputRegisters = new Dictionary<string, int>
    {
        { "I0", 0 }, { "I1", 0 }, { "I2", 0 }, { "I3", 0 }, { "I4", 0 },
        { "I5", 0 }, { "I6", 0 }, { "I7", 0 }, { "I8", 0 }, { "I9", 0 },
    };

    private int CMP = 0;
    private List<Instruction> program = new();
    private int instructionPointer = 0;
    private Dictionary<string, int> labels = new();
    private int delayCounter = 0;

    public event Action<int[]> OnOutput;
    public event Action<string> OnError;
    public event Action<string> OnPrint;
    public event Action OnTick;

    public void LoadProgram(List<Instruction> instructions)
    {
        program = instructions;
        for (int i = 0; i < program.Count; i++)
        {
            if (program[i].OpCode == OpCode.LABEL)
            {
                labels[program[i].Operands[0]] = i;
            }
        }
    }

    public bool HasInstructions()
    {
        return instructionPointer < program.Count;
    }

    public void setInputRegisters(Dictionary<string, int> register)
    {
        inputRegisters = register;
    }


    public Dictionary<string, int> getRegisters()
    {
        return registers;
    }

    public void Tick()
    {
        if (delayCounter > 0)
        {
            delayCounter--;
            OnTick?.Invoke();
            return;
        }

        if (instructionPointer < 0 || instructionPointer >= program.Count)
        {
            OnError?.Invoke($"Invalid instruction pointer: {instructionPointer}");
            return;
        }

        ExecuteInstruction(program[instructionPointer]);
        instructionPointer++;
        OnTick?.Invoke();
    }

    public int currentInstruction()
    {
        return instructionPointer;
    }

    private int GetOperandValue(string operand)
    {
        return registers.ContainsKey(operand) ? registers[operand] : int.Parse(operand);
    }

    private int GetJumpOperandValue(string operand)
    {
        return labels.ContainsKey(operand) ? labels[operand] : int.Parse(operand);
    }

    private int ReadRegister(string name)
    {
        if (registers.ContainsKey(name))
            return registers[name];
        if (inputRegisters.ContainsKey(name))
            return inputRegisters[name];

        return int.Parse(name);
    }

    private void WriteRegister(string name, int value)
    {
        if (value > 999)
            value = 999;

        if (registers.ContainsKey(name))
            registers[name] = value;
        else if (outputRegisters.ContainsKey(name))
            outputRegisters[name] = value;
        else
            OnError?.Invoke($"Unknown register: {name}");
    }

    private void ExecuteInstruction(Instruction instr)
    {
        try
        {
            switch (instr.OpCode)
            {
                case OpCode.LABEL:
                    break;
                case OpCode.MOVE:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.ADD:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[0]) + ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.SUB:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[0]) - ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.JUMP:
                    int target = GetJumpOperandValue(instr.Operands[0]);
                    if (target < 0 || target >= program.Count)
                    {
                        OnError?.Invoke($"Invalid jump target: {target}");
                    }
                    else
                    {
                        instructionPointer = target - 1;
                    }
                    break;

                case OpCode.PRINT:
                    OnPrint?.Invoke($"[{instr.Operands[0]}] = {ReadRegister(instr.Operands[0])}");
                    break;

                case OpCode.EXIT:
                    instructionPointer = program.Count;
                    break;

                case OpCode.AND:
                    WriteRegister(instr.Operands[0],   ReadRegister(instr.Operands[0]) & ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.OR:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[0]) | ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.XOR:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[0]) ^ ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.NOT:
                    WriteRegister(instr.Operands[0], ~ReadRegister(instr.Operands[1]));
                    break;

                case OpCode.INC:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[1])+1);
                    break;

                case OpCode.DEC:
                    WriteRegister(instr.Operands[0], ReadRegister(instr.Operands[1]) - 1);
                    break;

                case OpCode.PUSH:
                    if (stackPointer + 1 >= stackSize)
                    {
                        OnError?.Invoke("Stack overflow");
                    }
                    else
                    {
                        stack[++stackPointer] = ReadRegister(instr.Operands[0]);
                    }
                    break;

                case OpCode.POP:
                    if (stackPointer < 0)
                    {
                        OnError?.Invoke("Stack underflow");
                    }
                    else
                    {
                        WriteRegister(instr.Operands[0], stack[stackPointer--]);
                    }
                    break;

                case OpCode.CALL:
                    if (callStackPointer + 1 >= callStackSize)
                    {
                        OnError?.Invoke("Stack overflow on CALL");
                    }
                    else
                    {
                        int callTarget = GetJumpOperandValue(instr.Operands[0]);
                        if (callTarget < 0 || callTarget >= program.Count)
                        {
                            OnError?.Invoke($"Invalid CALL target: {callTarget}");
                        }
                        else
                        {
                            callStack[++callStackPointer] = instructionPointer;
                            instructionPointer = callTarget - 1;
                        }
                    }
                    break;

                case OpCode.RET:
                    if (stackPointer < 0)
                    {
                        OnError?.Invoke("Stack underflow on RET");
                    }
                    else
                    {
                        instructionPointer = callStack[callStackPointer--];
                    }
                    break;

                case OpCode.COMPARE:
                    CMP = ReadRegister(instr.Operands[0]) - ReadRegister(instr.Operands[1]);
                    break;

                case OpCode.JUMP_IF_EQUAL:
                    if (CMP == 0)
                    {
                        int jumpTarget = GetJumpOperandValue(instr.Operands[0]);
                        if (jumpTarget < 0 || jumpTarget >= program.Count)
                        {
                            OnError?.Invoke($"Invalid JUMP_IF_EQUAL target: {jumpTarget}");
                        }
                        else
                        {
                            instructionPointer = jumpTarget - 1;
                        }
                    }
                    break;

                case OpCode.JUMP_IF_NOT_EQUAL:
                    if (CMP != 0)
                    {
                        int jumpTarget = GetJumpOperandValue(instr.Operands[0]);
                        if (jumpTarget < 0 || jumpTarget >= program.Count)
                        {
                            OnError?.Invoke($"Invalid JUMP_IF_NOT_EQUAL target: {jumpTarget}");
                        }
                        else
                        {
                            instructionPointer = jumpTarget - 1;
                        }
                    }
                    break;

                case OpCode.JUMP_IF_GREATER:
                    if (CMP > 0)
                    {
                        int jumpTarget = GetJumpOperandValue(instr.Operands[0]);
                        if (jumpTarget < 0 || jumpTarget >= program.Count)
                        {
                            OnError?.Invoke($"Invalid JUMP_IF_GREATER target: {jumpTarget}");
                        }
                        else
                        {
                            instructionPointer = jumpTarget - 1;
                        }
                    }
                    break;

                case OpCode.JUMP_IF_LESS:
                    if (CMP < 0)
                    {
                        int jumpTarget = GetJumpOperandValue(instr.Operands[0]);
                        if (jumpTarget < 0 || jumpTarget >= program.Count)
                        {
                            OnError?.Invoke($"Invalid JUMP_IF_LESS target: {jumpTarget}");
                        }
                        else
                        {
                            instructionPointer = jumpTarget - 1;
                        }
                    }
                    break;

                case OpCode.RANDOM:
                    WriteRegister(instr.Operands[0], UnityEngine.Random.Range(ReadRegister(instr.Operands[1]), ReadRegister(instr.Operands[2])));
                    break;

                case OpCode.OUTPUT:
                    int[] outputArray = new int[10];
                    for (int i = 0; i < 10; i++)
                    {
                        outputArray[i] = outputRegisters[$"O{i}"];
                    }
                    OnOutput?.Invoke(outputArray);
                    break;

                case OpCode.DELAY:
                        delayCounter = ReadRegister(instr.Operands[0]);
                    break;
                default:
                    OnError?.Invoke($"Unknown opcode: {instr.OpCode}");
                    break;
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Error executing instruction {instr.OpCode}: {e.Message}");
        }
    }
}
