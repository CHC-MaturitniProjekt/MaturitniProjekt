# Virtual Machine Assembly Language

## Overview
This document describes the custom assembly-like language executed by the Virtual Machine. The language consists of human-readable mnemonics for low-level operations on registers and memory.

## Registers
The virtual machine has 10 general-purpose registers:

```
R0, R1, R2, R3, R4, R5, R6, R7, R8, R9
```
Additionally, there is a special register used for comparisons:
```
CMP
```

## Instructions
The following instructions are supported:

### Data Manipulation
- `MOVE <register> <value|register>` - Move a value into a register.
- `ADD <register> <value|register>` - Add a value to a register.
- `SUB <register> <value|register>` - Subtract a value from a register.
- `AND <register> <value|register>` - Perform bitwise AND.
- `OR <register> <value|register>` - Perform bitwise OR.
- `XOR <register> <value|register>` - Perform bitwise XOR.
- `NOT <register>` - Perform bitwise NOT.
- `INC <register>` - Increment a register.
- `DEC <register>` - Decrement a register.

### Stack Operations
- `PUSH <register>` - Push register value onto the stack.
- `POP <register>` - Pop value from the stack into a register.

### Control Flow
- `JUMP <label>` - Jump to a label.
- `COMPARE <register> <register>` - Compare two registers and store the result in `CMP`.
- `JUMP_IF_EQUAL <label>` - Jump if `CMP` is 0.
- `JUMP_IF_NOT_EQUAL <label>` - Jump if `CMP` is not 0.
- `JUMP_IF_GREATER <label>` - Jump if `CMP` is greater than 0.
- `JUMP_IF_LESS <label>` - Jump if `CMP` is less than 0.

### Subroutines
- `CALL <label>` - Call a subroutine.
- `RET` - Return from a subroutine.

### Miscellaneous
- `PRINT <register>` - Print the value of a register.
- `RANDOM <register> <min> <max>` - Store a random number in a register.
- `EXIT` - Stop execution.

### Labels
Labels are markers in the program used for jumps and calls. A label is defined using:
```
LABEL <name>
```
Example usage:
```
LABEL start
MOVE R1, 5
JUMP start
```

## Example: Fibonacci Sequence (First 10 Numbers)
```
MOVE R0, 0
MOVE R1, 1
MOVE R2, 10  ; Counter
LABEL loop
PRINT R0
ADD R3, R0, R1
MOVE R0, R1
MOVE R1, R3
DEC R2
JUMP_IF_NOT_EQUAL loop
EXIT
```

