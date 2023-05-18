using Antlr4.Runtime.Tree;

namespace EasyHue
{
    public interface IEasyHueVisitor<T> : IParseTreeVisitor<T>
    {
        T VisitProgram(EasyHueParser.ProgramContext context);
        T VisitStatement(EasyHueParser.StatementContext context);
        T VisitFunctionDefinition(EasyHueParser.FunctionDefinitionContext context);
        T VisitAssignment(EasyHueParser.AssignmentContext context);
        T VisitFunctionCall(EasyHueParser.FunctionCallContext context);
        T VisitKeywordCall(EasyHueParser.KeywordCallContext context);
        T VisitKeyword(EasyHueParser.KeywordContext context);
        T VisitParameters(EasyHueParser.ParametersContext context);
        T VisitArguments(EasyHueParser.ArgumentsContext context);
        T VisitExpression(EasyHueParser.ExpressionContext context);
        T VisitBinaryOperator(EasyHueParser.BinaryOperatorContext context);
        T VisitIfStatement(EasyHueParser.IfStatementContext context);
        T VisitBlock(EasyHueParser.BlockContext context);
        T VisitReturnStatement(EasyHueParser.ReturnStatementContext context);
    }
}
