public enum TokenType
{
    Identifier, Number, Operator, Assignment, Keyword, Print, EndOfStatement, EOF, Loop
}

public class Token
{
    public TokenType Type;
    public string Value;

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}
