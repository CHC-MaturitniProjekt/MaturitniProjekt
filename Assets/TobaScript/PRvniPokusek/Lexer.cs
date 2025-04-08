//using System.Collections.Generic;

//public class Lexer
//{
//    private string input;
//    private int position;

//    public Lexer(string input)
//    {
//        this.input = input;
//        this.position = 0;
//    }

//    public List<Token> Tokenize()
//    {
//        List<Token> tokens = new List<Token>();

//        while (position < input.Length)
//        {
//            char current = input[position];

//            if (char.IsWhiteSpace(current))
//            {
//                position++;
//                continue;
//            }

//            if (char.IsLetter(current))
//            {
//                string word = ReadWord();
//                switch(word)
//                {
//                    case "var":
//                        tokens.Add(new Token(TokenType.Keyword, word));
//                        break;
//                    case "print":
//                        tokens.Add(new Token(TokenType.Print, word));
//                        break;
//                    case "for":
//                        tokens.Add(new Token(TokenType.Print, word));
//                        break;
//                    default:
//                        tokens.Add(new Token(TokenType.Identifier, word));
//                        break;
//                }
//                continue;
//            }

//            if (char.IsDigit(current))
//            {
//                string number = ReadNumber();
//                tokens.Add(new Token(TokenType.Number, number));
//                continue;
//            }

//            if (current == '=')
//            {
//                tokens.Add(new Token(TokenType.Assignment, "="));
//                position++;
//                continue;
//            }

//            if ("+-*/".Contains(current))
//            {
//                tokens.Add(new Token(TokenType.Operator, current.ToString()));
//                position++;
//                continue;
//            }

//            if (current == ';')
//            {
//                tokens.Add(new Token(TokenType.EndOfStatement, ";"));
//                position++;
//                continue;
//            }

//            position++;
//        }

//        tokens.Add(new Token(TokenType.EOF, ""));
//        return tokens;
//    }

//    private string ReadWord()
//    {
//        int start = position;
//        while (position < input.Length && char.IsLetter(input[position]))
//            position++;

//        return input.Substring(start, position - start);
//    }

//    private string ReadNumber()
//    {
//        int start = position;
//        while (position < input.Length && char.IsDigit(input[position]))
//            position++;

//        return input.Substring(start, position - start);
//    }
//}
