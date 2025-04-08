using System.Collections.Generic;
using System;
using UnityEngine;

public class Interpreter
{
    private Dictionary<string, int> variables = new Dictionary<string, int>();

    public void Execute(List<AstNode> statements)
    {
        foreach (var statement in statements)
        {
            ExecuteStatement(statement);
        }
    }

    private void ExecuteStatement(AstNode node)
    {
        if (node is AssignmentNode assign)
        {
            variables[assign.VariableName] = Evaluate(assign.Expression);
        }
        else if (node is PrintNode print)
        {
            Debug.Log(Evaluate(print.Expression));
        }
    }

    private int Evaluate(AstNode node)
    {
        if (node is NumberNode number)
        {
            return number.Value;
        }

        if (node is VariableNode variable)
        {
            if (!variables.ContainsKey(variable.Name))
            {
                throw new Exception("Neznámá proměnná: " + variable.Name);
            }
            return variables[variable.Name];
        }

        if (node is BinaryOperationNode binary)
        {
            int left = Evaluate(binary.Left);
            int right = Evaluate(binary.Right);

            return binary.Operator switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                _ => throw new Exception("Neznámý operátor: " + binary.Operator)
            };
        }

        throw new Exception("Neznámý výraz");
    }
}
