using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace EasyHue
{
    public class CodeGenerator
    {

        public string GenerateCode(string input)
        {
            // Create the necessary objects for parsing
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new EasyHueLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            EasyHueParser parser = new EasyHueParser(tokens);
            parser.BuildParseTree = true;

            // Parse tree
            IParseTree tree = parser.program();

            // Generate C code from the parse tree
            StringBuilder codeBuilder = new StringBuilder();
            GenerateCode(tree, codeBuilder);

            // Return the generated C code as a string
            return codeBuilder.ToString();
        }


        private void GenerateCode(IParseTree parseTree, StringBuilder codeBuilder)
        {
            if (parseTree is EasyHueParser.ProgramContext)
            {
                GenerateCodeForProgram(parseTree as EasyHueParser.ProgramContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.StatementContext)
            {
                GenerateCodeForStatement(parseTree as EasyHueParser.StatementContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.FunctionDefinitionContext)
            {
                GenerateCodeForFunctionDefinition(parseTree as EasyHueParser.FunctionDefinitionContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.AssignmentContext)
            {
                GenerateCodeForAssignment(parseTree as EasyHueParser.AssignmentContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.FunctionCallContext)
            {
                GenerateCodeForFunctionCall(parseTree as EasyHueParser.FunctionCallContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.KeywordCallContext)
            {
                GenerateCodeForKeywordCall(parseTree as EasyHueParser.KeywordCallContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.ExpressionContext)
            {
                GenerateCodeForExpression(parseTree as EasyHueParser.ExpressionContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.IfStatementContext)
            {
                GenerateCodeForIfStatement(parseTree as EasyHueParser.IfStatementContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.WhileStatementContext)
            {
                GenerateCodeForWhileStatement(parseTree as EasyHueParser.WhileStatementContext, codeBuilder);
            }
            else if (parseTree is EasyHueParser.ReturnStatementContext)
            {
                GenerateCodeForReturnStatement(parseTree as EasyHueParser.ReturnStatementContext, codeBuilder);
            }
        }

        private void GenerateCodeForProgram(EasyHueParser.ProgramContext program, StringBuilder codeBuilder)
        {
            for (int i = 0; i < program.ChildCount; i++)
            {
                IParseTree statement = program.GetChild(i);
                GenerateCode(statement, codeBuilder);
            }
        }

        private void GenerateCodeForStatement(EasyHueParser.StatementContext statement, StringBuilder codeBuilder)
        {
            IParseTree statementType = statement.GetChild(0);
            GenerateCode(statementType, codeBuilder);
        }

        private void GenerateCodeForFunctionDefinition(EasyHueParser.FunctionDefinitionContext functionDefinition, StringBuilder codeBuilder)
        {
            string functionName = functionDefinition.ID().GetText();
            codeBuilder.AppendLine($"void {functionName}() {{");

            // Generate code for the function body
            EasyHueParser.BlockContext block = functionDefinition.block();
            GenerateCodeForBlock(block, codeBuilder);

            codeBuilder.AppendLine("}");
        }

        private void GenerateCodeForBlock(EasyHueParser.BlockContext block, StringBuilder codeBuilder)
        {
            codeBuilder.AppendLine("{");
            for (int i = 0; i < block.statement().Length; i++)
            {
                EasyHueParser.StatementContext statement = block.statement(i);
                GenerateCode(statement, codeBuilder);
            }
            codeBuilder.AppendLine("}");
        }



        private void GenerateCodeForAssignment(EasyHueParser.AssignmentContext assignment, StringBuilder codeBuilder)
        {
            string variableName = assignment.ID().GetText();
            string expression = assignment.expression().GetText();
            codeBuilder.AppendLine($"{variableName} = {expression};");
        }

        private void GenerateCodeForFunctionCall(EasyHueParser.FunctionCallContext functionCall, StringBuilder codeBuilder)
        {
            string functionName = functionCall.ID().GetText();
            codeBuilder.AppendLine($"{functionName}();");
        }

        private void GenerateCodeForKeywordCall(EasyHueParser.KeywordCallContext keywordCall, StringBuilder codeBuilder)
        {
            string keywordName = keywordCall.keyword().GetText();
            codeBuilder.AppendLine($"{keywordName}();");
        }

        private void GenerateCodeForExpression(EasyHueParser.ExpressionContext expression, StringBuilder codeBuilder)
        {
            if (expression.INT() != null)
            {
                codeBuilder.Append(expression.INT().GetText());
            }
            else if (expression.STRING() != null)
            {
                codeBuilder.Append(expression.STRING().GetText());
            }
            else if (expression.ID() != null)
            {
                codeBuilder.Append(expression.ID().GetText());
            }
            else if (expression.functionCall() != null)
            {
                GenerateCodeForFunctionCall(expression.functionCall(), codeBuilder);
            }
            else if (expression.expression().Length == 1)
            {
                GenerateCodeForExpression(expression.expression()[0], codeBuilder);
            }
            else if (expression.binaryOperator() != null)
            {
                GenerateCodeForBinaryExpression(expression, codeBuilder);
            }
        }

        private void GenerateCodeForBinaryExpression(EasyHueParser.ExpressionContext expression, StringBuilder codeBuilder)
        {
            EasyHueParser.ExpressionContext leftExpression = expression.expression()[0];
            EasyHueParser.ExpressionContext rightExpression = expression.expression()[1];
            string binaryOperator = expression.binaryOperator().GetText();

            codeBuilder.Append("(");
            GenerateCodeForExpression(leftExpression, codeBuilder);
            codeBuilder.Append($" {binaryOperator} ");
            GenerateCodeForExpression(rightExpression, codeBuilder);
            codeBuilder.Append(")");
        }

        private void GenerateCodeForIfStatement(EasyHueParser.IfStatementContext ifStatement, StringBuilder codeBuilder)
        {
            codeBuilder.AppendLine("if (");
            GenerateCodeForExpression(ifStatement.expression(), codeBuilder);
            codeBuilder.AppendLine(") {");
            GenerateCode(ifStatement.block(0), codeBuilder);
            codeBuilder.AppendLine("}");

            if (ifStatement.block(1) != null)
            {
                codeBuilder.AppendLine("else {");
                GenerateCode(ifStatement.block(1), codeBuilder);
                codeBuilder.AppendLine("}");
            }
        }

        private void GenerateCodeForWhileStatement(EasyHueParser.WhileStatementContext whileStatement, StringBuilder codeBuilder)
        {
            codeBuilder.AppendLine("while (");
            GenerateCodeForExpression(whileStatement.expression(), codeBuilder);
            codeBuilder.AppendLine(") {");
            GenerateCode(whileStatement.block(), codeBuilder);
            codeBuilder.AppendLine("}");
        }

        private void GenerateCodeForReturnStatement(EasyHueParser.ReturnStatementContext returnStatement, StringBuilder codeBuilder)
        {
            codeBuilder.Append("return");

            if (returnStatement.expression() != null)
            {
                codeBuilder.Append(" ");
                GenerateCodeForExpression(returnStatement.expression(), codeBuilder);
            }

            codeBuilder.AppendLine(";");
        }


    }
}
