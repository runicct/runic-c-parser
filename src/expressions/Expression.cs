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
                public Expression Pop(Token? token)
                {
                    if (_expressions.Count == 0)
                    {
                        return new Constant(null);
                    }
                    return _expressions.Pop();
                }
                public Expression? Peek()
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
                Type? _type;
                public Type? Type { get { return _type; } }
                Token? _token;
                public Token? Token { get { return _token; } }
                OperatorFlag _flag;
                public OperatorFlag Flag { get { return _flag; } }
                public Operator(Token? token, OperatorFlag flag)
                {
                    _token = token;
                    _flag = flag;
                }
                public Operator(Token? token, Type? type)
                {
                    _token = token;
                    _type = type;
                    _flag = OperatorFlag.Cast;
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
                public (Token?, OperatorFlag) Pop()
                {
                    if (_tokens.Count == 0)
                    {
                        return (null, OperatorFlag.None);
                    }
                    return (_tokens.Pop(), _flags.Pop());
                }
                public (Token?, OperatorFlag) Peek()
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
                VariableUse? leftVarible = left as VariableUse;
                if (leftVarible == null)
                {
                    Indexing? leftIndexing = left as Indexing;
                    if (leftIndexing == null)
                    {
                        Dereference? leftDereference = left as Dereference;
                        if (leftDereference == null)
                        {
                            context.Error_InvalidAssignmentTarget(token, left);
                            return right;
                        }
                        else
                        {
                            return new Assignment.DereferenceAssignment(token, leftDereference.DerefOp, leftDereference.Address, right);
                        }
                    }
                    else
                    {
                        return new Assignment.IndexingAssignment(token, leftIndexing.Target, leftIndexing.Index, right);
                    }
                }
                else
                {
                    MemberUse? leftField = leftVarible as MemberUse;
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
                VariableUse? variableUse = target as VariableUse;
                if (variableUse == null)
                {
                    Indexing? indexing = target as Indexing;
                    if (indexing == null)
                    {
                        Dereference? dereference = target as Dereference;
                        if (dereference == null)
                        {
                            context.Error_InvalidAssignmentTarget(token, target);
                            return target;
                        }
                        else
                        {
                            if (postfix)
                            {
                                if (decrement) { return new Decrement.Postfix.Dereference(token, dereference.DerefOp, dereference.Address); }
                                else { return new Increment.Postfix.Dereference(token, dereference.Address); }
                            }
                            else
                            {
                                if (decrement) { return new Decrement.Prefix.Dereference(token, dereference.DerefOp, dereference.Address); }
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
                    MemberUse? memberUse = variableUse as MemberUse;
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
                Token? token = Operator.Token;
                if (Operator.Flag == OperatorFlag.Cast)
                {
                    Type.StructOrUnion structOrUnion = Operator.Type as Type.StructOrUnion;
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
                    return new Cast(token, Operator.Type, value);
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
                            VariableUse? leftVarible = left as VariableUse;
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
            static bool IsOperator(Token? Token)
            {
                if (Token == null) { return false; }
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
                return GetPrecendence(op.Token == null ? null : op.Token.Value, op.Flag);
            }
            static int GetPrecendence(string? op, OperatorFlag flags)
            {
                if (op == null) { return int.MaxValue; }

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
            internal static Expression? ParseFunctionCall(Function Function, IScope ParentScope, Parser Context, TokenQueue TokenQueue)
            {
                Token? leftParenthesis;
                Token? rightParenthesis = null;
                Token? token = TokenQueue.ReadNextToken();
                if (token == null) { return null; }
                if (token.Value != "(")
                {
                    Context.Error_InvalidFunctionCall(Function, token);
                    return null;
                }
                leftParenthesis = token;
                int parameterIndex = 0;
                List<Expression> parameters = new List<Expression>();
                Expression? parameter = null;
                token = TokenQueue.PeekToken();
                if (token == null || token.Value == ")")
                {
                    TokenQueue.ReadNextToken();
                    return new Call(Function, leftParenthesis, new FunctionParameter[] { }, token);
                }
                while (true)
                {
                    parameter = Parse(ParentScope, Context, TokenQueue);
                    if (parameter == null)
                    {
                        return null;
                    }
                    if (Function != null && parameterIndex < Function.FunctionParameters.Length && Function.FunctionParameters[parameterIndex].Type is Type.StructOrUnion)
                    {
                        Type.StructOrUnion structType = Function.FunctionParameters[parameterIndex].Type as Type.StructOrUnion;
                        if (structType != null)
                        {
                            /* That is a Runic C Specific extension most C Compiler want a cast */
                            switch (parameter)
                            {
                                case CompoundLiteralsFields compoundLiteralsFields:
                                    if (!Context.AllowCompoundLiteralsPassedToFunctionWithoutCast)
                                    {
                                        Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(token, Function);
                                    }
                                    parameters.Add(CompoundLiteralsStruct.Create(Context, compoundLiteralsFields, structType));
                                    break;
                                case CompoundLiteralsList compoundLiteralsList:
                                    if (!Context.AllowCompoundLiteralsPassedToFunctionWithoutCast)
                                    {
                                        Context.Error_CompoundLiteralsPassedToFunctionWithoutCast(token, Function);
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

                    Context.Error_InvalidFunctionCall(Function, token);
                    while (token != null && token.Value != ")" && token.Value != ";" && token.Value != "}" && token.Value != "{")
                    {
                        token = TokenQueue.ReadNextToken();
                    }
                    break;
                }
                return new Call(Function, leftParenthesis, parameters.ToArray(), rightParenthesis);
            }
            static Expression.Constant ParseStringConstant(Parser Context, Token firstToken, TokenQueue tokenQueue)
            {
                Token? token = firstToken;
                List<Token> stringLiteral = new List<Token>();
                stringLiteral.Add(token);
                while (true)
                {
                    Token? next = tokenQueue.PeekToken();
                    if (next == null)
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
            static Type.StructOrUnion? ResolveType(Type.StructOrUnion? structOrUnion)
            {
                if (structOrUnion == null) { return null; }
                while (structOrUnion.Declaration != null && structOrUnion.Declaration != structOrUnion)
                {
                    structOrUnion = structOrUnion.Declaration;
                }
                if (structOrUnion.Declaration == null && structOrUnion.Name != null && structOrUnion.ParentScope != null)
                {
                    IScope? scope = structOrUnion.ParentScope;
                    Type? type = scope.ResolveType(structOrUnion.Name.Value);
                    if (type == null) { return null; }
                    return type as Type.StructOrUnion;
                }
                return null;
            }
            internal static Expression? Parse(IScope ParentScope, Parser Context, TokenQueue TokenQueue, bool StopOnComma = true)
            {
                ExpressionStack expressions = new ExpressionStack(Context);
                Stack<Operator> operators = new Stack<Operator>();
                Token? token = TokenQueue.ReadNextToken();
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
                        Expression? topOfStack = expressions.Peek();
                        if (topOfStack != null)
                        {
                            Type? expressionType = topOfStack.Type;
                            if (expressionType != null && expressionType is Type.FunctionPointerType) { flag |= OperatorFlag.MaybeFunction; }
                        }
                    }
                    else if (token.Value == "{")
                    {
                        Token? opToken = token;
                        flag &= ~OperatorFlag.MaybeFunction;
                        List<Expression> ordinalField = new List<Expression>();
                        Token? fieldOrValue = TokenQueue.ReadNextToken();
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
                                Token? fieldName = TokenQueue.ReadNextToken();
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
                                Token? assignmentOperator = TokenQueue.ReadNextToken();
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

                                Token? endOrComma = TokenQueue.ReadNextToken();
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
                                Token? nextFieldPeriod = TokenQueue.ReadNextToken();
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
                                Token? checkForMixedType = TokenQueue.PeekToken();
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
                                    Token? skipUntilAssignment = TokenQueue.ReadNextToken();
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
                                Expression? expression = Expression.Parse(ParentScope, Context, TokenQueue);
                                if (expression != null) { fields.Add(expression); }
                                endOrCommaParsing:;
                                Token? endOrComma = TokenQueue.ReadNextToken();
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
                        Expression? index = Parse(ParentScope, Context, TokenQueue);
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
                            Token? leftParenthesis = token;
                            flag &= ~OperatorFlag.Postfix;
                            Token? maybeTypeToken = TokenQueue.ReadNextToken();
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
                                Variable? variable = ParentScope.ResolveVariable(maybeTypeToken.Value);
                                if (variable != null)
                                {
                                    operators.Push(new Operator(token, OperatorFlag.None));
                                    expressions.Push(new VariableUse(variable));
                                }
                                else
                                {
                                    Type.Enum.Member? enumMember = ParentScope.ResolveEnumMember(maybeTypeToken.Value);
                                    if (enumMember != null)
                                    {
                                        operators.Push(new Operator(token, OperatorFlag.None));
                                        expressions.Push(new EnumMemberUse(enumMember));
                                    }
                                    else
                                    {
                                        Function? function = ParentScope.ResolveFunction(token.Value);
                                        if (function != null)
                                        {
                                            Token? nextToken = TokenQueue.PeekToken();
                                            if (nextToken != null && nextToken.Value == "(")
                                            {
                                                // We have a function call they need to be processed differently
                                                Expression? functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
                                                if (functionCall == null) { expressions.Push(new Expression.Constant(new Token[] { })); }
                                                else { expressions.Push(functionCall); }
                                            }
                                            else { expressions.Push(new FunctionReference(token, function)); }
                                        }
                                        else
                                        {
                                            Type? type = ParentScope.ResolveType(maybeTypeToken.Value);
                                            if (type != null)
                                            {
                                                Token? next = TokenQueue.ReadNextToken();
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
                                                operators.Push(new Operator(maybeTypeToken, type));
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
                                        Expression? function = expressions.Pop(maybeTypeToken);
                                        if (function == null)
                                        {
                                            // This should not happend
                                            expressions.Push(new Constant(new Token[] { maybeTypeToken }));
                                        }
                                        else
                                        {
                                            expressions.Push(new IndirectCall(function, leftParenthesis, new Expression[] { }, maybeTypeToken));
                                            Type? functionType = function.Type;
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
                                Type? type = Type.Parse(ParentScope, Context, maybeTypeToken, TokenQueue);
                                if (type != null)
                                {
                                    Token? next = TokenQueue.ReadNextToken();
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
                                    operators.Push(new Operator(maybeTypeToken, type));
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
                                Token? sizeOfKeyword = token;
                                Token? next = TokenQueue.ReadNextToken();
                                Token? leftParenthesis = null;
                                Token? rightParenthesis = null;

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

                                Type? type = Type.Parse(ParentScope, Context, next, TokenQueue);
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
                                    Expression? expression = Expression.Parse(ParentScope, Context, TokenQueue, false);
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
                                Token? topOperator = null;

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
                                        MemberUse? memberUse = variableUse as MemberUse;
                                        if (memberUse != null)
                                        {
                                            Type? variableType = memberUse.Fields[memberUse.Fields.Length - 1].Type;
                                            Type.StructOrUnion? structOrUnionType = null;
                                            if (!dereference) { structOrUnionType = variableType as Type.StructOrUnion; }
                                            else
                                            {
                                                Type.Pointer? pointerToStructOrUnion = variableType as Type.Pointer;
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
                                                Type.StructOrUnion? structOrUnionTypeDefinition = ResolveType(structOrUnionType);
                                                if (structOrUnionTypeDefinition == null)
                                                {
                                                    Context.Error_UseOfIncompleteTypeInFieldAccess(token, structOrUnionType);
                                                    expressions.Push(new Constant(new Token[] { }));
                                                }
                                                else
                                                {
                                                    Field? field = structOrUnionTypeDefinition.ResolveField(token.Value);
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
                                            Type.StructOrUnion? structOrUnionType = null;
                                            if (!dereference) { structOrUnionType = variableType as Type.StructOrUnion; }
                                            else
                                            {
                                                Type.Pointer? pointerToStructOrUnion = variableType as Type.Pointer;
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
                                                Type.StructOrUnion? structOrUnionTypeDefinition = ResolveType(structOrUnionType);
                                                if (structOrUnionTypeDefinition == null)
                                                {
                                                    Context.Error_UseOfIncompleteTypeInFieldAccess(token, structOrUnionType);
                                                    expressions.Push(new Constant(new Token[] { }));
                                                }
                                                else
                                                {
                                                    Field? field = structOrUnionTypeDefinition.ResolveField(token.Value);
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
                                    Variable? variable = ParentScope.ResolveVariable(token.Value);
                                    if (variable != null)
                                    {
                                        expressions.Push(new VariableUse(variable));
                                    }
                                    else
                                    {
                                        Type.Enum.Member? enumMember = ParentScope.ResolveEnumMember(token.Value);
                                        if (enumMember != null) { expressions.Push(new EnumMemberUse(enumMember)); }
                                        else
                                        {
                                            Function? function = ParentScope.ResolveFunction(token.Value);
                                            if (function != null)
                                            {
                                                Token? nextToken = TokenQueue.PeekToken();
                                                if (nextToken != null && nextToken.Value == "(")
                                                {
                                                    // We have a function call they need to be processed differently
                                                    Expression? functionCall = ParseFunctionCall(function, ParentScope, Context, TokenQueue);
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

            //public abstract Token FirstToken { get; }
            public virtual Type? Type { get { return null; } }
        }
    }
}
