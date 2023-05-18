using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System;


namespace EasyHue
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string input = "intexample = 2; stringexample = \"this is a string\";";
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new EasyHueLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            EasyHueParser parser = new EasyHueParser(tokens);
            parser.BuildParseTree = true;

            // Parse tree
            IParseTree tree = parser.program();
            Console.WriteLine("Hello World!");

            // Code generation
            CodeGenerator codeGenerator = new CodeGenerator();
            string generatedCode = codeGenerator.GenerateCode(tree.GetText());

            Console.WriteLine("Generated Code:");
            Console.WriteLine(generatedCode);
        }

    }
}

     
