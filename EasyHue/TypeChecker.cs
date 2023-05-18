using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyHue
{
    public class TypeChecker : EasyHueBaseVisitor<Type>
    {
        private Dictionary<string, Type> variableTypes = new Dictionary<string, Type>();
        private Dictionary<string, FunctionSignature> functionSignatures = new Dictionary<string, FunctionSignature>();

        public override Type VisitBinaryOperator(EasyHueParser.BinaryOperatorContext context)
        {
            // Since the binary operator can have different types based on the specific operator, 
            // you can handle each operator separately and return the appropriate type.

            string operatorText = context.GetText();

            if (operatorText == "+" || operatorText == "-" || operatorText == "*" || operatorText == "/")
            {
                // Arithmetic operators always expect both operands to be of type int
                return Type.Int;
            }
            else if (operatorText == "==" || operatorText == "!=" || operatorText == "<" || operatorText == ">" ||
                     operatorText == "<=" || operatorText == ">=")
            {
                // Comparison operators always return a boolean result
                return Type.Boolean;
            }

            throw new TypeMismatchException($"Unknown binary operator: {operatorText}");
        }

        public void CheckTypes(EasyHueParser.ProgramContext context)
        {
            Visit(context);
        }
        public override Type VisitArguments(EasyHueParser.ArgumentsContext context)
        {
            // Visit each expression in the arguments and collect their types
            List<Type> argumentTypes = new List<Type>();
            foreach (var expressionContext in context.expression())
            {
                argumentTypes.Add(Visit(expressionContext));
            }

            return argumentTypes.FirstOrDefault(); // Return the first argument type (assuming a single return type for the arguments)
        }


        public override Type VisitProgram(EasyHueParser.ProgramContext context)
        {
            foreach (var statement in context.statement())
            {
                Visit(statement);
            }

            return null;
        }

        public override Type VisitStatement(EasyHueParser.StatementContext context)
        {
            if (context.functionDefinition() != null)
            {
                return Visit(context.functionDefinition());
            }
            else if (context.assignment() != null)
            {
                return Visit(context.assignment());
            }
            else if (context.keywordCall() != null)
            {
                return Visit(context.keywordCall());
            }
            else if (context.ifStatement() != null)
            {
                return Visit(context.ifStatement());
            }
            else if (context.returnStatement() != null)
            {
                return Visit(context.returnStatement());
            }

            throw new InvalidOperationException("Invalid statement");
        }
        public override Type VisitKeyword(EasyHueParser.KeywordContext context)
        {
            string keywordName = context.GetText();

            // Implement logic based on the keyword name
            switch (keywordName)
            {
                case "print":
                    // Handle print keyword
                    // ...
                    break;
                case "input":
                    // Handle input keyword
                    // ...
                    break;
                case "turn_on":
                    // Handle turn_on keyword
                    // ...
                    break;
                case "turn_off":
                    // Handle turn_off keyword
                    // ...
                    break;
                default:
                    throw new InvalidOperationException($"Unknown keyword: {keywordName}");
            }

            return null; // or return an appropriate type if necessary
        }

        public override Type VisitFunctionDefinition(EasyHueParser.FunctionDefinitionContext context)
        {
            string functionName = context.ID().GetText();
            List<Type> parameterTypes = new List<Type>();

            if (context.parameters() != null)
            {
                foreach (var parameterContext in context.parameters().ID())
                {
                    parameterTypes.Add(new Type(parameterContext.GetText()));
                }
            }

            if (functionSignatures.ContainsKey(functionName))
            {
                throw new DuplicateFunctionDefinitionException($"Function {functionName} is already defined");
            }

            functionSignatures[functionName] = new FunctionSignature(parameterTypes, null);

            Visit(context.block());

            return null;
        }



        public override Type VisitParameters(EasyHueParser.ParametersContext context)
        {
            List<Type> parameterTypes = new List<Type>();

            foreach (var parameterContext in context.ID())
            {
                parameterTypes.Add(new Type(parameterContext.GetText()));
            }

            // Assuming the function returns a single parameter type, you can return the first type from the list
            return parameterTypes.FirstOrDefault();
        }



        public override Type VisitBlock(EasyHueParser.BlockContext context)
        {
            foreach (var statementContext in context.statement())
            {
                Visit(statementContext);
            }

            return null;
        }

        public override Type VisitAssignment(EasyHueParser.AssignmentContext context)
        {
            string variableName = context.ID().GetText();
            Type expressionType = Visit(context.expression());

            if (variableTypes.ContainsKey(variableName))
            {
                Type variableType = variableTypes[variableName];
                if (!IsAssignable(variableType, expressionType))
                {
                    throw new TypeMismatchException($"Cannot assign expression of type {expressionType} to variable '{variableName}' of type {variableType}");
                }
            }
            else
            {
                variableTypes[variableName] = expressionType;
            }

            return null;
        }

        public override Type VisitKeywordCall(EasyHueParser.KeywordCallContext context)
        {
            string keywordName = context.keyword().GetText();

            switch (keywordName)
            {
                case "print":
                    // Handle print keyword
                    // ...
                    break;
                case "input":
                    // Handle input keyword
                    // ...
                    break;
                case "turn_on":
                    // Handle turn_on keyword
                    // ...
                    break;
                case "turn_off":
                    // Handle turn_off keyword
                    // ...
                    break;
                default:
                    throw new InvalidOperationException($"Unknown keyword: {keywordName}");
            }

            return null;
        }

        public override Type VisitIfStatement(EasyHueParser.IfStatementContext context)
        {
            Type conditionType = Visit(context.expression());

            if (conditionType != Type.Boolean)
            {
                throw new TypeMismatchException($"If statement condition must be a Boolean, but got {conditionType}");
            }

            Visit(context.block(0));

            if (context.block(1) != null)
            {
                Visit(context.block(1));
            }

            return null;
        }

        public override Type VisitReturnStatement(EasyHueParser.ReturnStatementContext context)
        {
            Type returnType = Visit(context.expression());

            FunctionSignature currentFunction = GetCurrentFunction();
            Type expectedReturnType = currentFunction.ReturnType;

            if (!IsAssignable(expectedReturnType, returnType))
            {
                throw new TypeMismatchException($"Cannot return expression of type {returnType} in function with return type {expectedReturnType}");
            }

            return null;
        }

        public override Type VisitFunctionCall(EasyHueParser.FunctionCallContext context)
        {
            string functionName = context.ID().GetText();
            if (!functionSignatures.ContainsKey(functionName))
            {
                throw new InvalidOperationException($"Function {functionName} is not defined");
            }

            FunctionSignature functionSignature = functionSignatures[functionName];

            List<Type> argumentTypes = new List<Type>();
            if (context.arguments() != null)
            {
                foreach (var argumentContext in context.arguments().expression())
                {
                    argumentTypes.Add(Visit(argumentContext));
                }
            }

            if (!IsArgumentTypesCompatible(functionSignature, argumentTypes))
            {
                throw new TypeMismatchException($"Arguments do not match the parameter types of function {functionName}");
            }

            return functionSignature.ReturnType;
        }

        public override Type VisitExpression(EasyHueParser.ExpressionContext context)
        {
            if (context.INT() != null)
            {
                return Type.Int;
            }
            else if (context.STRING() != null)
            {
                return Type.String;
            }
            else if (context.ID() != null)
            {
                string variableName = context.ID().GetText();
                if (!variableTypes.ContainsKey(variableName))
                {
                    throw new TypeMismatchException($"Variable '{variableName}' is not defined");
                }
                return variableTypes[variableName];
            }
            else if (context.functionCall() != null)
            {
                return Visit(context.functionCall());
            }
            else if (context.expression().Length == 1)
            {
                return Visit(context.expression()[0]);
            }
            else if (context.binaryOperator() != null)
            {
                Type leftType = Visit(context.expression()[0]);
                Type rightType = Visit(context.expression()[1]);

                string operatorText = context.binaryOperator().GetText();

                if (operatorText == "+" || operatorText == "-" || operatorText == "*" || operatorText == "/")
                {
                    if (leftType != Type.Int || rightType != Type.Int)
                    {
                        throw new TypeMismatchException($"Binary arithmetic operation requires both operands to be of type int");
                    }
                    return Type.Int;
                }
                else if (operatorText == "==" || operatorText == "!=" || operatorText == "<" || operatorText == ">" ||
                         operatorText == "<=" || operatorText == ">=")
                {
                    if (leftType != Type.Int || rightType != Type.Int)
                    {
                        throw new TypeMismatchException($"Binary comparison operation requires both operands to be of type int");
                    }
                    return Type.Boolean;
                }
            }

            throw new TypeMismatchException($"Invalid expression");
        }

      

        private FunctionSignature GetCurrentFunction()
        {
            // Assuming you have access to the current function's name
            // Retrieve the current function's name from a field or parameter
            string currentFunctionName = ""; // Set the current function's name here

            if (functionSignatures.ContainsKey(currentFunctionName))
            {
                return functionSignatures[currentFunctionName];
            }

            throw new InvalidOperationException($"Function {currentFunctionName} is not defined");
        }

        private bool IsAssignable(Type variableType, Type expressionType)
        {
            if (variableType == expressionType)
            {
                return true; // Exact type match
            }
            else if (variableType == Type.String && expressionType == Type.Int)
            {
                return true; // Implicit conversion from int to string
            }

            return false; // Types are not compatible
        }

        private bool IsArgumentTypesCompatible(FunctionSignature functionSignature, List<Type> argumentTypes)
        {
            if (functionSignature.ParameterTypes.Count != argumentTypes.Count)
            {
                return false;
            }

            for (int i = 0; i < functionSignature.ParameterTypes.Count; i++)
            {
                Type parameterType = functionSignature.ParameterTypes[i];
                Type argumentType = argumentTypes[i];

                if (!IsAssignable(parameterType, argumentType))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(string message) : base(message) { }
    }

    public class DuplicateFunctionDefinitionException : Exception
    {
        public DuplicateFunctionDefinitionException(string message) : base(message) { }
    }

    public class FunctionSignature
    {
        public List<Type> ParameterTypes { get; set; }
        public Type ReturnType { get; set; }

        public FunctionSignature(List<Type> parameterTypes, Type returnType)
        {
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }
    }

    public class Type
    {
        public static readonly Type Int = new Type("int");
        public static readonly Type String = new Type("string");
        public static readonly Type Boolean = new Type("boolean");

        public string TypeName { get; }

        public Type(string typeName)
        {
            TypeName = typeName;
        }

        public override bool Equals(object obj)
        {
            if (obj is Type otherType)
            {
                return TypeName == otherType.TypeName;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }
    }
}




