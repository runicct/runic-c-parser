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

namespace Runic.C
{
    public partial class Parser : IStatementStream
    {
        public enum CStandardRevision
        {
            ANSI,
            C89,
            C90,
            C99,
            C11,
            C17,
            C23,
            CFuture
        }
        CStandardRevision _standardRevision = CStandardRevision.CFuture;
        public CStandardRevision StandardRevision
        {
            get { return _standardRevision; }
            set { _standardRevision = value; }
        }
        class InputFilter : ITokenStream
        {
            ITokenStream _tokenStream;
            public InputFilter(ITokenStream tokenStream)
            {
                _tokenStream = tokenStream;
            }

#if NET6_0_OR_GREATER
            public Token? ReadNextToken()
#else
            public Token ReadNextToken()
#endif
            {
#if NET6_0_OR_GREATER
                Token? token = _tokenStream.ReadNextToken();
#else
                Token token = _tokenStream.ReadNextToken();
#endif
                while (token != null)
                {
                    switch (token.Value)
                    {
                        case "\n":
                        case "\t":
                        case " ":
                            break;
                        default:
                            return token;
                    }
                    token = _tokenStream.ReadNextToken();
                }
                return token;
            }
        }
        TokenQueue _input;
        ulong _globalVariableIndex = 0;
        internal ulong GetNextGlobalVariableIndex()
        {
            lock (this)
            {
                return _globalVariableIndex++;
            }
        }
        RootScope _root;
        public Scope Root { get { return _root; } }
        public Parser(ITokenStream TokenStream)
        {
            _input = new TokenQueue(new InputFilter(TokenStream));
            _root = new RootScope(this);
            _scopes.Push(_root);
        }
        public virtual void Warning_DesignatedInitializersBeforeC99(Token Token) { }
        public virtual void Warning_ExtraFieldInStructInitialization(Expression Value) { }
        public virtual void Error_MissingTypedefName(Token TypedefToken) { }
        public virtual void Error_MissingTypedefType(Token TypedefToken) { }
        public virtual void Error_InvalidTypedefType(Token TypedefToken, Token TypeToken) { }
        public virtual void Error_AnonymousFunctionPointerTypeInTypedef(Token TypedefToken, Type.FunctionPointerType FunctionPointer) { }
        public virtual void Error_AnonymousFunctionPointerInStruct(Token StructToken, Type.FunctionPointerType FunctionPointer) { }
        public virtual void Error_MissingFieldName(Token Container, Type FieldType) { }
        public virtual void Error_IncompleteDeclaration(Token Container) { }
        public virtual void Error_IncompleteStatement(Token Keyword) { }
        public virtual void Error_IncompleteExpression(Token Token) { }
        public virtual void Error_InvalidIndex(Token Token) { }
        public virtual void Error_InvalidType(Token Token) { }
        public virtual void Error_InvalidEnumValue(Token Enum, Token Token) { }
        public virtual void Error_InvalidIfStatement(Token If, Token InvalidToken) { }
        public virtual void Error_InvalidFunctionPointerDeclaration(Token Token) { }
        public virtual void Error_InvalidIdentifier(Token Token) { }
        public virtual void Error_ExtraTokenAfterFieldDeclation(Token FieldName, Token ExtraToken) { }
        public virtual void Error_TypeWasAlreadyDeclared(Type PreviousDeclaration, Token NewTypeName, Type NewDeclaration) { }
        public virtual void Error_VariableWasAlreadyDeclared(Variable PreviousDeclaration, Variable NewDeclaration) { }
        public virtual void Error_InvalidFunctionDefintion(Token Function, Token Token) { }
        public virtual void Error_InvalidScope(Token Scope) { }
        public virtual void Error_ExpectedIdentifier(Token Token) { }
        public virtual void Error_ExpectedSemicolumn(Token Token) { }
        public virtual void Error_ExpectedParenthesis(Token Token) { }
        public virtual void Error_ExpectedExpression(Token Token) { }
        public virtual void Error_ExpectedAssignment(Token Token) { }
        public virtual void Error_ExpectedComma(Token Token) { }
        public virtual void Error_UnbalencedBracket(Token Token) { }
        public virtual void Error_UnbalencedParenthesis(Token Token) { }
        public virtual void Error_UnexpectedToken(Token Token) { }
        public virtual void Error_FieldDoesNotExistInStruct(Token Field, Type.StructOrUnion Struct) { }
        public virtual void Error_InvalidFunctionAttribute(Token AttributeToken, Token AttributeValue) { }
        public virtual void Error_InvalidFunctionParameterType(Token FunctionName, Token Type) { }
        public virtual void Error_InvalidAssignmentTarget(Token Assignment, Expression Target) { }
        public virtual void Error_InvalidFunctionCall(Function Function, Token InvalidToken) { }
        public virtual void Error_InvalidFunctionCall(Expression Function, Token InvalidToken) { }
        public virtual void Error_VariadicSpecifierMustEndParameterList(Token Function, Token InvalidToken) { }
        public virtual void Error_InvalidOperatorTarget(Token Operator, Expression Target) { }
        public virtual void Error_UndefinedVariable(Token Variable) { }
        public virtual void Error_UndefinedFunction(Token Variable) { }
        public virtual void Error_ExpectedCondition(Token Statement, Token InvalidToken) { }
        public virtual void Error_LabelUsedOutsideFunction(Token InvalidLabel) { }
        public virtual void Error_ExpectedWhile(Token Token) { }
        public virtual void Error_InvalidCast(Type Type, Token Token) { }
        public virtual void Error_InvalidArrayInitialization(Token Array, Expression Initialization) { }
        public virtual void Error_InvalidMemberAccess(Variable Variable, Token Operator, Token FieldName) { }
        public virtual void Error_InvalidStatementOutsideOfASwitchOrLoop(Token Token) { }
        public virtual void Error_InvalidReferenceTarget(Token Reference, Expression Target) { }
        public virtual void Error_FieldInitializedMultipleTimeInCompoundLiterals(Token FieldName) { }
        public virtual void Error_CompoundLiteralsPassedToFunctionWithoutCast(Function function, Token token) { }
        public virtual void Error_CompoundLiteralsPassedToFunctionWithoutCast(Expression function, Token token) { }
        public virtual void Error_UseOfIncompleteTypeInFieldAccess(Token token, Type incompleteType) { }
        public virtual void Error_UseOfIncompleteTypeInCompoundLiteral(Token token, Type incompleteType) { }
        public virtual void Error_CaseOutsideOfSwitch(Token token) { }

        bool _allowCompoundLiteralsPassedToFunctionWithoutCast = false;
        public bool AllowCompoundLiteralsPassedToFunctionWithoutCast
        {
            get { return _allowCompoundLiteralsPassedToFunctionWithoutCast; }
            set { _allowCompoundLiteralsPassedToFunctionWithoutCast = value; }
        }
        bool _allowMSVCExtensions = false;
        public bool AllowMSVCExtensions
        {
            get { return _allowMSVCExtensions; }
            set { _allowMSVCExtensions = value; }
        }

        // This function can be used to re-attach the parser after an error to
        // something that looks valid. This is best effort
#if NET6_0_OR_GREATER
        internal bool ReattachToken(Token? token)
#else
        internal bool ReattachToken(Token token)
#endif
        {
            if (token == null) { return true; }
            switch (token.Value)
            {
                case "auto":
                case "break":
                case "case":
                case "char":
                case "const":
                case "continue":
                case "default":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "extern":
                case "float":
                case "for":
                case "goto":
                case "if":
                case "int":
                case "long":
                case "register":
                case "return":
                case "short":
                case "signed":
                case "sizeof":
                case "static":
                case "struct":
                case "switch":
                case "typedef":
                case "union":
                case "unsigned":
                case "void":
                case "volatile":
                case "while":
                case ";":
                case "{":
                case "}":
                    return true;
            }
            return false;
        }
        Stack<Scope> _scopes = new Stack<Scope>();
        Stack<Scope.UnscopedForScope> _unscopedForScopes = new Stack<Scope.UnscopedForScope>();
        enum State
        {
            Normal,
            VariableDeclaration,
        }

        State _state = State.Normal;
#if NET6_0_OR_GREATER
        Type? _pendingType;
#else
        Type _pendingType;
#endif
        Expression[] ParseStaticArrayDim(Token staticArrayToken)
        {
#if NET6_0_OR_GREATER
            Token? token;
#else
            Token token;
#endif
            List<Expression> length = new List<Expression>();
            while (true)
            {
#if NET6_0_OR_GREATER
                Expression? expression = Expression.Parse(_scopes.Peek(), this, _input);
#else
                Expression expression = Expression.Parse(_scopes.Peek(), this, _input);
#endif

                if (expression == null)
                {
                    return new Expression[] { new Expression.Constant(new Token[] { }) };
                }



                token = _input.ReadNextToken();
                if (token == null)
                {
                    // __TODO__ ERROR
                    return new Expression[] { new Expression.Constant(new Token[] { }) };
                }

                length.Add(expression);


                if (token.Value == "]")
                {
                    token = _input.PeekToken();
                    if (token != null && token.Value == "[")
                    {
                        token = _input.ReadNextToken();
                        continue;
                    }
                    break;
                }
                // __TODO__ Error
                break;
            }
            return length.ToArray();
        }
#if NET6_0_OR_GREATER
        Statement? ParseDeclaration(Attribute[] attributes, Type type, Token token)
#else
        Statement ParseDeclaration(Attribute[] attributes, Type type, Token token)
#endif
        {
            Scope parentScope = _scopes.Peek();
#if NET6_0_OR_GREATER
            Function? parentFunction = parentScope.GetParentFunction();
#else
            Function parentFunction = parentScope.GetParentFunction();
#endif
            if (type != null)
            {
                switch (type)
                {
                    case Type.StructOrUnion structOrUnionType:
                        if (structOrUnionType.Name != null && (structOrUnionType.Declaration == structOrUnionType))
                        {
                            _scopes.Peek().AddType(structOrUnionType.Name, structOrUnionType);
                        }
                        break;
                    case Type.Enum @enum:
                        if (@enum.Name == null)
                        {
#if NET6_0_OR_GREATER
                            Type.Enum.EnumDeclation? enumDeclaration = @enum as Type.Enum.EnumDeclation;
#else
                            Type.Enum.EnumDeclation enumDeclaration = @enum as Type.Enum.EnumDeclation;
#endif
                            if (enumDeclaration != null)
                            {
                                _scopes.Peek().AddEnumValues(enumDeclaration);
                            }
                        }
                        else { _scopes.Peek().AddType(@enum.Name, @enum); }
                        break;
                    case Type.FunctionPointerType.FunctionReturningFunctionPointer functionReturningFunctionPointer:
                        if (functionReturningFunctionPointer.Name != null)
                        {
#if NET6_0_OR_GREATER
                            Token? declarationOrDefinition = _input.ReadNextToken();
#else
                            Token declarationOrDefinition = _input.ReadNextToken();
#endif
                            if (declarationOrDefinition == null)
                            {
                                Error_IncompleteDeclaration(token);
                                return null;
                            }
                            if (declarationOrDefinition.Value == ";")
                            {
                                return new FunctionDeclaration(attributes, functionReturningFunctionPointer.ReturnType, functionReturningFunctionPointer.Name, functionReturningFunctionPointer.Parameters, false);
                            }
                            else if (declarationOrDefinition.Value == "{")
                            {

                            }
                        }
                        break;
                }

                Token name;
#if NET6_0_OR_GREATER
                Token? variableNameOrSemicolumn = _input.ReadNextToken();
#else
                Token variableNameOrSemicolumn = _input.ReadNextToken();
#endif
                if (variableNameOrSemicolumn == null)
                {
                    Error_IncompleteDeclaration(token);
                    return null;
                }
                if (variableNameOrSemicolumn.Value == ";")
                {
                    switch (type)
                    {
                        case Type.StructOrUnion structOrUnionType: return null;
                        case Type.Enum.EnumDeclation enumDeclaration: return null;
                        default:
                            Error_ExpectedIdentifier(variableNameOrSemicolumn);
                            if (parentFunction != null)
                            {
                                return new VariableDeclaration(new LocalVariable(attributes, type, null, parentFunction, parentFunction.GetNextLocalIndex()), null, null);
                            }
                            else
                            {
                                return new VariableDeclaration(new GlobalVariable(attributes, type, null, GetNextGlobalVariableIndex()), null, null);
                            }

                    }
                }
                name = variableNameOrSemicolumn;
                if (!Identifier.IsValid(name, _standardRevision))
                {
                    Error_InvalidIdentifier(name);
                }

                // Can be '=' ',' ';' '[' or '('
                token = _input.ReadNextToken();
                if (token == null)
                {
                    Error_IncompleteDeclaration(name);
                    return null;
                }
#if NET6_0_OR_GREATER
                Token? staticArrayToken = null;
                Expression? arraySize = null;
#else
                Token staticArrayToken = null;
                Expression arraySize = null;
#endif
                bool isStaticArray = false;
                bool implicitStaticArraySize = false;
                if (token.Value == "[")
                {
                    isStaticArray = true;
                    staticArrayToken = token;
                    token = _input.PeekToken();
                    if (token != null && token.Value == "]")
                    {
                        _input.ReadNextToken();
                        implicitStaticArraySize = true;
                    }
                    else
                    {
#if NET6_0_OR_GREATER
                        arraySize = Expression.Parse(_scopes.Peek(), this, _input);
#else
                        arraySize = Expression.Parse(_scopes.Peek(), this, _input);
#endif
                        token = _input.ReadNextToken();
                        if (token == null)
                        {
                            Error_IncompleteDeclaration(name);
                            return null;
                        }
                        if (token.Value != "]")
                        {
                            // TODO Error
                        }
                    }
                    token = _input.ReadNextToken();
                    if (token == null)
                    {
                        Error_IncompleteDeclaration(name);
                        return null;
                    }
                }

                switch (token.Value)
                {
                    case "=":
                        {
                            Token equal = token;
#if NET6_0_OR_GREATER
                            Type.StructOrUnion? variableStructType = type as Type.StructOrUnion;
                            Expression? initialization = Expression.Parse(_scopes.Peek(), this, _input);
                            Expression.CompoundLiteralsList? compountLiteralsListInit = initialization as Expression.CompoundLiteralsList;
#else
                            Type.StructOrUnion variableStructType = type as Type.StructOrUnion;
                            Expression initialization = Expression.Parse(_scopes.Peek(), this, _input);
                            Expression.CompoundLiteralsList compountLiteralsListInit = initialization as Expression.CompoundLiteralsList;
#endif

                            if (isStaticArray)
                            {
                                if (compountLiteralsListInit != null)
                                {
                                    Type.StaticArray staticArrayType;
                                    if (implicitStaticArraySize)
                                    {
                                        staticArrayType = type.MakeStaticArray(null, new Expression[] { new Expression.Constant(new Token[] { new ImplicitToken(staticArrayToken.StartLine, staticArrayToken.StartColumn, staticArrayToken.EndLine, staticArrayToken.EndColumn, staticArrayToken.File, compountLiteralsListInit.Values.Length.ToString()) }) });
                                    }
                                    else
                                    {
                                        staticArrayType = type.MakeStaticArray(null, new Expression[] { arraySize });
                                    }
                                    type = staticArrayType;
                                    initialization = new Expression.ArrayInitializer(compountLiteralsListInit.Op, staticArrayType, compountLiteralsListInit.Values);
                                }
                                else
                                {
                                    Error_InvalidArrayInitialization(name, initialization);
                                }
                            }
                            else
                            {
                                if (compountLiteralsListInit != null)
                                {
                                    initialization = Expression.CompoundLiteralsStruct.Create(this, compountLiteralsListInit, variableStructType);
                                }
                            }

                            Variable variable;
                            if (parentFunction != null)
                            {
                                variable = new LocalVariable(attributes, type, name, parentFunction, parentFunction.GetNextLocalIndex());
                            }
                            else
                            {
                                variable = new GlobalVariable(attributes, type, name, GetNextGlobalVariableIndex());
                            }
                            _scopes.Peek().AddVariable(variable);

                            token = _input.ReadNextToken();
                            if (token == null)
                            {
                                Error_ExpectedSemicolumn(name);
                                return new VariableDeclaration(variable, equal, initialization);
                            }
                            if (token.Value == ",")
                            {
                                _pendingType = type;
                                _state = State.VariableDeclaration;
                                return new VariableDeclaration(variable, equal, initialization);
                            }
                            else if (token.Value != ";")
                            {
                                Error_ExpectedSemicolumn(token);
                                while (!ReattachToken(token)) { token = _input.ReadNextToken(); }
                            }
                            return new VariableDeclaration(variable, equal, initialization);
                        }
                    case ";":
                        {
                            Variable variable;
                            if (parentFunction != null)
                            {
                                variable = new LocalVariable(attributes, type, name, parentFunction, parentFunction.GetNextLocalIndex());
                            }
                            else
                            {
                                variable = new GlobalVariable(attributes, type, name, GetNextGlobalVariableIndex());
                            }
                            _scopes.Peek().AddVariable(variable);
                            return new VariableDeclaration(variable, null, null);
                        }
                    case ",":
                        {
                            Variable variable;
                            if (parentFunction != null)
                            {
                                variable = new LocalVariable(attributes, type, name, parentFunction, parentFunction.GetNextLocalIndex());
                            }
                            else
                            {
                                variable = new GlobalVariable(attributes, type, name, GetNextGlobalVariableIndex());
                            }
                            _scopes.Peek().AddVariable(variable);
                            _pendingType = type;
                            _state = State.VariableDeclaration;
                            return new VariableDeclaration(variable, null, null);
                        }
                    case "(":
                        {
                            bool variadic = false;
                            List<FunctionParameter> parameters = new List<FunctionParameter>();
                            ulong parameterIndex = 0;
                            token = _input.PeekToken();
                            if (token == null)
                            {
                                Error_IncompleteDeclaration(name);
                                return null;
                            }
                            if (token.Value == ")")
                            {
                                _input.ReadNextToken();
                            }
                            else
                            {
                                while (true)
                                {
                                    token = _input.ReadNextToken();
                                    if (token == null)
                                    {
                                        Error_IncompleteDeclaration(name);
                                        return null;
                                    }
                                    if (token.Value == "...")
                                    {
                                        variadic = true;
                                        token = _input.ReadNextToken();
                                        if (token == null)
                                        {
                                            Error_IncompleteDeclaration(name);
                                            return null;
                                        }
                                        if (token.Value == ";")
                                        {
                                            Error_ExpectedParenthesis(token);
                                            return new FunctionDeclaration(attributes, type, name, parameters.ToArray(), true);
                                        }
                                        if (token.Value == "{")
                                        {
                                            Error_ExpectedParenthesis(token);
                                            FunctionDefinition functionDefinition = new FunctionDefinition(_scopes.Peek(), this, attributes, type, name, parameters.ToArray(), true);
                                            _scopes.Peek().AddFunction(functionDefinition);
                                            _scopes.Push(functionDefinition.Body);
                                            return functionDefinition;
                                        }
                                        if (token.Value != ")")
                                        {
                                            Error_VariadicSpecifierMustEndParameterList(name, token);
                                            while (token.Value != ")")
                                            {
                                                token = _input.ReadNextToken();
                                                if (token == null || token.Value == ";")
                                                {
                                                    FunctionDeclaration functionDeclaration = new FunctionDeclaration(attributes, type, name, parameters.ToArray(), true);
                                                    _scopes.Peek().AddFunction(functionDeclaration);
                                                    return functionDeclaration;
                                                }
                                                if (token == null || token.Value == "{")
                                                {
                                                    FunctionDefinition functionDefinition = new FunctionDefinition(_scopes.Peek(), this, attributes, type, name, parameters.ToArray(), true);
                                                    _scopes.Peek().AddFunction(functionDefinition);
                                                    _scopes.Push(functionDefinition.Body);
                                                    return functionDefinition;
                                                }
                                            }
                                            break;
                                        }
                                    }
#if NET6_0_OR_GREATER
                                    Type? parameterType = Type.Parse(_scopes.Peek(), this, token, _input);
#else
                                    Type parameterType = Type.Parse(_scopes.Peek(), this, token, _input);
#endif
                                    if (parameterType == null)
                                    {
                                        parameterType = new Type.Int(token);
                                        Error_InvalidFunctionParameterType(name, token);
                                        token = _input.ReadNextToken();
                                        while (token != null && token.Value != "," && token.Value != ")") { token = _input.ReadNextToken(); }
                                        parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                                        if (token == null) { return null; }
                                        if (token.Value == ",") { continue; }
                                        break;
                                    }
                                    token = _input.ReadNextToken();
                                    if (token == null)
                                    {
                                        Error_IncompleteDeclaration(name);
                                        return null;
                                    }

                                    bool exceptAnonymousParameter = false;
                                    // If we hit a '[' We might have an anonymous parameter type array like char[X]
                                    if (token.Value == "[")
                                    {
#if NET6_0_OR_GREATER
                                        Expression[]? length = ParseStaticArrayDim(token);
#else
                                        Expression[] length = ParseStaticArrayDim(token);
#endif
                                        if (length == null)
                                        {
                                            Error_IncompleteDeclaration(name);
                                            return null;
                                        }
                                        parameterType.MakeStaticArray(token, length);
                                        exceptAnonymousParameter = true;
                                        token = _input.ReadNextToken();
                                        if (token == null)
                                        {
                                            Error_IncompleteDeclaration(name);
                                            return null;
                                        }
                                    }

                                    // If we hit the ',' or the ')' here we have an anonymous parameters
                                    if (token.Value == ",")
                                    {
                                        parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                                        parameterIndex++;
                                        continue;
                                    }
                                    if (token.Value == ")")
                                    {
                                        parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                                        break;
                                    }

                                    if (exceptAnonymousParameter)
                                    {
                                        // TODO Error
                                    }

                                    if (!Parser.Identifier.IsValid(token, _standardRevision))
                                    {
                                        Error_InvalidIdentifier(token);
                                    }
                                    Token parameterName = token;

                                    token = _input.ReadNextToken();
                                    if (token == null)
                                    {
                                        Error_IncompleteDeclaration(name);
                                        return null;
                                    }

                                    if (token.Value == "[")
                                    {
#if NET6_0_OR_GREATER
                                        Expression[]? length = ParseStaticArrayDim(token);
#else
                                        Expression[] length = ParseStaticArrayDim(token);
#endif
                                        if (length == null)
                                        {
                                            Error_IncompleteDeclaration(name);
                                            return null;
                                        }
                                        parameterType.MakeStaticArray(token, length);
                                        token = _input.ReadNextToken();
                                        if (token == null)
                                        {
                                            Error_IncompleteDeclaration(name);
                                            return null;
                                        }
                                    }

                                    parameters.Add(new FunctionParameter(new Attribute[0], parameterType, parameterName, parameterIndex));
                                    parameterIndex++;

                                    if (token.Value == ",") { continue; }
                                    if (token.Value == ")") { break; }

                                    Error_InvalidFunctionDefintion(name, token);
                                    {
                                        token = _input.ReadNextToken();
                                        while (token != null && token.Value != "," && token.Value != ")") { token = _input.ReadNextToken(); }
                                        parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                                        if (token == null) { return null; }
                                        if (token.Value == ",") { continue; }
                                        break;
                                    }
                                }
                            }

                            if (parameters.Count == 1 && parameters[0].Type is Type.Void)
                            {
                                // __TODO__ we might want to distinguish between () and (void)
                                parameters = new List<FunctionParameter>();
                            }

                            token = _input.ReadNextToken();
                            if (token == null)
                            {
                                Error_IncompleteDeclaration(name);
                                return null;
                            }
                            if (token.Value == ";")
                            {
                                FunctionDeclaration functionDeclaration = new FunctionDeclaration(attributes, type, name, parameters.ToArray(), variadic);
                                _scopes.Peek().AddFunction(functionDeclaration);
                                return functionDeclaration;
                            }
                            if (token.Value == "{")
                            {
                                FunctionDefinition functionDefinition = new FunctionDefinition(_scopes.Peek(), this, attributes, type, name, parameters.ToArray(), variadic);
                                _scopes.Peek().AddFunction(functionDefinition);
                                _scopes.Push(functionDefinition.Body);
                                return functionDefinition;
                            }
                            Error_InvalidFunctionDefintion(name, token);
                            {
                                token = _input.ReadNextToken();
                                while (token != null && token.Value != ";" && token.Value != "{") { token = _input.ReadNextToken(); }
                                if (token == null) { return null; }
                                if (token.Value == ";")
                                {
                                    FunctionDeclaration functionDeclaration = new FunctionDeclaration(attributes, type, name, parameters.ToArray(), variadic);
                                    _scopes.Peek().AddFunction(functionDeclaration);
                                    return functionDeclaration;
                                }
                                if (token.Value == "{")
                                {
                                    FunctionDefinition functionDefinition = new FunctionDefinition(_scopes.Peek(), this, attributes, type, name, parameters.ToArray(), variadic);
                                    _scopes.Peek().AddFunction(functionDefinition);
                                    _scopes.Push(functionDefinition.Body);
                                    return functionDefinition;
                                }
                                return null;
                            }
                        }
                }
            }
            return null;
        }
        Queue<Statement> _pendingStatements = new Queue<Statement>();
#if NET6_0_OR_GREATER
        public Statement? ReadNextStatementVariableDeclaration()
#else
        public Statement ReadNextStatementVariableDeclaration()
#endif
        {
            _pendingType = _pendingType.Strip();
            Scope parentScope = _scopes.Peek();
#if NET6_0_OR_GREATER
            Function? parentFunction = parentScope.GetParentFunction();
#else
            Function parentFunction = parentScope.GetParentFunction();
#endif

            _state = State.Normal;
            restart:;
#if NET6_0_OR_GREATER
            Token? token = _input.ReadNextToken();
#else
            Token token = _input.ReadNextToken();
#endif
            if (token == null)
            {
                return null;
            }

            if (token.Value == "*")
            {
                _pendingType = _pendingType.MakePointer(token);
                while (true)
                {
                    token = _input.ReadNextToken();
                    if (token == null)
                    {
                        Error_ExpectedSemicolumn(token);
                        return null;
                    }

                    if (token.Value == "const") { _pendingType = _pendingType.MakeConst(token); }
                    else if (token.Value == "*") { _pendingType = _pendingType.MakePointer(token); }
                    else { break; }
                }
            }

            Token name = token;

            if (!Identifier.IsValid(name, _standardRevision))
            {

            }
            token = _input.ReadNextToken();
            if (token == null)
            {
                Error_ExpectedSemicolumn(name);
                return null;
            }



            Variable variable;
            if (parentFunction == null)
            {
                variable = new GlobalVariable(new Attribute[0], _pendingType, name, GetNextGlobalVariableIndex());
            }
            else
            {
                variable = new LocalVariable(new Attribute[0], _pendingType, name, parentFunction, parentFunction.GetNextLocalIndex());
            }
            _scopes.Peek().AddVariable(variable);
            if (token.Value == ";")
            {
                return new VariableDeclaration(variable, null, null);
            }
            if (token.Value == ",")
            {
                _state = State.VariableDeclaration;
                return new VariableDeclaration(variable, null, null);
            }
            if (token.Value == "=")
            {
                Token equal = token;
#if NET6_0_OR_GREATER
                Type.StructOrUnion? variableStructType = variable.Type as Type.StructOrUnion;
                Expression? initialization = Expression.Parse(_scopes.Peek(), this, _input);
                Expression.CompoundLiteralsList? compoundLiteralsListInit = initialization as Expression.CompoundLiteralsList;
#else
                Type.StructOrUnion variableStructType = variable.Type as Type.StructOrUnion;
                Expression initialization = Expression.Parse(_scopes.Peek(), this, _input);
                Expression.CompoundLiteralsList compoundLiteralsListInit = initialization as Expression.CompoundLiteralsList;
#endif
                if (compoundLiteralsListInit != null)
                {
                    if (variableStructType != null) { initialization = Expression.CompoundLiteralsStruct.Create(this, compoundLiteralsListInit, variableStructType); }
                }
                token = _input.ReadNextToken();
                if (token == null)
                {
                    Error_ExpectedSemicolumn(name);
                    return null;
                }
                if (token.Value == ";")
                {
                    return new VariableDeclaration(variable, equal, initialization);
                }
                if (token.Value == ",")
                {
                    _state = State.VariableDeclaration;
                    return new VariableDeclaration(variable, null, null);
                }
            }


            return null;
        }
#if NET6_0_OR_GREATER
        public Statement? ReadNextStatementNormal()
#else
        public Statement ReadNextStatementNormal()
#endif
        {
            restart:;
#if NET6_0_OR_GREATER
            Token? token = _input.ReadNextToken();
#else
            Token token = _input.ReadNextToken();
#endif
            if (token == null) { return null; }
            switch (token.Value)
            {
                case "{":
                    {
                        if (_scopes.Count == 1)
                        {
                            Error_InvalidScope(token);
                        }
                        Scope subscope = new Scope(_scopes.Peek(), this);
                        _scopes.Push(subscope);
                        return new Scope.Enter(token, subscope);
                    }
                case "}":
                    {
                        if (_scopes.Count == 1)
                        {
                            Error_InvalidScope(token);
                            goto restart;
                        }

                        Scope subscope = _scopes.Pop();
                        switch (subscope)
                        {
                            case Scope.FunctionScope functionScope: return new Scope.ExitFunctionDefinition(token, functionScope, functionScope.FunctionDefinition);
                            case Scope.IfScope ifScope: return new Scope.ExitIf(token, ifScope, ifScope.If);
                            case Scope.ElseScope elseScope: return new Scope.ExitElse(token, elseScope, elseScope.Else);
                            case Scope.WhileScope whileScope: return new Scope.ExitWhile(token, whileScope, whileScope.While);
                            case Scope.ForScope forScope: return new Scope.ExitFor(token, forScope, forScope.For);
                            case Scope.SwitchScope switchScope: return new Scope.ExitSwitch(token, switchScope, switchScope.Switch);
                            case Scope.DoWhileScope doWhileScope:
                                {
#if NET6_0_OR_GREATER
                                    Scope.ExitDoWhile? exitDoWhile = DoWhile.ParseExitDoWhile(_scopes.Peek(), token, this, doWhileScope.DoWhile, _input);
#else
                                    Scope.ExitDoWhile exitDoWhile = DoWhile.ParseExitDoWhile(_scopes.Peek(), token, this, doWhileScope.DoWhile, _input);
#endif
                                    if (exitDoWhile == null)
                                    {
                                        exitDoWhile = new Scope.ExitDoWhile(token, doWhileScope, doWhileScope.DoWhile, new Expression.Constant(new Token[] { }));
                                    }
                                    return exitDoWhile;
                                }
                        }
                        return new Scope.Exit(token, subscope);
                    }
                case "(":
                    {
                        _input.FrontLoadToken(token);
#if NET6_0_OR_GREATER
                        Expression? expression = Expression.Parse(_scopes.Peek(), this, _input);
#else
                        Expression expression = Expression.Parse(_scopes.Peek(), this, _input);
#endif
                        token = _input.ReadNextToken();
                        if (token == null)
                        {
                            Error_ExpectedSemicolumn(token);
                        }
                        else if (token.Value != ";")
                        {
                            while (!ReattachToken(token)) { token = _input.ReadNextToken(); }
                            Error_ExpectedSemicolumn(token);
                        }
                        return expression;
                    }
                case "if":
                    {
#if NET6_0_OR_GREATER
                        If? ifStatement = If.ParseIf(_scopes.Peek(), this, token, _input);
#else
                        If ifStatement = If.ParseIf(_scopes.Peek(), this, token, _input);
#endif
                        if (ifStatement == null)
                        {
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return ifStatement;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(ifStatement.Body);
                            _input.ReadNextToken();
                            return ifStatement;
                        }
                        return new UnscopedIf(this, token, ifStatement.Condition);
                    }
                case "else":
                    {
#if NET6_0_OR_GREATER
                        Else? elseStatement = new Else(_scopes.Peek(), this, token);
                        Token? next = _input.PeekToken();
#else
                        Else elseStatement = new Else(_scopes.Peek(), this, token);
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return elseStatement;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(elseStatement.Body);
                            _input.ReadNextToken();
                            return elseStatement;
                        }
                        return new UnscopedElse(this, token);
                    }
                case "for":
                    {
#if NET6_0_OR_GREATER
                        For? forStatement = For.ParseFor(_scopes.Peek(), this, token, _input);
#else
                        For forStatement = For.ParseFor(_scopes.Peek(), this, token, _input);
#endif
                        if (forStatement == null)
                        {
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return forStatement;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(forStatement.Body);
                            _input.ReadNextToken();
                            return forStatement;
                        }
                        UnscopedFor unscopedFor = new UnscopedFor(_scopes.Peek(), this, token, forStatement.VariableDeclarations, forStatement.Condition, forStatement.Increment);
                        _scopes.Push(unscopedFor.Body);
                        _unscopedForScopes.Push(unscopedFor.Body);
                        return unscopedFor;
                    }
                case "do":
                    {
                        DoWhile doWhile = new DoWhile(_scopes.Peek(), this, token);
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return doWhile;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(doWhile.Body);
                            _input.ReadNextToken();
                            return doWhile;
                        }
                        return new UnscopedDo(this, token);
                    }
                    break;
                case "while":
                    {
#if NET6_0_OR_GREATER
                        While? whileStatement = While.ParseWhile(_scopes.Peek(), this, token, _input);
#else
                        While whileStatement = While.ParseWhile(_scopes.Peek(), this, token, _input);
#endif
                        if (whileStatement == null)
                        {
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return whileStatement;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(whileStatement.Body);
                            _input.ReadNextToken();
                            return whileStatement;
                        }

                        if (next.Value == ";")
                        {
                            _input.ReadNextToken();
                            return new EmptyWhile(this, token, whileStatement.Condition);
                        }
                        else
                        {
                            return new UnscopedWhile(this, token, whileStatement.Condition);
                        }
                    }
                case "return":
                    {
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                        }

                        if (next.Value == ";")
                        {
                            _input.ReadNextToken();
                            return new Return(_scopes.Peek(), this, token, null);
                        }
#if NET6_0_OR_GREATER
                        Expression? expression = Expression.Parse(_scopes.Peek(), this, _input, true);
#else
                        Expression expression = Expression.Parse(_scopes.Peek(), this, _input, true);
#endif
                        next = _input.PeekToken();
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                        }
                        else if (next.Value != ";")
                        {
                            Error_ExpectedSemicolumn(next);
                        }
                        return new Return(_scopes.Peek(), this, token, expression);
                    }
                    break;
                case "typedef":
#if NET6_0_OR_GREATER
                    Typedef[]? typedef = Typedef.Parse(_scopes.Peek(), this, token, _input);
#else
                    Typedef[] typedef = Typedef.Parse(_scopes.Peek(), this, token, _input);
#endif
                    if (typedef == null || typedef.Length == 0) { goto restart; }
                    for (int n = 0; n < typedef.Length; n++)
                    {
                        _scopes.Peek().AddType(typedef[n].Name, typedef[n].Type);
                        _pendingStatements.Enqueue(typedef[n]);
                    }
                    return null;
                case "switch":
                    {
#if NET6_0_OR_GREATER
                        Switch? switchStatement = Switch.ParseSwitch(_scopes.Peek(), this, token, _input);
#else
                        Switch switchStatement = Switch.ParseSwitch(_scopes.Peek(), this, token, _input);
#endif
                        if (switchStatement == null)
                        {
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            return switchStatement;
                        }
                        if (next.Value == "{")
                        {
                            _scopes.Push(switchStatement.Body);
                            _input.ReadNextToken();
                            return switchStatement;
                        }

                        throw new NotSupportedException();
                    }
                case "case":
                    {
#if NET6_0_OR_GREATER
                        Token? next = _input.PeekToken();
#else
                        Token next = _input.PeekToken();
#endif
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Expression? constantExpression = Expression.Parse(_scopes.Peek(), this, _input, true);
#else
                        Expression constantExpression = Expression.Parse(_scopes.Peek(), this, _input, true);
#endif
                        next = _input.ReadNextToken();
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                            return null;
                        }
                        if (next.Value != ":")
                        {
                            // TODO Error
                        }
#if NET6_0_OR_GREATER
                        Scope.SwitchScope? switchScope = _scopes.Peek() as Scope.SwitchScope;
#else
                        Scope.SwitchScope switchScope = _scopes.Peek() as Scope.SwitchScope;
#endif
                        if (switchScope == null)
                        {
                            Error_CaseOutsideOfSwitch(token);
                            return null;
                        }

                        return new Case(this, token, switchScope.Switch, constantExpression, next);
                    }
                case "continue":
                case "break":
                    {
                        bool isContinue = (token.Value == "continue");
                        bool noMoreErrors = false;
#if NET6_0_OR_GREATER
                        Token? next = _input.ReadNextToken();
#else
                        Token next = _input.ReadNextToken();
#endif
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                            return null;
                        }
                        if (next.Value != ";")
                        {
                            Error_ExpectedSemicolumn(next);
                            ReattachToken(next);
                            noMoreErrors = true;
                        }
#if NET6_0_OR_GREATER
                        IScope? scope = _scopes.Peek().GetBreakContinueScope();
#else
                        IScope scope = _scopes.Peek().GetBreakContinueScope();
#endif
                        if (scope == null)
                        {
                            if (!noMoreErrors)
                            {
                                Error_InvalidStatementOutsideOfASwitchOrLoop(token);
                            }
                            return new Empty(token);
                        }

                        switch (scope)
                        {
                            case Scope.ForScope forScope: if (isContinue) { return new Continue.ForContinue(token, forScope.For); } else { return new Break.ForBreak(token, forScope.For); }
                            case Scope.UnscopedForScope forScope: if (isContinue) { return new Continue.UnscopedForContinue(token, forScope.For); } else { return new Break.UnscopedForBreak(token, forScope.For); }
                            case Scope.WhileScope whileScope: if (isContinue) { return new Continue.WhileContinue(token, whileScope.While); } else { return new Break.WhileBreak(token, whileScope.While); }
                            case Scope.DoWhileScope doWhileScope: if (isContinue) { return new Continue.DoWhileContinue(token, doWhileScope.DoWhile); } else { return new Break.DoWhileBreak(token, doWhileScope.DoWhile); }
                            case Scope.SwitchScope switchScope: if (isContinue) { return new Continue.SwitchContinue(token, switchScope.Switch); } else { return new Break.SwitchBreak(token, switchScope.Switch); }
                            default: throw new Exception("Internal error: the scope should be a switch or loop");
                        }
                    }
                case "default":
                    {
#if NET6_0_OR_GREATER
                        Token? next = _input.ReadNextToken();
#else
                        Token next = _input.ReadNextToken();
#endif
                        if (next == null)
                        {
                            Error_IncompleteStatement(token);
                            return null;
                        }
                        if (next.Value != ":")
                        {
                            // TODO Error
                        }
#if NET6_0_OR_GREATER
                        Scope.SwitchScope? switchScope = _scopes.Peek() as Scope.SwitchScope;
#else
                        Scope.SwitchScope switchScope = _scopes.Peek() as Scope.SwitchScope;
#endif
                        if (switchScope == null)
                        {
                            Error_CaseOutsideOfSwitch(token);
                            return null;
                        }
                        return new Default(this, token, switchScope.Switch, next);
                    }
                default:
                    _input.FrontLoadToken(token);
                    Token firstToken = token;
                    Attribute[] attributes = Attribute.Parse(this, _input);
                    token = _input.ReadNextToken();
                    if (token == null)
                    {
                        Error_IncompleteDeclaration(firstToken);
                        return null;
                    }
                    if (token.Value == ";")
                    {
                        return new Empty(token);
                    }
                    if (Identifier.IsValid(token, _standardRevision) && (_scopes.Peek().ResolveType(token.Value) == null))
                    {
#if NET6_0_OR_GREATER
                        Token? maybeLabel = _input.PeekToken();
#else
                        Token maybeLabel = _input.PeekToken();
#endif
                        if (maybeLabel != null && maybeLabel.Value == ":")
                        {
                            _input.ReadNextToken();
#if NET6_0_OR_GREATER
                            Function? function = _scopes.Peek().GetParentFunction();
#else
                            Function function = _scopes.Peek().GetParentFunction();
#endif
                            if (function == null)
                            {
                                Error_LabelUsedOutsideFunction(maybeLabel);
                            }
                            else
                            {
                                return function.GetOrDeclareLabel(maybeLabel);
                            }
                        }
                        _input.FrontLoadToken(token);
#if NET6_0_OR_GREATER
                        Expression? expression = Expression.Parse(_scopes.Peek(), this, _input, false);
#else
                        Expression expression = Expression.Parse(_scopes.Peek(), this, _input, false);
#endif
                        token = _input.ReadNextToken();
                        if (token == null)
                        {
                            Error_ExpectedSemicolumn(firstToken);
                        }
                        else if (token.Value != ";")
                        {
                            while (!ReattachToken(token)) { token = _input.ReadNextToken(); }
                            Error_ExpectedSemicolumn(firstToken);
                        }
                        return expression;
                    }
#if NET6_0_OR_GREATER
                    Type? type = Type.Parse(_scopes.Peek(), this, token, _input);
#else
                    Type type = Type.Parse(_scopes.Peek(), this, token, _input);
#endif
                    if (type != null)
                    {
                        return ParseDeclaration(attributes, type, token);
                    }

                    Error_UnexpectedToken(token);
                    break;
            }

            return null;
        }
        void ReadNextBlock()
        {
            List<Statement> statements = new List<Statement>();

            ReadNextStatement();
        }
#if NET6_0_OR_GREATER
        public Statement? ReadNextStatement()
#else
        public Statement ReadNextStatement()
#endif
        {
            if (_pendingStatements.Count > 0)
            {
                return _pendingStatements.Dequeue();
            }
            switch (_state)
            {
                case State.Normal:
                    restart:
#if NET6_0_OR_GREATER
                    Statement? statement = ReadNextStatementNormal();
#else
                    Statement statement = ReadNextStatementNormal();
#endif
                    if (statement == null && _pendingStatements.Count > 0)
                    {
                        return _pendingStatements.Dequeue();
                    }

                    if (statement == null)
                    {
                        if (_input.PeekToken() == null) { return null; }
                        goto restart;
                    }
                    if (_unscopedForScopes.Count > 0)
                    {
                        if (!_unscopedForScopes.Peek().CheckLiveness(statement, _scopes.Peek())) { _unscopedForScopes.Pop(); _scopes.Pop(); }
                    }
                    return statement;
                case State.VariableDeclaration:
                    return ReadNextStatementVariableDeclaration();
            }
            return null;
        }
    }
}
