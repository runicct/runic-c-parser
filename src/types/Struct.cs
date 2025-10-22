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

using System.Text;

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Type
        {
            public abstract class StructOrUnion : Type, IScope
            {
                class StructOrUnionDeclation : StructOrUnion
                {
                    bool _union = false;
                    Token _structToken;
                    Token? _name;
                    internal Field[]? _fields;
                    internal Dictionary<string, Type> _typeDeclarations = new Dictionary<string, Type>();
                    IScope _parentScope;
                    internal StructOrUnionDeclation(IScope ParentScope, Token StructToken, Token? Name)
                    {
                        _union = (StructToken != null && StructToken.Value == "union");
                        _structToken = StructToken;
                        _name = Name;
                        _parentScope = ParentScope;
                    }
                    public override bool Union { get { return _union; } }
                    public override StructOrUnion? Declaration { get { return this; } }
                    public override IScope? ParentScope { get { return _parentScope; } }
                    public override Token? Name { get { return _name; } }
                    public override Field[]? Fields { get { return _fields; } }
                    uint _packing = 1;
                    public override uint Packing { get { return _packing; } }
                    uint _padding = 1;
                    public override uint Padding { get { return _padding; } }
                    public override Type? ResolveType(string Name)
                    {
                        Type? type = null;
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
                    public override Variable? ResolveVariable(string Name)
                    {
                        return _parentScope.ResolveVariable(Name);
                    }
                    public override Function? ResolveFunction(string Name)
                    {
                        return _parentScope.ResolveFunction(Name);
                    }
                    public override Type.Enum.Member? ResolveEnumMember(string Name)
                    {
                        return _parentScope.ResolveEnumMember(Name);
                    }

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
                    Token? _name;
                    IScope _parentScope;
                    StructOrUnionDeclation? _declaration = null;
                    public StructOrUnionReference(IScope ParentScope, Token StructToken, Token? Name, StructOrUnionDeclation? Declaration)
                    {
                        _union = (StructToken != null && StructToken.Value == "union");
                        _structToken = StructToken;
                        _name = Name;
                        _parentScope = ParentScope;
                        _declaration = Declaration;
                    }
                    public override bool Union { get { return _union; } }
                    public override StructOrUnion? Declaration { get { return _declaration; } }
                    public override IScope? ParentScope { get { return _parentScope; } }
                    public override Token? Name { get { return _name; } }
                    public override Field[]? Fields { get { if (_declaration == null) { return null; } return _declaration._fields; } }
                    public override uint Packing { get { if (_declaration == null) { return 1; } return _declaration.Packing; } }
                    public override uint Padding { get { if (_declaration == null) { return 1; } return _declaration.Padding; } }
                    public override Type? ResolveType(string Name)
                    {
                        if (_parentScope != null)
                        {
                            return _parentScope.ResolveType(Name);
                        }
                        return null;
                    }
                    public override Variable? ResolveVariable(string Name)
                    {
                        return _parentScope.ResolveVariable(Name);
                    }
                    public override Function? ResolveFunction(string Name)
                    {
                        return _parentScope.ResolveFunction(Name);
                    }
                    public override Type.Enum.Member? ResolveEnumMember(string Name)
                    {
                        return _parentScope.ResolveEnumMember(Name);
                    }
                    public override string ToString()
                    {
                        if (_name == null) { return "struct"; }
                        return "struct " + _name.Value;
                    }
                }

                public abstract IScope? ParentScope { get; }
                public abstract Type? ResolveType(string Name);
                public abstract Variable? ResolveVariable(string Name);
                public abstract Function? ResolveFunction(string Name);
                public abstract Type.Enum.Member? ResolveEnumMember(string Name);
                public IScope? GetBreakContinueScope() { return null; }
                public abstract Token? Name { get; }
                public abstract Field[]? Fields { get; }
                public abstract StructOrUnion? Declaration { get; }
                public abstract bool Union { get; }
                public abstract uint Packing { get; }
                public abstract uint Padding { get; }
                static internal StructOrUnion? Parse(IScope ParentScope, Parser Context, Token StructToken, TokenQueue TokenQueue)
                {
                    Token? name = null;
                    Token? nameOrAnonymous = TokenQueue.ReadNextToken();
                    if (nameOrAnonymous == null)
                    {
                        Context.Error_IncompleteDeclaration(StructToken);
                        return new StructOrUnionReference(ParentScope, StructToken, null, null);
                    }
                    if (nameOrAnonymous.Value != "{")
                    {
                        name = nameOrAnonymous;
                        Token? nextToken = TokenQueue.PeekToken();
                        if (nextToken == null)
                        {
                            Context.Error_IncompleteDeclaration(StructToken);
                            return new StructOrUnionReference(ParentScope, StructToken, null, null);
                        }
                        if (nextToken.Value != "{")
                        {
                            Type? referencedType = ParentScope.ResolveType(name.Value);
                            if (referencedType == null)
                            {
                                return new StructOrUnionReference(ParentScope, StructToken, name, null);
                            }
                            else
                            {
                                StructOrUnionDeclation? referencedStruct = referencedType as StructOrUnionDeclation;
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

                    StructOrUnionDeclation pendingStruct = new StructOrUnionDeclation(ParentScope, StructToken, name);

                    List<Field> fields = new List<Field>();
                    while (true)
                    {
                        Token? token = TokenQueue.ReadNextToken();
                        if (token == null) { return null; }
                        if (token.Value == "}")
                        {
                            pendingStruct._fields = fields.ToArray();
                            return pendingStruct;
                        }

                        Type? type = Type.Parse(pendingStruct, Context, token, TokenQueue);
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

                        Token? fieldName = null;
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
                                case StructOrUnionDeclation structDeclarationType:
                                    {
                                        if (structDeclarationType.Name != null)
                                        {
                                            Type? previousDeclation;
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
                                    Token? startToken = token;

                                    while (true)
                                    {
                                        Expression? expression = Expression.Parse(pendingStruct, Context, TokenQueue);
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
                public Field? ResolveField(string name)
                {
                    Field[]? fields = Fields;
                    if (fields == null) { return null; }
                    for (int n = 0; n < fields.Length; n++)
                    {
                        if (fields[n].Name == null)
                        {
                            StructOrUnion? structOrUnion = fields[n].Type as StructOrUnion;
                            if (structOrUnion != null)
                            {
                                Field? result = structOrUnion.ResolveField(name);
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
