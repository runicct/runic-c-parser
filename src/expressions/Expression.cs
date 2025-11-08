/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Expression : Statement
        {
            class ExpressionStack
            {
                Parser _context;
                Stack<Expression> _expressions = new Stack<Expression>();
                public ExpressionStack(Parser context)
                {
                    _context = context;
                    _expressions = new Stack<Expression>();
                }

                public void Push(Expression expression) { _expressions.Push(expression); }
#if NET6_0_OR_GREATER
                public Expression Pop(Token? token)
#else
                public Expression Pop(Token token)
#endif
                {
                    if (_expressions.Count == 0)
                    {
                        return new Constant(null);
                    }
                    return _expressions.Pop();
                }
#if NET6_0_OR_GREATER
                public Expression? Peek()
#else
                public Expression Peek()
#endif
                {
                    if (_expressions.Count == 0)
                    {
                        return null;
                    }
                    return _expressions.Peek();
                }
            }
            enum OperatorFlag
            {
                None = 0x0,
                Unary = 0x1,
                Cast = 0x2,
                Postfix = 0x4,
                MaybeFunction = 0x8,
            }
            class Operator
            {

                Token _token;
                public Token Token { get { return _token; } }
                OperatorFlag _flag;
                public OperatorFlag Flag { get { return _flag; } }
                public Operator(Token token, OperatorFlag flag)
                {
                    _token = token;
                    _flag = flag;
                }
            }
            class CastOperator : Operator
            {
                Token _leftParenthesis;
                public Token LeftParenthesis { get { return _leftParenthesis; } }
                Token _rightParenthesis;
                public Token RightParenthesis { get { return _rightParenthesis; } }
#if NET6_0_OR_GREATER
                Type? _type;
                public Type? Type { get { return _type; } }
#else
                Type _type;
                public Type Type { get { return _type; } }
#endif
#if NET6_0_OR_GREATER
                public CastOperator(Token leftParenthesis, Type? type, Token rightParenthesis) : base(leftParenthesis, OperatorFlag.Cast)
#else
                public CastOperator(Token leftParenthesis, Type type, Token rightParenthesis) : base(leftParenthesis, OperatorFlag.Cast)
#endif
                {
                    _type = type;
                    _leftParenthesis = leftParenthesis;
                    _rightParenthesis = rightParenthesis;
                }
            }
                class OperatorStackStack
            {
                Parser _context;
                Stack<Token> _tokens = new Stack<Token>();
                Stack<OperatorFlag> _flags = new Stack<OperatorFlag>();
                public OperatorStackStack(Parser context)
                {
                    _context = context;
                    _tokens = new Stack<Token>();
                    _flags = new Stack<OperatorFlag>();
                }

                public void Push(Token token, OperatorFlag flag) { _tokens.Push(token); _flags.Push(flag); }
#if NET6_0_OR_GREATER
                public (Token?, OperatorFlag) Pop()
#else
                public (Token, OperatorFlag) Pop()
#endif
                {
                    if (_tokens.Count == 0)
                    {
                        return (null, OperatorFlag.None);
                    }
                    return (_tokens.Pop(), _flags.Pop());
                }
#if NET6_0_OR_GREATER
                public (Token?, OperatorFlag) Peek()
#else
                public (Token, OperatorFlag) Peek()
#endif
                {
                    if (_tokens.Count == 0)
                    {
                        return (null, OperatorFlag.None);
                    }
                    return (_tokens.Peek(), _flags.Peek());
                }
                public bool IsEmpty { get { return _tokens.Count == 0; } }
            }
            static Expression ProcessAssignment(Parser context, Token token, Expression left, Expression right)
            {
#if NET6_0_OR_GREATER
                VariableUse? leftVarible = left as VariableUse;
#else
                VariableUse leftVarible = left as VariableUse;
#endif
                if (leftVarible == null)
                {
#if NET6_0_OR_GREATER
                    Indexing? leftIndexing = left as Indexing;
#else
                    Indexing leftIndexing = left as Indexing;
#endif
                    if (leftIndexing == null)
                    {
#if NET6_0_OR_GREATER
                        Dereference? leftDereference = left as Dereference;
#else
                        Dereference leftDereference = left as Dereference;
#endif
                        if (leftDereference == null)
                        {
                            context.Error_InvalidAssignmentTarget(token, left);
                            return right;
                        }
                        else
                        {
                            return new Assignment.DereferenceAssignment(token, leftDereference.Operator, leftDereference.Address, right);
                        }
                    }
                    else
                    {
                        return new Assignment.IndexingAssignment(token, leftIndexing.Target, leftIndexing.Index, right);
                    }
                }
                else
                {
#if NET6_0_OR_GREATER
                    MemberUse? leftField = leftVarible as MemberUse;
#else
                    MemberUse leftField = leftVarible as MemberUse;
#endif
                    if (leftField != null)
                    {
                        return new Assignment.MemberAssignment(token, leftField.Variable, leftField.Fields, right);
                    }
                    else
                    {
                        return new Assignment.VariableAssignment(token, leftVarible.Variable, right);
                    }
                }
            }
            static Expression ProcessIncrement(Parser context, Token token, bool postfix, bool decrement, Expression target)
            {
#if NET6_0_OR_GREATER
                VariableUse? variableUse = target as VariableUse;
#else
                VariableUse variableUse = target as VariableUse;
#endif
                if (variableUse == null)
                {
#if NET6_0_OR_GREATER
                    Indexing? indexing = target as Indexing;
#else
                    Indexing indexing = target as Indexing;
#endif
                    if (indexing == null)
                    {
#if NET6_0_OR_GREATER
                        Dereference? dereference = target as Dereference;
#else
                        Dereference dereference = target as Dereference;
#endif
                        if (dereference == null)
                        {
                            context.Error_InvalidAssignmentTarget(token, target);
                            return target;
                        }
                        else
                        {
                            if (postfix)
                            {
                                if (decrement) { return new Decrement.Postfix.Dereference(token, dereference.Operator, dereference.Address); }
                                else { return new Increment.Postfix.Dereference(token, dereference.Address); }
                            }
                            else
                            {
                                if (decrement) { return new Decrement.Prefix.Dereference(token, dereference.Operator, dereference.Address); }
                                else { return new Increment.Prefix.Dereference(token, dereference.Address); }
                            }
                        }
                    }
                    else
                    {
                        if (postfix)
                        {
                            if (decrement) { return new Decrement.Postfix.Indexing(token, indexing.Target, indexing.Index); }
                            else { return new Increment.Postfix.Indexing(token, indexing.Target, indexing.Index); }
                        }
                        else
                        {
                            if (decrement) { return new Decrement.Prefix.Indexing(token, indexing.Target, indexing.Index); }
                            else { return new Increment.Prefix.Indexing(token, indexing.Target, indexing.Index); }
                        }
                    }
                }
                else
                {
#if NET6_0_OR_GREATER
                    MemberUse? memberUse = variableUse as MemberUse;
#else
                    MemberUse memberUse = variableUse as MemberUse;
#endif
                    if (memberUse == null)
                    {
                        if (postfix)
                        {
                            if (decrement) { return new Decrement.Postfix.Variable(token, variableUse.Variable); }
                            else { return new Increment.Postfix.Variable(token, variableUse.Variable); }
                        }
                        else
                        {
                            if (decrement) { return new Decrement.Prefix.Variable(token, variableUse.Variable); }
                            else { return new Increment.Prefix.Variable(token, variableUse.Variable); }
                        }
                    }
                    else
                    {
                        if (postfix)
                        {
                            if (decrement) { return new Decrement.Postfix.Member(token, memberUse.Variable, memberUse.Fields); }
                            else { return new Increment.Postfix.Member(token, memberUse.Variable, memberUse.Fields); }
                        }
                        else
                        {
                            if (decrement) { return new Decrement.Prefix.Member(token, memberUse.Variable, memberUse.Fields); }
                            else { return new Increment.Prefix.Member(token, memberUse.Variable, memberUse.Fields); }
                        }
                    }
                }
            }
            static Expression ProcessOperator(Parser Context, Operator Operator, ExpressionStack Expressions)
            {
#if NET6_0_OR_GREATER
                Token? token = Operator.Token;
#else
                Token token = Operator.Token;
#endif
                if (Operator.Flag == OperatorFlag.Cast)
                {
                    CastOperator castOperator = Operator as CastOperator;
                    Type.StructOrUnion structOrUnion = castOperator.Type as Type.StructOrUnion;
                    Expression value = Expressions.Pop(token);
                    switch (value)
                    {
                        case Expression.CompoundLiteralsList literalsList:
                            if (structOrUnion != null) { return CompoundLiteralsStruct.Create(Context, literalsList, structOrUnion); }
                            break;
                        case Expression.CompoundLiteralsFields literalsFields:
                            if (structOrUnion != null) { return CompoundLiteralsStruct.Create(Context, literalsFields, structOrUnion); }
                            break;
                    }
                    return new Cast(castOperator.LeftParenthesis, castOperator.Type, castOperator.RightParenthesis, value);
                }
                switch (token.Value)
                {
                    case "(":
                        {
                            Context.Error_UnbalencedParenthesis(token);
                            return new Constant(new Token[] { new ImplicitToken(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, "0") });
                        }
                    case "!":
                        {
                            Expression value = Expressions.Pop(token);
                            return new Not(token, value);
                        }
                    case "+":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Add(token, left, right);
                        }
                    case "++":
                        {
                            Expression value = Expressions.Pop(token);
                            return ProcessIncrement(Context, token, (Operator.Flag & OperatorFlag.Postfix) != 0, false, value);
                        }
                    case "--":
                        {
                            Expression value = Expressions.Pop(token);
                            return ProcessIncrement(Context, token, (Operator.Flag & OperatorFlag.Postfix) != 0, true, value);
                        }
                    case "+=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Add(token, left, right));
                        }
                    case "-":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left;
                            if ((Operator.Flag & OperatorFlag.Unary) != 0)
                            {
                                left = new Constant(new Token[] { new ImplicitToken(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, "0") });
                            }
                            else
                            {
                                left = Expressions.Pop(token);
                            }
                            return new Sub(token, left, right);
                        }
                    case "-=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Sub(token, left, right));

                        }
                    case "*":
                        {
                            if ((Operator.Flag & OperatorFlag.Unary) != 0)
                            {
                                Expression address = Expressions.Pop(token);
                                return new Dereference(token, address);
                            }
                            else
                            {
                                Expression right = Expressions.Pop(token);
                                Expression left = Expressions.Pop(token);
                                return new Mul(token, left, right);
                            }
                        }
                    case "&":
                        {
                            if ((Operator.Flag & OperatorFlag.Unary) != 0)
                            {
                                Expression target = Expressions.Pop(token);
                                switch (target)
                                {
                                    case MemberUse memberUse: return new MemberReference(token, memberUse.Variable, memberUse.Fields);
                                    case VariableUse variableUse: return new VariableReference(token, variableUse.Variable);
                                    case Indexing indexing: return new IndexingReference(token, indexing.Target, indexing.Index); break;
                                    default:
                                        Context.Error_InvalidReferenceTarget(token, target);
                                        return target;
                                }
                            }
                            else
                            {
                                Expression right = Expressions.Pop(token);
                                Expression left = Expressions.Pop(token);
                                return new And(token, left, right);
                            }
                        }
                    case "&&":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new LogicalAnd(token, left, right);
                        }
                    case "||":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new LogicalOr(token, left, right);
                        }
                    case "*=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
#if NET6_0_OR_GREATER
                            VariableUse? leftVarible = left as VariableUse;
#else
                            VariableUse leftVarible = left as VariableUse;
#endif
                            return ProcessAssignment(Context, token, left, new Mul(token, left, right));
                        }
                    case "/":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Div(token, left, right);
                        }
                    case "/=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Div(token, left, right));
                        }
                    case "%":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Rem(token, left, right);
                        }
                    case "%=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Rem(token, left, right));
                        }
                    case "==":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.Equal, token, left, right);
                        }
                    case "!=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.NotEqual, token, left, right);
                        }
                    case ">":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.GreaterThan, token, left, right);
                        }
                    case "<":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.LowerThan, token, left, right);
                        }
                    case ">=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.GreaterOrEqual, token, left, right);
                        }
                    case "<=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comparison(Comparison.ComparisonOperation.LowerOrEqual, token, left, right);
                        }
                    case "|":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Or(token, left, right);
                        }
                    case "|=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Or(token, left, right));
                        }
                    case "^":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Xor(token, left, right);
                        }
                    case "^=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, new Xor(token, left, right));
                        }
                    case "=":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return ProcessAssignment(Context, token, left, right);
                        }
                    case ",":
                        {
                            Expression right = Expressions.Pop(token);
                            Expression left = Expressions.Pop(token);
                            return new Comma(token, left, right);
                        }
                }
                throw new Exception(token.File + " " + token.StartLine.ToString() + " " + token.Value);
                return null;
            }
            static bool IsOperator(Token Token)
            {
                switch (Token.Value)
                {
                    case "(":
                    case ")":
                    case "[":
                    case "]":
                    case "++":
                    case "--":
                    case ".":
                    case "->":
                    case "!":
                    case "~":
                    case "*":
                    case "/":
                    case "%":
                    case "+":
                    case "-":
                    case "<<":
                    case ">>":
                    case "<=":
                    case ">=":
                    case "<":
                    case ">":
                    case "==":
                    case "!=":
                    case "&":
                    case "^":
                    case "|":
                    case "&&":
                    case "||":
                    case "?":
                    case ":":
                    case "::":
                    case "=":
                    case "+=":
                    case "-=":
                    case "*=":
                    case "/=":
                    case "%=":
                    case "<<=":
                    case ">>=":
                    case "&=":
                    case "^=":
                    case "|=":
                    case ",":
                        return true;
                }
                return false;
            }
            static int GetPrecendence(Operator op)
            {
                return GetPrecendence(op.Token.Value, op.Flag);
            }
            static int GetPrecendence(string op, OperatorFlag flags)
            {
                if ((flags & OperatorFlag.Cast) != 0) { return 2; }
                if ((flags & OperatorFlag.Postfix) != 0)
                {
                    if (op == "++" || op == "--")
                    {
                        return 1;
                    }
                }
                if ((flags & OperatorFlag.Unary) != 0)
                {
                    if (op == "&" || op == "*")
                    {
                        return 2;
                    }
                }
                return OperatorPrecedence.GetPrecendence(op);
            }
#if NET6_0_OR_GREATER
            internal static Expression? ParseFunctionCall(Statement Function, IScope ParentScope, Parser Context, TokenQueue TokenQueue)
#else
            internal static Expression ParseFunctionCall(Statement Function, IScope ParentScope, Parser Context, TokenQueue TokenQueue)
#endif
            {
                FunctionParameter[] functionParameters = null;
                switch (Function)
                {
                    case Function func: functionParameters = func.FunctionParameters; break;
                    case Expression func:
                        Type.FunctionPointerType functionPointerType = func.Type as Type.FunctionPointerType;
                        if (functionPointerType != null) { functionParameters = functionPointerType.Parameters; }
                        else { functionParameters = new FunctionParameter[0]; }
                        break;
                }

#if NET6_0_OR_GREATER
                Token? leftParenthesis;
                Token? rightParenthesis = null;
                Token? token = TokenQueue.ReadNextToken();
#else
                Token leftParenthesis;
                Token rightParenthesis = null;
                Token token = TokenQueue.ReadNextToken();
#endif
                if (token == null) { return null; }
                if (token.Value != "(")
                {
                    switch (Function)
                    {
                        case Function func: Context.Error_InvalidFunctionCall(func, token); break;
                        case Expression func: Context.Error_InvalidFunctionCall(func, token); break;
                    }
                    return null;
                }
                leftParenthesis = token;
                int parameterIndex = 0;
                List<Expression> parameters = new List<Expression>();
#if NET6_0_OR_GREATER
                Expression? parameter = null;
#else
                Expression parameter = null;
#endif
                token = TokenQueue.PeekToken();
                if (token == null || token.Value == ")")
                {
                    TokenQueue.ReadNextToken();
                    switch (Function)
                    {
                        case Function func: return new Call(func, leftParenthesis, new FunctionParameter[] { }, token);
                        case Expression func: return new IndirectCall(func, leftParenthesis, new FunctionParameter[] { }, token);
                    }
                }
                while (true)
                {
                    parameter = Parse(ParentScope, Context, TokenQueue);
                    if (parameter == null)
                    {
                        return null;
                    }
                    if (Function != null && parameterIndex < functionParameters.Length && functionParameters[parameterIndex].Type is Type.StructOrUnion)
                    {
                        Type.StructOrUnion structType = functionParameters[parameterIndex].Type as Type.StructOrUnion;
                        if (structType != null)
                        {
                            /* That is a Runic C Specific extension most C Compiler want a cast */
                            switch (parameter)
                            {
                                case CompoundLiteralsFields compoundLiteralsFields:
                                    if (!Context.AllowCompoundLiteralsPassedToFunctionWithoutCast)
                                    {
                                        switch (Function)
                                        {
                                            case Function func: Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(func, token); break;
                                            case Expression func: Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(func, token); break;
                                        }
                                    }
                                    parameters.Add(CompoundLiteralsStruct.Create(Context, compoundLiteralsFields, structType));
                                    break;
                                case CompoundLiteralsList compoundLiteralsList:
                                    if (!Context.AllowCompoundLiteralsPassedToFunctionWithoutCast)
                                    {
                                        switch (Function)
                                        {
                                            case Function func: Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(func, token); break;
                                            case Expression func: Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(func, token); break;
                                        }
                                    }
                                    parameters.Add(CompoundLiteralsStruct.Create(Context, compoundLiteralsList, structType)); break;
                                default: parameters.Add(parameter); break;
                            }
                        }
                        else
                        {
                            parameters.Add(parameter);
                        }
                    }
                    else
                    {
                        parameters.Add(parameter);
                    }
                    parameterIndex++;
                    token = TokenQueue.ReadNextToken();
                    if (token == null)
                    {
                        Context.Error_IncompleteExpression(leftParenthesis);
                        break;
                    }
                    if (token.Value == ",")
                    {
                        continue;
                    }
                    if (token.Value == ")")
                    {
                        rightParenthesis = token;
                        break;
                    }

                    switch (Function)
                    {
                        case Function func: Context.Error_InvalidFunctionCall(func, token); break;
                        case Expression func: Context.Error_InvalidFunctionCall(func, token); break;
                    }
                    while (token != null && token.Value != ")" && token.Value != ";" && token.Value != "}" && token.Value != "{")
                    {
                        token = TokenQueue.ReadNextToken();
                    }
                    break;
                }
                switch (Function)
                {
                    case Function func: return new Call(func, leftParenthesis, parameters.ToArray(), rightParenthesis);
                    case Expression func: return new IndirectCall(func, leftParenthesis, parameters.ToArray(), rightParenthesis);
                }
                return null;
            }
            static Expression.Constant ParseStringConstant(Parser Context, Token firstToken, TokenQueue tokenQueue)
            {
#if NET6_0_OR_GREATER
                Token? token = firstToken;
#else
                Token token = firstToken;
#endif
                List<Token> stringLiteral = new List<Token>();
                stringLiteral.Add(token);
                while (true)
                {
#if NET6_0_OR_GREATER
                    Token? next = tokenQueue.PeekToken();
#else
                    Token next = tokenQueue.PeekToken();
#endif
                    if (next == null || next.Value == null)
                    {
                        Context.Error_IncompleteExpression(firstToken);
                        return new Expression.Constant(stringLiteral.ToArray());
                    }
                    if (next.Value.StartsWith("\""))
                    {
                        token = tokenQueue.ReadNextToken();
                        stringLiteral.Add(token);
                    }
                    else
                    {
                        break;
                    }
                }
                return new Expression.Constant(stringLiteral.ToArray());
            }
#if NET6_0_OR_GREATER
            static Type.StructOrUnion? ResolveType(Type.StructOrUnion? structOrUnion)
#else
            static Type.StructOrUnion ResolveType(Type.StructOrUnion structOrUnion)
#endif
            {
                if (structOrUnion == null) { return null; }
                while (structOrUnion.Declaration != null && structOrUnion.Declaration != structOrUnion)
                {
                    structOrUnion = structOrUnion.Declaration;
                }
                if (structOrUnion.Declaration == null && structOrUnion.Name != null && structOrUnion.ParentScope != null)
                {
#if NET6_0_OR_GREATER
                    IScope? scope = structOrUnion.ParentScope;
                    Type? type = scope.ResolveType(structOrUnion.Name.Value);
#else
                    IScope scope = structOrUnion.ParentScope;
                    Type type = scope.ResolveType(structOrUnion.Name.Value);
#endif
                    if (type == null) { return null; }
                    return type as Type.StructOrUnion;
                }
                return structOrUnion as Type.StructOrUnion.StructOrUnionDeclaration;
            }
#if NET6_0_OR_GREATER
            internal static Expression? Parse(IScope ParentScope, Parser Context, TokenQueue TokenQueue, bool StopOnComma = true)
#else
            internal static Expression Parse(IScope ParentScope, Parser Context, TokenQueue TokenQueue, bool StopOnComma = true)
#endif
            {
                ExpressionStack expressions = new ExpressionStack(Context);
                Stack<Operator> operators = new Stack<Operator>();
#if NET6_0_OR_GREATER
                Token? token = TokenQueue.ReadNextToken();
#else
                Token token = TokenQueue.ReadNextToken();
#endif
                if (token == null) { return null; }
                Token lastValidToken = token;
                OperatorFlag flag = OperatorFlag.Unary;
                while (true)
                {
                    if (token == null)
                    {
                        Context.Error_IncompleteExpression(lastValidToken);
                        goto done;
                    }
                    else if (token.Value == ";" || token.Value == "]" || token.Value == "}")
                    {
                        TokenQueue.FrontLoadToken(token);
                        break;
                    }
                    else if (token.Value == ",")
                    {
                        if (StopOnComma)
                        {
                            TokenQueue.FrontLoadToken(token);
                            break;
                        }

                        flag &= ~OperatorFlag.MaybeFunction;
                        int precedence = GetPrecendence(token.Value, flag);

                        while ((operators.Count > 0) && GetPrecendence(operators.Peek()) < precedence && operators.Peek().Token.Value != "(")
                        {
                            expressions.Push(ProcessOperator(Context, operators.Pop(), expressions));
                        }
                        operators.Push(new Operator(token, flag));
                        flag |= OperatorFlag.Unary;
                        flag &= ~OperatorFlag.Postfix;
                    }
                    else if (token.Value == ":")
                    {
                        // TODO check for ?
                        TokenQueue.FrontLoadToken(token);
                        break;
                    }
                    else if (token.Value.StartsWith("\""))
                    {
                        expressions.Push(ParseStringConstant(Context, token, TokenQueue));
                        flag &= ~OperatorFlag.Unary;
                    }
                    else if (token.Value == ")")
                    {
                        flag &= ~OperatorFlag.MaybeFunction;
                        while ((operators.Count > 0) && operators.Peek().Token.Value != "(")
                        {
                            expressions.Push(ProcessOperator(Context, operators.Pop(), expressions));
                        }
                        if (operators.Count == 0)
                        {
                            TokenQueue.FrontLoadToken(token);
                            break;
                        }
                        operators.Pop();
                        flag &= ~OperatorFlag.Unary;
                        flag &= ~OperatorFlag.Postfix;
#if NET6_0_OR_GREATER
                        Expression? topOfStack = expressions.Peek();
#else
                        Expression topOfStack = expressions.Peek();
#endif
                        if (topOfStack != null)
                        {
#if NET6_0_OR_GREATER
                            Type? expressionType = topOfStack.Type;
#else
                            Type expressionType = topOfStack.Type;
#endif
                            if (expressionType != null && expressionType is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                        }
                    }
                    else if (token.Value == "{")
                    {
#if NET6_0_OR_GREATER
                        Token? opToken = token;
#else
                        Token opToken = token;
#endif
                        flag &= ~OperatorFlag.MaybeFunction;
                        List<Expression> ordinalField = new List<Expression>();
#if NET6_0_OR_GREATER
                        Token? fieldOrValue = TokenQueue.ReadNextToken();
#else
                        Token fieldOrValue = TokenQueue.ReadNextToken();
#endif
                        if (fieldOrValue == null)
                        {
                            Context.Error_IncompleteExpression(lastValidToken);
                            goto done;
                        }
                        if (fieldOrValue.Value == "}") { }
                        else if (fieldOrValue.Value == ".")
                        {
                            Dictionary<string, Expression> fields = new Dictionary<string, Expression>();

                            bool usedCommaBefore = false;
                            if (Context.StandardRevision < CStandardRevision.C99)
                            {
                                Context.Warning_DesignatedInitializersBeforeC99(fieldOrValue);
                            }
                            while (true)
                            {
#if NET6_0_OR_GREATER
                                Token? fieldName = TokenQueue.ReadNextToken();
#else
                                Token fieldName = TokenQueue.ReadNextToken();
#endif
                                if (fieldName == null)
                                {
                                    Context.Error_IncompleteExpression(fieldOrValue);
                                    goto done;
                                }
                                if (!Identifier.IsValid(fieldName, Context.StandardRevision))
                                {
                                    Context.Error_InvalidIdentifier(fieldOrValue);
                                    fieldName = null;
                                }
#if NET6_0_OR_GREATER
                                Token? assignmentOperator = TokenQueue.ReadNextToken();
#else
                                Token assignmentOperator = TokenQueue.ReadNextToken();
#endif
                                if (assignmentOperator == null)
                                {
                                    Context.Error_IncompleteExpression(fieldName);
                                    goto done;
                                }

                                if (assignmentOperator.Value != "=")
                                {
                                    Context.Error_ExpectedAssignment(assignmentOperator);
                                    while (assignmentOperator != null)
                                    {
                                        if (assignmentOperator.Value == "}") { goto endOfCompoundLiteralsParsing; }
                                        if (assignmentOperator.Value == ";") { goto endOfCompoundLiteralsParsing; }
                                        if (assignmentOperator.Value == ",") { goto nextField; }
                                        if (assignmentOperator.Value == "=") { break; }
                                        assignmentOperator = TokenQueue.ReadNextToken();
                                    }
                                }
                                nextExpression:;
                                if (fieldName != null)
                                {
                                    if (fields.ContainsKey(fieldName.Value))
                                    {
                                        Context.Error_FieldInitializedMultipleTimeInCompoundLiterals(fieldName);
                                    }
                                    else
                                    {
                                        fields.Add(fieldName.Value, Expression.Parse(ParentScope, Context, TokenQueue));
                                    }
                                }

#if NET6_0_OR_GREATER
                                Token? endOrComma = TokenQueue.ReadNextToken();
#else
                                Token endOrComma = TokenQueue.ReadNextToken();
#endif
                                if (endOrComma == null)
                                {
                                    Context.Error_IncompleteExpression(fieldOrValue);
                                    goto done;
                                }
                                if (endOrComma.Value == "}") { break; }
                                if (endOrComma.Value == ";")
                                {
                                    Context.Error_ExpectedComma(endOrComma);
                                    // Error unexpected. Here to continue parsing we have two choices. Either assume the
                                    // user made the following mistake a = {0; 1; 2}; instead of a = {0, 1, 2}; or assume
                                    // they forgot the '}' like a = {0, 1, 2;.
                                    // We take a best guess and move forward based if they did it right before or not.
                                    if (usedCommaBefore) { break; }
                                    continue;
                                }
                                if (endOrComma.Value == "=")
                                {
                                    Context.Error_ExpectedIdentifier(endOrComma);
                                    goto nextExpression;
                                }
                                if (endOrComma.Value != ",")
                                {
                                    Context.Error_ExpectedComma(endOrComma);
                                }
                                usedCommaBefore = true;
                                lastValidToken = endOrComma;
                                nextField:;
#if NET6_0_OR_GREATER
                                Token? nextFieldPeriod = TokenQueue.ReadNextToken();
#else
                                Token nextFieldPeriod = TokenQueue.ReadNextToken();
#endif
                                if (nextFieldPeriod == null)
                                {
                                    Context.Error_IncompleteExpression(lastValidToken);
                                    return null;
                                }

                                if (nextFieldPeriod.Value == "}")
                                {
                                    Context.Error_ExpectedIdentifier(lastValidToken);
                                    break;
                                }
                                if (nextFieldPeriod.Value == "=")
                                {
                                    Context.Error_ExpectedIdentifier(lastValidToken);
                                    goto nextExpression;
                                }
                                if (nextFieldPeriod.Value == ",")
                                {
                                    Context.Error_ExpectedIdentifier(lastValidToken);
                                    goto nextField;
                                }
                                if (nextFieldPeriod.Value != ".")
                                {
                                    // Error
                                }
                                lastValidToken = nextFieldPeriod;
                            }
                            endOfCompoundLiteralsParsing:;
                            expressions.Push(new CompoundLiteralsFields(opToken, fields));
                        }
                        else
                        {
                            List<Expression> fields = new List<Expression>();
                            bool usedCommaBefore = false;
                            bool mixedTypeReported = false;
                            TokenQueue.FrontLoadToken(fieldOrValue);
                            while (true)
                            {
                                nextValue:;
#if NET6_0_OR_GREATER
                                Token? checkForMixedType = TokenQueue.PeekToken();
#else
                                Token checkForMixedType = TokenQueue.PeekToken();
#endif
                                if (checkForMixedType == null)
                                {
                                    Context.Error_IncompleteExpression(lastValidToken);
                                    goto done;
                                }
                                if (checkForMixedType.Value == ".")
                                {
                                    if (!mixedTypeReported)
                                    {
                                        // TODO Report the error
                                        mixedTypeReported = true;
                                    }
#if NET6_0_OR_GREATER
                                    Token? skipUntilAssignment = TokenQueue.ReadNextToken();
#else
                                    Token skipUntilAssignment = TokenQueue.ReadNextToken();
#endif
                                    while (true)
                                    {
                                        skipUntilAssignment = TokenQueue.ReadNextToken();
                                        if (skipUntilAssignment == null) { goto done; }
                                        if (skipUntilAssignment.Value == "=") { break; }
                                        if (skipUntilAssignment.Value == ",") { goto nextValue; }
                                        if (skipUntilAssignment.Value == "}") { goto endOfCompoundLiteralsParsing; }
                                        if (skipUntilAssignment.Value == ";") { goto endOfCompoundLiteralsParsing; }
                                    }
                                    // Ignore the expression
                                    Expression.Parse(ParentScope, Context, TokenQueue);
                                    goto endOrCommaParsing;
                                }
#if NET6_0_OR_GREATER
                                Expression? expression = Expression.Parse(ParentScope, Context, TokenQueue);
#else
                                Expression expression = Expression.Parse(ParentScope, Context, TokenQueue);
#endif
                                if (expression != null) { fields.Add(expression); }
                                endOrCommaParsing:;
#if NET6_0_OR_GREATER
                                Token? endOrComma = TokenQueue.ReadNextToken();
#else
                                Token endOrComma = TokenQueue.ReadNextToken();
#endif
                                if (endOrComma == null)
                                {
                                    Context.Error_IncompleteExpression(lastValidToken);
                                    goto done;
                                }
                                if (endOrComma.Value == "}") { break; }
                                if (endOrComma.Value == ";")
                                {
                                    Context.Error_ExpectedComma(endOrComma);
                                    // Error unexpected. Here to continue parsing we have two choices. Either assume the
                                    // user made the following mistake a = {0; 1; 2}; instead of a = {0, 1, 2}; or assume
                                    // they forgot the '}' like a = {0, 1, 2;.
                                    // We take a best guess and move forward based if they did it right before or not.
                                    if (usedCommaBefore) { break; }
                                    continue;
                                }
                                if (endOrComma.Value != ",")
                                {
                                    Context.Error_ExpectedComma(endOrComma);
                                    while (true)
                                    {
                                        endOrComma = TokenQueue.ReadNextToken();
                                        if (endOrComma == null) { goto endOfCompoundLiteralsParsing; }
                                        if (endOrComma.Value == ",") { break; }
                                        if (endOrComma.Value == "}" || endOrComma.Value == ";") { goto endOfCompoundLiteralsParsing; }
                                    }
                                }
                                usedCommaBefore = true;
                            }
                            endOfCompoundLiteralsParsing:;
                            expressions.Push(new CompoundLiteralsList(opToken, fields.ToArray()));
                        }
                    }
                    else if (token.Value == "[")
                    {
                        flag &= ~OperatorFlag.MaybeFunction;

                        Token bracket = token;
#if NET6_0_OR_GREATER
                        Expression? index = Parse(ParentScope, Context, TokenQueue);
#else
                        Expression index = Parse(ParentScope, Context, TokenQueue);
#endif
                        if (index == null)
                        {
                            Context.Error_InvalidIndex(token);
                        }
                        Expression target = expressions.Pop(token);
                        expressions.Push(new Indexing(token, target, index));
                        token = TokenQueue.ReadNextToken();
                        if (token == null || token.Value != "]")
                        {
                            Context.Error_UnbalencedBracket(bracket);
                            break;
                        }
                        flag &= ~OperatorFlag.Unary;
                        flag &= ~OperatorFlag.Postfix;
                    }
                    else
                    {
                        if (token.Value == "(")
                        {
#if NET6_0_OR_GREATER
                            Token? leftParenthesis = token;
                            Token? maybeTypeToken = TokenQueue.ReadNextToken();
#else
                            Token leftParenthesis = token;
                            Token maybeTypeToken = TokenQueue.ReadNextToken();
#endif
                            flag &= ~OperatorFlag.Postfix;
                            if (maybeTypeToken == null)
                            {
                                Context.Error_IncompleteExpression(token);
                                goto done;
                            }
                            if (maybeTypeToken.Value == "sizeof")
                            {

                            }
                            else if (Identifier.IsValid(maybeTypeToken, Context.StandardRevision))
                            {
#if NET6_0_OR_GREATER
                                Variable? variable = ParentScope.ResolveVariable(maybeTypeToken.Value);
#else
                                Variable variable = ParentScope.ResolveVariable(maybeTypeToken.Value);
#endif
                                if (variable != null)
                                {
                                    if ((flag & OperatorFlag.MaybeFunction) != 0)
                                    {
#if NET6_0_OR_GREATER
                                        Expression? function = expressions.Pop(maybeTypeToken);
#else
                                        Expression function = expressions.Pop(maybeTypeToken);
#endif
                                        if (function == null)
                                        {
                                            // This should not happend
                                            expressions.Push(new Constant(new Token[] { maybeTypeToken }));
                                        }
                                        else
                                        {
                                            TokenQueue.FrontLoadToken(maybeTypeToken);
                                            TokenQueue.FrontLoadToken(leftParenthesis);
                                            Expression result = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
                                            expressions.Push(result);
#if NET6_0_OR_GREATER
                                            Type? resultType = result.Type;
#else
                                            Type resultType = result.Type;
#endif
                                            if (resultType != null && resultType is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                                            else { flag &= ~OperatorFlag.MaybeFunction; }
                                        }
                                    }
                                    else
                                    {
                                        operators.Push(new Operator(token, OperatorFlag.None));
                                        expressions.Push(new VariableUse(variable));
                                    }
                                }
                                else
                                {
#if NET6_0_OR_GREATER
                                    Type.Enum.Member? enumMember = ParentScope.ResolveEnumMember(maybeTypeToken.Value);
#else
                                    Type.Enum.Member enumMember = ParentScope.ResolveEnumMember(maybeTypeToken.Value);
#endif
                                    if (enumMember != null)
                                    {
                                        operators.Push(new Operator(token, OperatorFlag.None));
                                        expressions.Push(new EnumMemberUse(enumMember));
                                    }
                                    else
                                    {
#if NET6_0_OR_GREATER
                                        Function? function = ParentScope.ResolveFunction(token.Value);
#else
                                        Function function = ParentScope.ResolveFunction(token.Value);
#endif
                                        if (function != null)
                                        {
#if NET6_0_OR_GREATER
                                            Token? nextToken = TokenQueue.PeekToken();
#else
                                            Token nextToken = TokenQueue.PeekToken();
#endif
                                            if (nextToken != null && nextToken.Value == "(")
                                            {
                                                // We have a function call they need to be processed differently
#if NET6_0_OR_GREATER
                                                Expression? functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
#else
                                                Expression functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
#endif
                                                if (functionCall == null) { expressions.Push(new Expression.Constant(new Token[] { })); }
                                                else { expressions.Push(functionCall); }
                                            }
                                            else { expressions.Push(new FunctionReference(token, function)); }
                                        }
                                        else
                                        {
#if NET6_0_OR_GREATER
                                            Type? type = ParentScope.ResolveType(maybeTypeToken.Value);
#else
                                            Type type = ParentScope.ResolveType(maybeTypeToken.Value);
#endif
                                            if (type != null)
                                            {
#if NET6_0_OR_GREATER
                                                Token? next = TokenQueue.ReadNextToken();
#else
                                                Token next = TokenQueue.ReadNextToken();
#endif
                                                if (next == null)
                                                {
                                                    Context.Error_IncompleteExpression(maybeTypeToken);
                                                    goto done;
                                                }
                                                while (next.Value == "*")
                                                {
                                                    type = type.MakePointer(next);
                                                    next = TokenQueue.ReadNextToken();
                                                    if (next == null)
                                                    {
                                                        Context.Error_IncompleteExpression(maybeTypeToken);
                                                        goto done;
                                                    }
                                                }
                                                if (next.Value != ")")
                                                {
                                                    Context.Error_InvalidCast(type, next);
                                                    break;
                                                }
                                                operators.Push(new CastOperator(leftParenthesis, type, next));
                                            }
                                        }
                                    }
                                }
                                flag &= ~OperatorFlag.Unary;
                            }
                            else if (IsOperator(maybeTypeToken))
                            {
                                if (maybeTypeToken.Value == ")")
                                {
                                    flag &= ~OperatorFlag.Unary;
                                    if ((flag & OperatorFlag.MaybeFunction) != 0)
                                    {
#if NET6_0_OR_GREATER
                                        Expression? function = expressions.Pop(maybeTypeToken);
#else
                                        Expression function = expressions.Pop(maybeTypeToken);
#endif
                                        if (function == null)
                                        {
                                            // This should not happend
                                            expressions.Push(new Constant(new Token[] { maybeTypeToken }));
                                        }
                                        else
                                        {
                                            expressions.Push(new IndirectCall(function, leftParenthesis, new Expression[] { }, maybeTypeToken));
#if NET6_0_OR_GREATER
                                            Type? functionType = function.Type;
#else
                                            Type functionType = function.Type;
#endif
                                            if (functionType != null && functionType is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                                            else { flag &= ~OperatorFlag.MaybeFunction; }
                                        }
                                    }
                                    else
                                    {
                                        // TODO: Throw an error
                                    }
                                }
                                else
                                {
                                    flag &= ~OperatorFlag.MaybeFunction;
                                    operators.Push(new Operator(token, OperatorFlag.None));
                                    operators.Push(new Operator(maybeTypeToken, flag | OperatorFlag.Unary));
                                    flag &= ~OperatorFlag.Unary;
                                }
                            }
                            else
                            {
#if NET6_0_OR_GREATER
                                Type? type = Type.Parse(ParentScope, Context, maybeTypeToken, TokenQueue);
#else
                                Type type = Type.Parse(ParentScope, Context, maybeTypeToken, TokenQueue);
#endif
                                if (type != null)
                                {
#if NET6_0_OR_GREATER
                                    Token? next = TokenQueue.ReadNextToken();
#else
                                    Token next = TokenQueue.ReadNextToken();
#endif
                                    if (next == null)
                                    {
                                        Context.Error_IncompleteExpression(maybeTypeToken);
                                        goto done;
                                    }
                                    if (next.Value != ")")
                                    {
                                        Context.Error_InvalidCast(type, next);
                                        break;
                                    }
                                    operators.Push(new CastOperator(leftParenthesis, type, next));
                                    flag &= ~OperatorFlag.Unary;
                                    if (type is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                                }
                                else
                                {
                                    flag &= ~OperatorFlag.MaybeFunction;
                                    operators.Push(new Operator(token, OperatorFlag.None));
                                    token = maybeTypeToken;
                                    if (token.Value.StartsWith("\""))
                                    {
                                        expressions.Push(ParseStringConstant(Context, token, TokenQueue));
                                    }
                                    else
                                    {
                                        expressions.Push(new Expression.Constant(new Token[] { token }));
                                    }
                                    flag &= ~OperatorFlag.Unary;
                                }
                            }
                        }
                        else if (IsOperator(token))
                        {
                            flag &= ~OperatorFlag.MaybeFunction;
                            int precedence = GetPrecendence(token.Value, flag);

                            while ((operators.Count > 0) && GetPrecendence(operators.Peek()) < precedence && operators.Peek().Token.Value != "(")
                            {
                                expressions.Push(ProcessOperator(Context, operators.Pop(), expressions));
                            }
                            operators.Push(new Operator(token, flag));
                            flag |= OperatorFlag.Unary;
                            flag &= ~OperatorFlag.Postfix;
                        }
                        else
                        {
                            if (token.Value == "sizeof")
                            {
#if NET6_0_OR_GREATER
                                Token? sizeOfKeyword = token;
                                Token? next = TokenQueue.ReadNextToken();
                                Token? leftParenthesis = null;
                                Token? rightParenthesis = null;
#else
                                Token sizeOfKeyword = token;
                                Token next = TokenQueue.ReadNextToken();
                                Token leftParenthesis = null;
                                Token rightParenthesis = null;
#endif
                                if (next == null)
                                {
                                    break;
                                }

                                if (next.Value != "(")
                                {
                                    Context.Error_ExpectedParenthesis(next);
                                    break;
                                }
                                leftParenthesis = next;
                                next = TokenQueue.ReadNextToken();
                                if (next == null)
                                {
                                    break;
                                }
#if NET6_0_OR_GREATER
                                Type? type = Type.Parse(ParentScope, Context, next, TokenQueue);
#else
                                Type type = Type.Parse(ParentScope, Context, next, TokenQueue);
#endif
                                if (type != null)
                                {
                                    rightParenthesis = TokenQueue.ReadNextToken();
                                    if (rightParenthesis == null || rightParenthesis.Value != ")")
                                    {
                                        if (rightParenthesis == null) { Context.Error_ExpectedParenthesis(next); }
                                        else { Context.Error_ExpectedParenthesis(rightParenthesis); }
                                    }
                                    expressions.Push(new SizeOf.SizeOfType(sizeOfKeyword, leftParenthesis, type, rightParenthesis));
                                }
                                else
                                {

                                    TokenQueue.FrontLoadToken(token);
#if NET6_0_OR_GREATER
                                    Expression? expression = Expression.Parse(ParentScope, Context, TokenQueue, false);
#else
                                    Expression expression = Expression.Parse(ParentScope, Context, TokenQueue, false);
#endif
                                    rightParenthesis = TokenQueue.ReadNextToken();
                                    if (rightParenthesis == null || next.Value != ")")
                                    {
                                        if (rightParenthesis == null) { Context.Error_ExpectedParenthesis(next); }
                                        else { Context.Error_ExpectedParenthesis(rightParenthesis); }
                                    }
                                    if (expression != null)
                                    {
                                        expressions.Push(new SizeOf.SizeOfExpression(sizeOfKeyword, leftParenthesis, expression, rightParenthesis));
                                    }
                                    else
                                    {
                                        expressions.Push(new Constant(new Token[] { }));
                                    }
                                }
                            }
                            else if (Identifier.IsValid(token, Context.StandardRevision))
                            {
#if NET6_0_OR_GREATER
                                Token? topOperator = null;
#else
                                Token topOperator = null;
#endif
                                if (operators.Count > 0) { topOperator = operators.Peek().Token; }

                                if (topOperator != null && (topOperator.Value == "." || topOperator.Value == "->"))
                                {
                                    bool dereference = (topOperator.Value == "->");
                                    operators.Pop();
                                    VariableUse variableUse = expressions.Pop(topOperator) as VariableUse;
                                    if (variableUse == null)
                                    {
                                        expressions.Push(new Constant(new Token[] { }));
                                    }
                                    else
                                    {
#if NET6_0_OR_GREATER
                                        MemberUse? memberUse = variableUse as MemberUse;
#else
                                        MemberUse memberUse = variableUse as MemberUse;
#endif
                                        if (memberUse != null)
                                        {
#if NET6_0_OR_GREATER
                                            Type? variableType = memberUse.Fields[memberUse.Fields.Length - 1].Type;
                                            Type.StructOrUnion? structOrUnionType = null;
#else
                                            Type variableType = memberUse.Fields[memberUse.Fields.Length - 1].Type;
                                            Type.StructOrUnion structOrUnionType = null;
#endif
                                            if (!dereference) { structOrUnionType = variableType as Type.StructOrUnion; }
                                            else
                                            {
#if NET6_0_OR_GREATER
                                                Type.Pointer? pointerToStructOrUnion = variableType as Type.Pointer;
#else
                                                Type.Pointer pointerToStructOrUnion = variableType as Type.Pointer;
#endif
                                                if (pointerToStructOrUnion != null)
                                                {
                                                    structOrUnionType = pointerToStructOrUnion.TargetType as Type.StructOrUnion;
                                                }
                                            }

                                            if (structOrUnionType == null)
                                            {
                                                Context.Error_InvalidMemberAccess(variableUse.Variable, topOperator, token);
                                                expressions.Push(variableUse);
                                            }
                                            else
                                            {
#if NET6_0_OR_GREATER
                                                Type.StructOrUnion? structOrUnionTypeDefinition = ResolveType(structOrUnionType);
#else
                                                Type.StructOrUnion structOrUnionTypeDefinition = ResolveType(structOrUnionType);
#endif
                                                if (structOrUnionTypeDefinition == null)
                                                {
                                                    Context.Error_UseOfIncompleteTypeInFieldAccess(token, structOrUnionType);
                                                    expressions.Push(new Constant(new Token[] { }));
                                                }
                                                else
                                                {
#if NET6_0_OR_GREATER
                                                    Field? field = structOrUnionTypeDefinition.ResolveField(token.Value);
#else
                                                    Field field = structOrUnionTypeDefinition.ResolveField(token.Value);
#endif
                                                    Field[] fieldChain = new Field[memberUse.Fields.Length + 1];
                                                    for (int n = 0; n < memberUse.Fields.Length; n++) { fieldChain[n] = memberUse.Fields[n]; }
                                                    if (field == null) { throw new Exception(); }
                                                    fieldChain[fieldChain.Length - 1] = field;
                                                    expressions.Push(new MemberUse(variableUse.Variable, fieldChain));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Type variableType = variableUse.Variable.Type;
#if NET6_0_OR_GREATER
                                            Type.StructOrUnion? structOrUnionType = null;
#else
                                            Type.StructOrUnion structOrUnionType = null;
#endif
                                            if (!dereference) { structOrUnionType = variableType as Type.StructOrUnion; }
                                            else
                                            {
#if NET6_0_OR_GREATER
                                                Type.Pointer? pointerToStructOrUnion = variableType as Type.Pointer;
#else
                                                Type.Pointer pointerToStructOrUnion = variableType as Type.Pointer;
#endif
                                                if (pointerToStructOrUnion != null)
                                                {
                                                    structOrUnionType = pointerToStructOrUnion.TargetType as Type.StructOrUnion;
                                                }
                                            }
                                            if (structOrUnionType == null)
                                            {
                                                Context.Error_InvalidMemberAccess(variableUse.Variable, topOperator, token);
                                                expressions.Push(variableUse);
                                            }
                                            else
                                            {
#if NET6_0_OR_GREATER
                                                Type.StructOrUnion? structOrUnionTypeDefinition = ResolveType(structOrUnionType);
#else
                                                Type.StructOrUnion structOrUnionTypeDefinition = ResolveType(structOrUnionType);
#endif
                                                if (structOrUnionTypeDefinition == null)
                                                {
                                                    Context.Error_UseOfIncompleteTypeInFieldAccess(token, structOrUnionType);
                                                    expressions.Push(new Constant(new Token[] { }));
                                                }
                                                else
                                                {
#if NET6_0_OR_GREATER
                                                    Field? field = structOrUnionTypeDefinition.ResolveField(token.Value);
#else
                                                    Field field = structOrUnionTypeDefinition.ResolveField(token.Value);
#endif
                                                    if (field == null)
                                                    {
                                                        Context.Error_FieldDoesNotExistInStruct(token, structOrUnionTypeDefinition);
                                                        expressions.Push(new Constant(new Token[] { }));
                                                    }
                                                    else
                                                    {
                                                        expressions.Push(new MemberUse(variableUse.Variable, new Field[] { field }));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
#if NET6_0_OR_GREATER
                                    Variable? variable = ParentScope.ResolveVariable(token.Value);
#else
                                    Variable variable = ParentScope.ResolveVariable(token.Value);
#endif
                                    if (variable != null)
                                    {
                                        expressions.Push(new VariableUse(variable));
                                        if (variable.Type is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                                    }
                                    else
                                    {
#if NET6_0_OR_GREATER
                                        Type.Enum.Member? enumMember = ParentScope.ResolveEnumMember(token.Value);
#else
                                        Type.Enum.Member enumMember = ParentScope.ResolveEnumMember(token.Value);
#endif
                                        if (enumMember != null) { expressions.Push(new EnumMemberUse(enumMember)); }
                                        else
                                        {
#if NET6_0_OR_GREATER
                                            Function? function = ParentScope.ResolveFunction(token.Value);
#else
                                            Function function = ParentScope.ResolveFunction(token.Value);
#endif
                                            if (function != null)
                                            {
#if NET6_0_OR_GREATER
                                                Token? nextToken = TokenQueue.PeekToken();
#else
                                                Token nextToken = TokenQueue.PeekToken();
#endif
                                                if (nextToken != null && nextToken.Value == "(")
                                                {
                                                    // We have a function call they need to be processed differently
#if NET6_0_OR_GREATER
                                                    Expression? functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
#else
                                                    Expression functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
#endif
                                                    if (functionCall == null) { expressions.Push(new Expression.Constant(new Token[] { })); }
                                                    else { expressions.Push(functionCall); }
                                                }
                                                else { expressions.Push(new FunctionReference(token, function)); }
                                            }
                                            else
                                            {
                                                // We have an error here lets try to report it as best as possible by attaching the
                                                // parser back to a next valid token
                                                Token identifier = token;
                                                token = TokenQueue.PeekToken();
                                                bool functionCall = false;
                                                while (token != null)
                                                {
                                                    if (token.Value == "." || token.Value == "->")
                                                    {
                                                        token = TokenQueue.ReadNextToken();
                                                        token = TokenQueue.PeekToken();
                                                        if (token == null) { break; }
                                                        if (IsOperator(token)) { break; }
                                                        if (Identifier.IsValid(token, Context.StandardRevision)) { continue; }
                                                    }
                                                    else if (token.Value == "(")
                                                    {
                                                        token = TokenQueue.ReadNextToken();
                                                        token = TokenQueue.ReadNextToken();
                                                        int parenthesis = 1;
                                                        while (token != null)
                                                        {
                                                            if (token.Value == ")") { parenthesis--; if (parenthesis == 0) { break; } }
                                                            else if (token.Value == "(") { parenthesis++; }
                                                            token = TokenQueue.ReadNextToken();
                                                        }
                                                        functionCall = true;
                                                    }
                                                    break;
                                                }
                                                if (functionCall)
                                                {
                                                    Context.Error_UndefinedFunction(identifier);
                                                }
                                                else
                                                {
                                                    Context.Error_UndefinedVariable(identifier);
                                                }
                                                expressions.Push(new Constant(new Token[] { }));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                expressions.Push(new Constant(new Token[] { token }));
                            }
                            flag |= OperatorFlag.Postfix;
                            flag &= ~OperatorFlag.Unary;
                        }
                    }


                    lastValidToken = token;
                    token = TokenQueue.ReadNextToken();
                }

                done:;
                while (operators.Count > 0)
                {
                    expressions.Push(ProcessOperator(Context, operators.Pop(), expressions));
                }
                return expressions.Pop(null);
            }
#if NET6_0_OR_GREATER
            internal virtual Type? Type { get { return null; } }
#else
            internal virtual Type Type { get { return null; } }
#endif
            }
        }
}
