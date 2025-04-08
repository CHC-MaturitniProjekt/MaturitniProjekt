using System;
using System.Collections.Generic;

public class Lexer
{
    private string input;
    private int position;

    public Lexer(string input)
    {
        this.input = input;
        this.position = 0;
    }

    public List<Instruction> Tokenize()
    {
        List<Instruction> instructions = new List<Instruction>();
        while (position < input.Length)
        {
            SkipWhitespaceAndNewlines();

            if (position >= input.Length)
                break;

            string word = Clean(ReadWord()).ToUpper();

            if (Enum.TryParse<OpCode>(word, out var opcode))
            {
                List<string> operands = new List<string>();

                while (true)
                {
                    SkipWhitespace();
                    if (position >= input.Length || input[position] == '\n')
                        break;

                    string operand = Clean(ReadOperand());
                    if (!string.IsNullOrEmpty(operand))
                        operands.Add(operand);

                    SkipWhitespace();
                    if (position < input.Length && input[position] == ',') position++;
                }

                instructions.Add(new Instruction(opcode, operands.ToArray()));
            }
            else
            {
                throw new Exception($"Unknown opcode or label: '{word}'");
            }

            SkipWhitespaceAndNewlines();
        }

        return instructions;
    }

    private string Clean(string input)
    {
        return input.Replace("\u200B", "")
                    .Replace("\u200C", "")
                    .Replace("\u200D", "")
                    .Replace("\uFEFF", "")
                    .Trim();
    }

    private void SkipWhitespace()
    {
        while (position < input.Length && char.IsWhiteSpace(input[position]) && input[position] != '\n')
            position++;
    }

    private void SkipWhitespaceAndNewlines()
    {
        while (position < input.Length && char.IsWhiteSpace(input[position]))
            position++;
    }

    private string ReadWord()
    {
        SkipWhitespace();
        int start = position;

        while (position < input.Length && (char.IsLetterOrDigit(input[position]) || input[position] == '_'))
            position++;

        return input.Substring(start, position - start);
    }

    private string ReadOperand()
    {
        SkipWhitespace();

        int start = position;
        while (position < input.Length && !char.IsWhiteSpace(input[position]) && input[position] != ',')
            position++;

        return input.Substring(start, position - start);
    }
}
