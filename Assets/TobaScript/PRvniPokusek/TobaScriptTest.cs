//using System.Collections.Generic;
//using UnityEngine;

//class TobanScriptTest : MonoBehaviour
//{
//    void Start()
//    {
//        string code = "var x = 5; var y = x + 20; print(y);";

//        Lexer lexer = new Lexer(code);
//        List<Token> tokens = lexer.Tokenize();

//        Parser parser = new Parser(tokens);
//        List<AstNode> ast = parser.Parse();

//        Interpreter interpreter = new Interpreter();
//        interpreter.Execute(ast);
//    }
//}
