using Antlr4.Runtime.Tree;

namespace EasyHue
{
    public abstract class EasyHueBaseVisitor<T> : IEasyHueVisitor<T>
    {
        public abstract T VisitProgram(EasyHueParser.ProgramContext context);
        public abstract T VisitStatement(EasyHueParser.StatementContext context);
        public abstract T VisitFunctionDefinition(EasyHueParser.FunctionDefinitionContext context);
        public abstract T VisitAssignment(EasyHueParser.AssignmentContext context);
        public abstract T VisitFunctionCall(EasyHueParser.FunctionCallContext context);
        public abstract T VisitKeywordCall(EasyHueParser.KeywordCallContext context);
        public abstract T VisitKeyword(EasyHueParser.KeywordContext context);
        public abstract T VisitParameters(EasyHueParser.ParametersContext context);
        public abstract T VisitArguments(EasyHueParser.ArgumentsContext context);
        public abstract T VisitExpression(EasyHueParser.ExpressionContext context);
        public abstract T VisitBinaryOperator(EasyHueParser.BinaryOperatorContext context);
        public abstract T VisitIfStatement(EasyHueParser.IfStatementContext context);
        public abstract T VisitBlock(EasyHueParser.BlockContext context);
        public abstract T VisitReturnStatement(EasyHueParser.ReturnStatementContext context);

        public virtual T Visit(IParseTree tree)
        {
            return tree.Accept(this);
        }

        public virtual T VisitChildren(IRuleNode node)
        {
            T result = DefaultResult;
            int count = node.ChildCount;
            for (int i = 0; i < count; i++)
            {
                if (!ShouldVisitNextChild(node, result))
                {
                    break;
                }
                IParseTree child = node.GetChild(i);
                T childResult = child.Accept(this);
                result = AggregateResult(result, childResult);
            }
            return result;
        }

        public virtual T VisitTerminal(ITerminalNode node)
        {
            return DefaultResult;
        }

        public virtual T VisitErrorNode(IErrorNode node)
        {
            return DefaultResult;
        }

        protected virtual T DefaultResult => default(T);

        protected virtual bool ShouldVisitNextChild(IRuleNode node, T currentResult)
        {
            return true;
        }

        protected virtual T AggregateResult(T aggregate, T nextResult)
        {
            return nextResult;
        }
    }
}
