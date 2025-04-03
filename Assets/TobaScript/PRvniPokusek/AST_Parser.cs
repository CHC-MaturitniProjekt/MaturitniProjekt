using System.Collections.Generic;
using System;

public abstract class AstNode { }

public class AssignmentNode : AstNode
{
    public string VariableName;
    public AstNode Expression;

    public AssignmentNode(string variableName, AstNode expression)
    {
        VariableName = variableName;
        Expression = expression;
    }
}

public class NumberNode : AstNode
{
    public int Value;

    public NumberNode(int value)
    {
        Value = value;
    }
}

public class VariableNode : AstNode
{
    public string Name;

    public VariableNode(string name)
    {
        Name = name;
    }
}

public class BinaryOperationNode : AstNode
{
    public AstNode Left;
    public string Operator;
    public AstNode Right;

    public BinaryOperationNode(AstNode left, string op, AstNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public class PrintNode : AstNode
{
    public AstNode Expression;

    public PrintNode(AstNode expression)
    {
        Expression = expression;
    }
}


public class Parser
{
    private List<Token> tokens;
    private int position = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Token Peek() => position < tokens.Count ? tokens[position] : new Token(TokenType.EOF, "");
    private Token Consume() => tokens[position++];

    public List<AstNode> Parse()
    {
        List<AstNode> statements = new List<AstNode>();

        while (Peek().Type != TokenType.EOF)
        {
            statements.Add(ParseStatement());
        }

        return statements;
    }

    private AstNode ParseStatement()
    {
        Token token = Consume();

        if (token.Type == TokenType.Keyword && token.Value == "var")
        {
            string variableName = Consume().Value;
            Consume(); // '='
            AstNode expression = ParseExpression();
            Consume(); // ';'
            return new AssignmentNode(variableName, expression);
        }

        if (token.Type == TokenType.Print)
        {
            AstNode expression = ParseExpression();
            Consume(); // ';'
            return new PrintNode(expression);
        }

        throw new Exception("Neznámý p?íkaz: " + token.Value);
    }

    private AstNode ParseExpression()
    {
        AstNode left = ParsePrimary();

        while (Peek().Type == TokenType.Operator)
        {
            string op = Consume().Value;
            AstNode right = ParsePrimary();
            left = new BinaryOperationNode(left, op, right);
        }

        return left;
    }

    private AstNode ParsePrimary()
    {
        Token token = Consume();

        if (token.Type == TokenType.Number)
            return new NumberNode(int.Parse(token.Value));

        if (token.Type == TokenType.Identifier)
            return new VariableNode(token.Value);
        

        throw new Exception("Neznámý token v primárním výrazu: " + token.Value);
    }
}
