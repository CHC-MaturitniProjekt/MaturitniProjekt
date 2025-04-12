using System;
using System.Collections;
using System.Collections.Generic;
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
    private int CMP = 0;
    private List<Instruction> program = new();
    private int instructionPointer = 0;
    private Dictionary<string, int> labels = new();

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

    public Dictionary<string, int> getRegisters()
    {
        return registers;
    }

    public void Tick()
    {
        if (instructionPointer < 0 || instructionPointer >= program.Count)
        {
            OnError?.Invoke($"Invalid instruction pointer: {instructionPointer}");
            return;
        }

        ExecuteInstruction(program[instructionPointer]);
        instructionPointer++;
       // OnTick.Invoke();
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

    private void ExecuteInstruction(Instruction instr)
    {
        try
        {
            switch (instr.OpCode)
            {
                case OpCode.LABEL:
                    break;
                case OpCode.MOVE:
                    registers[instr.Operands[0]] = GetOperandValue(instr.Operands[1]);
                    break;

                case OpCode.ADD:
                    registers[instr.Operands[0]] += GetOperandValue(instr.Operands[1]);
                    break;

                case OpCode.SUB:
                    registers[instr.Operands[0]] -= GetOperandValue(instr.Operands[1]);
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
                    OnPrint?.Invoke($"[{instr.Operands[0]}] = {registers[instr.Operands[0]]}");
                    break;

                case OpCode.EXIT:
                    instructionPointer = program.Count;
                    break;

                case OpCode.AND:
                    registers[instr.Operands[0]] &= GetOperandValue(instr.Operands[1]);
                    break;

                case OpCode.OR:
                    registers[instr.Operands[0]] |= GetOperandValue(instr.Operands[1]);
                    break;

                case OpCode.XOR:
                    registers[instr.Operands[0]] ^= GetOperandValue(instr.Operands[1]);
                    break;

                case OpCode.NOT:
                    registers[instr.Operands[0]] = ~registers[instr.Operands[0]];
                    break;

                case OpCode.INC:
                    registers[instr.Operands[0]]++;
                    break;

                case OpCode.DEC:
                    registers[instr.Operands[0]]--;
                    break;

                case OpCode.PUSH:
                    if (stackPointer + 1 >= stackSize)
                    {
                        OnError?.Invoke("Stack overflow");
                    }
                    else
                    {
                        stack[++stackPointer] = registers[instr.Operands[0]];
                    }
                    break;

                case OpCode.POP:
                    if (stackPointer < 0)
                    {
                        OnError?.Invoke("Stack underflow");
                    }
                    else
                    {
                        registers[instr.Operands[0]] = stack[stackPointer--];
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
                    CMP = registers[instr.Operands[0]] - registers[instr.Operands[1]];
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
                    registers[instr.Operands[0]] = UnityEngine.Random.Range(GetOperandValue(instr.Operands[1]), GetOperandValue(instr.Operands[2]));
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
