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

using System.Collections.Generic;
using System.Text;

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Type
        {
            public abstract class StructOrUnion : Type, IScope
            {
                class StructOrUnionDeclaration : StructOrUnion
                {
                    bool _union = false;
                    Token _structToken;
                    public Token Keyword { get { return _structToken; } }
#if NET6_0_OR_GREATER
                    Token? _name;
                    public override Token? Name { get { return _name; } }
#else
                    Token _name;
                    public override Token Name { get { return _name; } }
#endif
                    internal Field[] _fields;
                    public override Field[] Fields { get { return _fields; } }
                    internal Dictionary<string, Type> _typeDeclarations = new Dictionary<string, Type>();
                    IScope _parentScope;
#if NET6_0_OR_GREATER
                    internal StructOrUnionDeclaration(IScope parentScope, Token structToken, Token? name)
#else
                    internal StructOrUnionDeclaration(IScope parentScope, Token structToken, Token name)
#endif
                    {
                        _union = (structToken.Value == "union");
                        _structToken = structToken;
                        _name = name;
                        _parentScope = parentScope;
                        _fields = new Field[0];
                    }
                    public override bool Union { get { return _union; } }
#if NET6_0_OR_GREATER
                    public override StructOrUnion? Declaration { get { return this; } }
                    public override IScope? ParentScope { get { return _parentScope; } }
#else
                    public override StructOrUnion Declaration { get { return this; } }
                    public override IScope ParentScope { get { return _parentScope; } }
#endif
                    uint _packing = 1;
                    public override uint Packing { get { return _packing; } }
                    uint _padding = 1;
                    public override uint Padding { get { return _padding; } }
#if NET6_0_OR_GREATER
                    public override Type? ResolveType(string Name)
#else
                    public override Type ResolveType(string Name)
#endif
                    {
#if NET6_0_OR_GREATER
                        Type? type = null;
#else
                        Type type = null;
#endif
                        if (_typeDeclarations.TryGetValue(Name, out type))
                        {
                            return type;
                        }
                        if (_parentScope != null)
                        {
                            return _parentScope.ResolveType(Name);
                        }
                        return null;
                    }
#if NET6_0_OR_GREATER
                    public override Variable? ResolveVariable(string Name) { return _parentScope.ResolveVariable(Name); }
                    public override Function? ResolveFunction(string Name) { return _parentScope.ResolveFunction(Name); }
                    public override Type.Enum.Member? ResolveEnumMember(string Name) { return _parentScope.ResolveEnumMember(Name); }
#else
                    public override Variable ResolveVariable(string Name) { return _parentScope.ResolveVariable(Name); }
                    public override Function ResolveFunction(string Name) { return _parentScope.ResolveFunction(Name); }
                    public override Type.Enum.Member ResolveEnumMember(string Name) { return _parentScope.ResolveEnumMember(Name); }
#endif

                    public override string ToString()
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append("struct");
                        if (_name != null) { builder.Append(" " + _name.Value); }
                        builder.AppendLine();
                        builder.AppendLine("{");
                        foreach (Type typeDeclaration in _typeDeclarations.Values)
                        {
                            builder.Append(typeDeclaration);
                            builder.AppendLine(";");
                        }
                        if (_fields != null)
                        {
                            for (int n = 0; n < _fields.Length; n++)
                            {
                                builder.Append(_fields[n]);
                                builder.AppendLine(";");
                            }
                        }
                        builder.Append("}");
                        return builder.ToString(); 
                    }
                }

                class StructOrUnionReference : StructOrUnion
                {
                    bool _union;
                    Token _structToken;
                    IScope _parentScope;
#if NET6_0_OR_GREATER
                    Token? _name;
                    StructOrUnionDeclaration? _declaration = null;
#else
                     Token _name;
                    StructOrUnionDeclaration _declaration = null;
#endif
#if NET6_0_OR_GREATER
                    public StructOrUnionReference(IScope ParentScope, Token StructToken, Token? Name, StructOrUnionDeclaration? Declaration)
#else
                    public StructOrUnionReference(IScope ParentScope, Token StructToken, Token Name, StructOrUnionDeclaration Declaration)
#endif
                    {
                        _union = (StructToken != null && StructToken.Value == "union");
                        _structToken = StructToken;
                        _name = Name;
                        _parentScope = ParentScope;
                        _declaration = Declaration;
                    }
                    public override bool Union { get { return _union; } }
#if NET6_0_OR_GREATER
                    public override StructOrUnion? Declaration { get { return _declaration; } }
                    public override IScope? ParentScope { get { return _parentScope; } }
                    public override Token? Name { get { return _name; } }
#else
                    public override StructOrUnion Declaration { get { return _declaration; } }
                    public override IScope ParentScope { get { return _parentScope; } }
                    public override Token Name { get { return _name; } }
#endif
                    static Field[] _emptyFields = new Field[0];
                    public override Field[] Fields { get { if (_declaration == null) { return _emptyFields; } return _declaration._fields; } }
                    public override uint Packing { get { if (_declaration == null) { return 1; } return _declaration.Packing; } }
                    public override uint Padding { get { if (_declaration == null) { return 1; } return _declaration.Padding; } }
#if NET6_0_OR_GREATER
                    public override Type? ResolveType(string Name)
#else
                    public override Type ResolveType(string Name)
#endif
                    {
                        if (_parentScope != null)
                        {
                            return _parentScope.ResolveType(Name);
                        }
                        return null;
                    }
#if NET6_0_OR_GREATER
                    public override Variable? ResolveVariable(string Name)
#else
                    public override Variable ResolveVariable(string Name)
#endif
                    {
                        return _parentScope.ResolveVariable(Name);
                    }
#if NET6_0_OR_GREATER
                    public override Function? ResolveFunction(string Name)
#else
                    public override Function ResolveFunction(string Name)
#endif
                    {
                        return _parentScope.ResolveFunction(Name);
                    }
#if NET6_0_OR_GREATER
                    public override Type.Enum.Member? ResolveEnumMember(string Name)
#else
                    public override Type.Enum.Member ResolveEnumMember(string Name)
#endif
                    {
                        return _parentScope.ResolveEnumMember(Name);
                    }
                    public override string ToString()
                    {
                        if (_name == null) { return "struct"; }
                        return "struct " + _name.Value;
                    }
                }
#if NET6_0_OR_GREATER
                public abstract IScope? ParentScope { get; }
                public abstract Type? ResolveType(string Name);
                public abstract Variable? ResolveVariable(string Name);
                public abstract Function? ResolveFunction(string Name);
                public abstract Type.Enum.Member? ResolveEnumMember(string Name);
                public IScope? GetBreakContinueScope() { return null; }
                public abstract Token? Name { get; }
                public abstract Field[]? Fields { get; }
                public abstract StructOrUnion? Declaration { get; }
#else
                public abstract IScope ParentScope { get; }
                public abstract Type ResolveType(string Name);
                public abstract Variable ResolveVariable(string Name);
                public abstract Function ResolveFunction(string Name);
                public abstract Type.Enum.Member ResolveEnumMember(string Name);
                public IScope GetBreakContinueScope() { return null; }
                public abstract Token Name { get; }
                public abstract Field[] Fields { get; }
                public abstract StructOrUnion Declaration { get; }
#endif
                public abstract bool Union { get; }
                public abstract uint Packing { get; }
                public abstract uint Padding { get; }
#if NET6_0_OR_GREATER
                static internal StructOrUnion? Parse(IScope ParentScope, Parser Context, Token StructToken, TokenQueue TokenQueue)
#else
                static internal StructOrUnion Parse(IScope ParentScope, Parser Context, Token StructToken, TokenQueue TokenQueue)
#endif
                {
#if NET6_0_OR_GREATER
                    Token? name = null;
                    Token? nameOrAnonymous = TokenQueue.ReadNextToken();
#else
                    Token name = null;
                    Token nameOrAnonymous = TokenQueue.ReadNextToken();
#endif
                    if (nameOrAnonymous == null)
                    {
                        Context.Error_IncompleteDeclaration(StructToken);
                        return new StructOrUnionReference(ParentScope, StructToken, null, null);
                    }
                    if (nameOrAnonymous.Value != "{")
                    {
                        name = nameOrAnonymous;
#if NET6_0_OR_GREATER
                        Token? nextToken = TokenQueue.PeekToken();
#else
                        Token nextToken = TokenQueue.PeekToken();
#endif
                        if (nextToken == null)
                        {
                            Context.Error_IncompleteDeclaration(StructToken);
                            return new StructOrUnionReference(ParentScope, StructToken, null, null);
                        }
                        if (nextToken.Value != "{")
                        {
#if NET6_0_OR_GREATER
                            Type? referencedType = ParentScope.ResolveType(name.Value);
#else
                            Type referencedType = ParentScope.ResolveType(name.Value);
#endif
                            if (referencedType == null)
                            {
                                return new StructOrUnionReference(ParentScope, StructToken, name, null);
                            }
                            else
                            {
#if NET6_0_OR_GREATER
                                StructOrUnionDeclaration? referencedStruct = referencedType as StructOrUnionDeclaration;
#else
                                StructOrUnionDeclaration referencedStruct = referencedType as StructOrUnionDeclaration;
#endif
                                if (referencedStruct == null)
                                {
                                    // Raise an error
                                    return new StructOrUnionReference(ParentScope, StructToken, name, null);
                                }
                                else
                                {
                                    return new StructOrUnionReference(ParentScope, StructToken, name, referencedStruct);
                                }
                            }
                        }
                        TokenQueue.ReadNextToken();
                    }

                    StructOrUnionDeclaration pendingStruct = new StructOrUnionDeclaration(ParentScope, StructToken, name);

                    List<Field> fields = new List<Field>();
                    while (true)
                    {
#if NET6_0_OR_GREATER
                        Token? token = TokenQueue.ReadNextToken();
#else
                        Token token = TokenQueue.ReadNextToken();
#endif
                        if (token == null) { return null; }
                        if (token.Value == "}")
                        {
                            pendingStruct._fields = fields.ToArray();
                            return pendingStruct;
                        }
#if NET6_0_OR_GREATER
                        Type? type = Type.Parse(pendingStruct, Context, token, TokenQueue);
#else
                        Type type = Type.Parse(pendingStruct, Context, token, TokenQueue);
#endif
                        if (type == null)
                        {
                            Context.Error_InvalidType(token);

                            while (token != null && token.Value != ";")
                            {
                                if (token.Value == "}") { pendingStruct._fields = fields.ToArray(); return pendingStruct; }
                                token = TokenQueue.ReadNextToken();
                            }

                            continue;
                        }

#if NET6_0_OR_GREATER
                        Token? fieldName = null;
#else
                        Token fieldName = null;
#endif
                        token = TokenQueue.ReadNextToken();
                        if (token == null) 
                        {
                            Context.Error_IncompleteDeclaration(StructToken);
                            pendingStruct._fields = fields.ToArray();
                            return pendingStruct;
                        }
                        if (token.Value == ";")
                        {
                            switch (type)
                            {
                                case StructOrUnionDeclaration structDeclarationType:
                                    {
                                        if (structDeclarationType.Name != null)
                                        {
#if NET6_0_OR_GREATER
                                            Type? previousDeclation;
#else
                                            Type previousDeclation;
#endif
                                            if (pendingStruct._typeDeclarations.TryGetValue(structDeclarationType.Name.Value, out previousDeclation))
                                            {
                                                Context.Error_TypeWasAlreadyDeclared(previousDeclation, structDeclarationType.Name, structDeclarationType);
                                            }
                                            else
                                            {
                                                pendingStruct._typeDeclarations.Add(structDeclarationType.Name.Value, structDeclarationType);
                                            }
                                        }
                                        else
                                        {
                                            fields.Add(new Field(structDeclarationType, null, (uint)fields.Count));
                                        }
                                    }
                                    break;
                                case FunctionPointerType functionPointerType:
                                    {
                                        if (functionPointerType.Name != null)
                                        {
                                            fields.Add(new Field(functionPointerType, functionPointerType.Name, (uint)fields.Count));
                                        }
                                        else
                                        {
                                            Context.Error_AnonymousFunctionPointerInStruct(StructToken, functionPointerType);
                                        }
                                    }
                                    break;
                                default:
                                    Context.Error_MissingFieldName(StructToken, type);
                                    break;
                            }
                        }
                        else
                        {
                            fieldName = token;
                            while (true)
                            {
                                token = TokenQueue.ReadNextToken();
                                if (token == null)
                                {
                                    fields.Add(new Field(type, fieldName, (uint)fields.Count));
                                    Context.Error_IncompleteDeclaration(StructToken);
                                    pendingStruct._fields = fields.ToArray();
                                    return pendingStruct;
                                }


                                if (token.Value == "[")
                                {
                                    List<Expression> length = new List<Expression>();
#if NET6_0_OR_GREATER
                                    Token? startToken = token;
#else
                                    Token startToken = token;
#endif
                                    while (true)
                                    {
#if NET6_0_OR_GREATER
                                        Expression? expression = Expression.Parse(pendingStruct, Context, TokenQueue);
#else
                                        Expression expression = Expression.Parse(pendingStruct, Context, TokenQueue);
#endif
                                        if (expression != null) { length.Add(expression); }

                                        token = TokenQueue.ReadNextToken();
                                        if (token == null)
                                        {
                                            Context.Error_IncompleteDeclaration(StructToken);
                                            return pendingStruct;
                                        }


                                        if (token.Value == "]")
                                        {
                                            break;
                                        }
                                        if (token.Value == ",")
                                        {
                                            continue;
                                        }
                                        Context.Error_UnbalencedBracket(token);
                                        while (token != null && token.Value != ";")
                                        {
                                            if (token.Value == "}") { return pendingStruct; }
                                            token = TokenQueue.ReadNextToken();
                                        }
                                        break;
                                    }

                                    type = type.MakeStaticArray(startToken, length.ToArray());
                                    token = TokenQueue.ReadNextToken();
                                    if (token == null)
                                    {
                                        fields.Add(new Field(type, fieldName, (uint)fields.Count));
                                        Context.Error_IncompleteDeclaration(StructToken);
                                        pendingStruct._fields = fields.ToArray();
                                        return pendingStruct;
                                    }
                                }
                                fields.Add(new Field(type, fieldName, (uint)fields.Count));

                                if (token.Value == ",")
                                {
                                    token = TokenQueue.ReadNextToken();
                                    type = type.Strip();
                                    while (token != null && token.Value == "*")
                                    {
                                        type = type.MakePointer(token);
                                        token = TokenQueue.ReadNextToken();
                                    }
                                    fieldName = token;
                                    continue;
                                }

                                if (token.Value != ";")
                                {
                                    Context.Error_ExtraTokenAfterFieldDeclation(fieldName, token);
                                    while (token != null && token.Value != ";")
                                    {
                                        if (token.Value == "}") { return pendingStruct; }
                                        token = TokenQueue.ReadNextToken();
                                    }
                                }
                                break;
                            }
                        }
                    }

                    return null;
                }
#if NET6_0_OR_GREATER
                public Field? ResolveField(string name)
#else
                public Field ResolveField(string name)
#endif
                {
                    Field[] fields = Fields;
                    for (int n = 0; n < fields.Length; n++)
                    {
                        if (fields[n].Name == null)
                        {
#if NET6_0_OR_GREATER
                            StructOrUnion? structOrUnion = fields[n].Type as StructOrUnion;
#else
                            StructOrUnion structOrUnion = fields[n].Type as StructOrUnion;
#endif
                            if (structOrUnion != null)
                            {
#if NET6_0_OR_GREATER
                                Field? result = structOrUnion.ResolveField(name);
#else
                                Field result = structOrUnion.ResolveField(name);
#endif
                                if (result != null) { return result; }
                            }
                        }
                        else if (fields[n].Name.Value == name) { return fields[n]; }
                    }
                    return null;
                }
            }

        }
    }
}
