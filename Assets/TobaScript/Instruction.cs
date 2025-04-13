public enum OpCode
{
    //BASE
    ADD, SUB, PRINT, EXIT, LABEL,

    //OPERATIONS
    AND, OR, XOR, NOT, INC, DEC,

    //STACK
    PUSH, POP,

    //MEMORY
    MOVE,

    //JUMPS
    JUMP,

    //Subrutines
    CALL, RET,

    //IFS
    COMPARE, JUMP_IF_EQUAL, JUMP_IF_NOT_EQUAL, JUMP_IF_GREATER, JUMP_IF_LESS,

    //IO
    READ, WRITE,  //TODO

    //OTHERS
    RANDOM, DELAY, OUTPUT
}

public class Instruction
{
    public OpCode OpCode;
    public string[] Operands;

    public Instruction(OpCode opCode, params string[] operands)
    {
        OpCode = opCode;
        Operands = operands;
    }
}
