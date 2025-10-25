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

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Type
        {
            public abstract class Enum : Type, IScope
            {
                public class Member : Variable
                {
                    Token _name;
                    public Token Name { get { return _name; } }
                    Expression _value;
                    public Expression Value { get { return _value; } }
                    internal Member(Attribute[] attributes, EnumDeclation parent, Token name, Expression value) : base(attributes, parent, name)
                    {
                        _name = name;
                        _value = value;
                    }
                }
                internal class EnumDeclation : Enum
                {
                    internal Member[] _members;
                    public Member[] Members { get { return _members; } }
#if NET6_0_OR_GREATER
                    public EnumDeclation(Attribute[] attributes, IScope parentScope, Token enumToken, Token? name) : base(attributes, parentScope, enumToken, name)
#else
                    public EnumDeclation(Attribute[] attributes, IScope parentScope, Token enumToken, Token name) : base(attributes, parentScope, enumToken, name)
#endif
                    {

                    }
                }

                internal class EnumReference : Enum
                {
#if NET6_0_OR_GREATER
                    public EnumReference(Attribute[] attributes, IScope parentScope, Token enumToken, Token? name) : base(attributes, parentScope, enumToken, name)
#else
                    public EnumReference(Attribute[] attributes, IScope parentScope, Token enumToken, Token name) : base(attributes, parentScope, enumToken, name)
#endif
                    {

                    }
                }
                IScope _parent;
#if NET6_0_OR_GREATER
                Token? _name;
                public Token? Name { get { return _name; } }

                public Type? ResolveType(string Name) { return _parent.ResolveType(Name); }
                public Variable? ResolveVariable(string Name) { return _parent.ResolveVariable(Name); }
                public Function? ResolveFunction(string Name) { return _parent.ResolveFunction(Name); }
                public Member? ResolveEnumMember(string Name) { return _parent.ResolveEnumMember(Name); }
                public IScope? GetBreakContinueScope() { return null; }

                public IScope? ParentScope { get { return _parent; } }
#else
                Token _name;
                public Token Name { get { return _name; } }

                public Type ResolveType(string Name) { return _parent.ResolveType(Name); }
                public Variable ResolveVariable(string Name) { return _parent.ResolveVariable(Name); }
                public Function ResolveFunction(string Name) { return _parent.ResolveFunction(Name); }
                public Member ResolveEnumMember(string Name) { return _parent.ResolveEnumMember(Name); }
                public IScope GetBreakContinueScope() { return null; }

                public IScope ParentScope { get { return _parent; } }
#endif

#if NET6_0_OR_GREATER
                public Enum(Attribute[] attributes, IScope parentScope, Token enumToken, Token? name)
#else
                public Enum(Attribute[] attributes, IScope parentScope, Token enumToken, Token name)
#endif
                {
                    _parent = parentScope;
                    _name = name;
                }
#if NET6_0_OR_GREATER
                internal static Enum? Parse(IScope ParentScope, Parser Context, Token EnumToken, TokenQueue TokenQueue)
#else
                internal static Enum Parse(IScope ParentScope, Parser Context, Token EnumToken, TokenQueue TokenQueue)
#endif
                {
#if NET6_0_OR_GREATER
                    Expression? previousValue = null;
                    Token? name = null;
                    Token? token = TokenQueue.PeekToken();
#else
                    Expression previousValue = null;
                    Token name = null;
                    Token token = TokenQueue.PeekToken();
#endif
                    ulong counter = 0;
                    if (token == null)
                    {
                        Context.Error_IncompleteDeclaration(EnumToken);
                        return null;
                    }

                    if (token.Value != "{")
                    {
                        name = token;
                        if (!Identifier.IsValid(name, Context.StandardRevision)) { Context.Error_InvalidIdentifier(name); }
                        TokenQueue.ReadNextToken();
                        token = TokenQueue.PeekToken();
                        if (token == null)
                        {
                            Context.Error_IncompleteDeclaration(EnumToken);
                            return null;
                        }
                    }

                    if (token.Value == ";")
                    {
                        return new EnumReference(new Attribute[0], ParentScope, EnumToken, name);
                    }

                    if (token.Value != "{")
                    {
                        Context.Error_IncompleteDeclaration(EnumToken);
                        return null;
                    }

                    TokenQueue.ReadNextToken();

                    List<Member> enumMembers = new List<Member>();
                    EnumDeclation pendingEnum = new EnumDeclation(new Attribute[0], ParentScope, EnumToken, name);
                    while (true)
                    {
#if NET6_0_OR_GREATER
                        Token? memberName = TokenQueue.ReadNextToken();
#else
                        Token memberName = TokenQueue.ReadNextToken();
#endif
                        if (memberName == null)
                        {
                            pendingEnum._members = enumMembers.ToArray();
                            Context.Error_IncompleteDeclaration(EnumToken);
                            return pendingEnum;
                        }
                        if (!Identifier.IsValid(memberName, Context.StandardRevision)) { Context.Error_InvalidIdentifier(memberName); }

#if NET6_0_OR_GREATER
                        Token? nextToken = TokenQueue.ReadNextToken();
#else
                        Token nextToken = TokenQueue.ReadNextToken();
#endif
                        if (nextToken == null)
                        {
                            pendingEnum._members = enumMembers.ToArray();
                            Context.Error_IncompleteDeclaration(EnumToken);
                            return pendingEnum;
                        }
                        if (nextToken.Value == "=")
                        {
#if NET6_0_OR_GREATER
                            Expression? value = Expression.Parse(pendingEnum, Context, TokenQueue);
#else
                            Expression value = Expression.Parse(pendingEnum, Context, TokenQueue);
#endif
                            previousValue = value;
                            counter = 0;
                            if (value != null)
                            {
                                enumMembers.Add(new Member(new Attribute[0], pendingEnum, memberName, value));
                            }
                            nextToken = TokenQueue.ReadNextToken();
                            if (nextToken == null)
                            {
                                pendingEnum._members = enumMembers.ToArray();
                                Context.Error_IncompleteDeclaration(EnumToken);
                                return pendingEnum;
                            }
                        }
                        else
                        {

                            Expression value;
                            if (previousValue != null)
                            {
                                value = new Expression.Add(memberName, previousValue, new Expression.Constant(new Token[] { new ImplicitToken(memberName.StartLine, memberName.StartColumn, memberName.EndLine, memberName.EndColumn, memberName.File, counter.ToString()) }));
                            }
                            else
                            {
                                value = new Expression.Constant(new Token[] { new ImplicitToken(memberName.StartLine, memberName.StartColumn, memberName.EndLine, memberName.EndColumn, memberName.File, counter.ToString()) });
                            }
                            enumMembers.Add(new Member(new Attribute[0], pendingEnum, memberName, value));
                            counter++;
                        }

                        if (nextToken.Value == "}")
                        {
                            pendingEnum._members = enumMembers.ToArray();
                            return pendingEnum;
                        }

                        if (nextToken.Value != ",")
                        {
                            Context.Error_InvalidEnumValue(EnumToken, nextToken);
                            while (nextToken.Value != ",")
                            {
                                nextToken = TokenQueue.ReadNextToken();
                                if (nextToken == null || nextToken.Value == "}")
                                {
                                    pendingEnum._members = enumMembers.ToArray();
                                    return pendingEnum;
                                }
                            }
                        }
                    }
                    return null;
                }
            }
        }
    }
}