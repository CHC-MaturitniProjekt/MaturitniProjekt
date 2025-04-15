# Virtual Machine Assembly Language

## Overview
This document describes the custom assembly-like language executed by the Virtual Machine. The language consists of human-readable mnemonics for low-level operations on registers and memory.

## Registers
The virtual machine has several types of registers:

### General Registers (`R0`–`R9`)
Used for general-purpose calculations and operations:
```
R0, R1, R2, R3, R4, R5, R6, R7, R8, R9
```

### Input Registers (`I0`–`I9`)
Used to provide input values from the external system into the VM:
```
I0, I1, I2, I3, I4, I5, I6, I7, I8, I9
```

### Output Registers (`O0`–`O9`)
Used to send output values from the VM to the external system:
```
O0, O1, O2, O3, O4, O5, O6, O7, O8, O9
```

## Instructions
Each instruction has a name (`OpCode`) and zero to three operands. Below is a list of all supported instructions.

### Data Movement
| Instruction     | Description                           |
|------------------|--------------------------------------|
| `MOVE A, B`      | Moves the value from `B` to `A`.     |

### Stack
| Instruction   | Description                   |
|----------------|------------------------------|
| `PUSH A`       | Pushes value of `A` to stack |
| `POP A`        | Pops top of stack into `A`   |

### Arithmetic
| Instruction     | Description        |
|------------------|-------------------|
| `ADD A, B`       | `A = A + B`       |
| `SUB A, B`       | `A = A - B`       |
| `INC A, B`       | `A = B + 1`       |
| `DEC A, B`       | `A = B - 1`       |

### Logic Operations

| Instruction     | Description        |
|------------------|-------------------|
| `AND A, B`       | `A = A & B`       |
| `OR A, B`        | `A = A | B`       |
| `XOR A, B`       | `A = A ^ B`       |
| `NOT A, B`       | `A = ~B`          |

### Jumps & Control Flow
| Instruction             | Description                           |
|--------------------------|--------------------------------------|
| `LABEL name`             | Defines a jump target label.         |
| `JUMP label`             | Jumps to the given label.            |
| `COMPARE A, B`           | Sets `CMP = A - B`                   |
| `JUMP_IF_EQUAL label`    | Jumps if `CMP == 0`                  |
| `JUMP_IF_NOT_EQUAL label`| Jumps if `CMP != 0`                  |
| `JUMP_IF_GREATER label`  | Jumps if `CMP > 0`                   |
| `JUMP_IF_LESS label`     | Jumps if `CMP < 0`                   |
| `EXIT`                   | Terminates the program.              |

### Random Number Generation
| Instruction            | Description                                       |
|-------------------------|--------------------------------------------------|
| `RANDOM A, MIN, MAX`    | Sets `A` to a random integer between MIN and MAX |

### Output & Debugging
| Instruction   | Description                                    |
|----------------|-----------------------------------------------|
| `PRINT A`      | Prints the value in register `A`              |
| `OUTPUT`       | Triggers an output event with all `O0`–`O9`   |

### Delay
| Instruction   | Description                           |
|----------------|--------------------------------------|
| `DELAY A`      | Pauses execution for `A` ticks       |

### Subroutines
| Instruction   | Description                                  |
|----------------|---------------------------------------------|
| `CALL label`   | Calls subroutine, stores return address     |
| `RET`          | Returns from subroutine                     |

---

## Example: Fibonacci Sequence (First 10 Numbers)
```
MOVE R0, 0       ; a = 0
MOVE R1, 1       ; b = 1
MOVE R2, 10      ; number of iterations

LABEL loop
PRINT R0         ; print a

MOVE R3, R0      ; R3 = a
ADD R3, R1       ; R3 = R3 + b

MOVE R0, R1      ; a = b
MOVE R1, R3      ; b = R3

DEC R2
COMPARE R2 0     ; decrement iteration counter
JUMP_IF_NOT_EQUAL loop

EXIT
```