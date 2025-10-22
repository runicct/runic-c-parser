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
        public abstract partial class Type
        {
            internal Pointer MakePointer(Token? token)
            {
                return new Pointer(this, token);
            }
            internal Type MakeConst(Token? token)
            {
                return this;
            }
            internal StaticArray MakeStaticArray(Token? token, Expression[] length)
            {
                return new StaticArray(this, token, length);
            }
            internal virtual Type Strip()
            {
                return this;
            }
            static internal Type? Parse(IScope ParentScope, Parser Context, Token? Type, TokenQueue TokenQueue)
            {
                bool @const = false; 
                if (Type == null) { return null; }
                if (Type.Value == "const") { @const = true; Type = TokenQueue.ReadNextToken(); }

                Type? prologueType = ParsePrologue(ParentScope,Context, Type, TokenQueue);
                if (prologueType == null) { return null; }

                Token? parenthesisOrPointer = TokenQueue.PeekToken();
                if (parenthesisOrPointer == null) { return prologueType; }
                if (parenthesisOrPointer.Value == "const")
                {
                    TokenQueue.ReadNextToken();
                    @const = true;
                    parenthesisOrPointer = TokenQueue.PeekToken();
                    if (parenthesisOrPointer == null) { return prologueType; }
                }
                if (parenthesisOrPointer.Value == "*")
                {
                    Token? pointer = parenthesisOrPointer;
                    do
                    {
                        prologueType = prologueType.MakePointer(pointer);
                        TokenQueue.ReadNextToken();
                        pointer = TokenQueue.PeekToken();
                        if (pointer == null) { return prologueType; }

                        if (pointer.Value == "const")
                        {
                            prologueType = prologueType.MakeConst(pointer);
                            TokenQueue.ReadNextToken();
                            pointer = TokenQueue.PeekToken();
                            if (pointer == null) { return prologueType; }
                        }
                    } while (pointer.Value == "*");
                    parenthesisOrPointer = pointer;
                }

                if (parenthesisOrPointer.Value != "(") { return prologueType; }

                // Function pointer case
                {
                    TokenQueue.ReadNextToken();
                    Token? pointer = TokenQueue.PeekToken();
                    if (pointer == null) { return prologueType; }
                    if (pointer.Value != "*") { return prologueType; }
                    TokenQueue.ReadNextToken();
                    FunctionPointerType functionPointer = FunctionPointerType.Parse(ParentScope, Context, prologueType, TokenQueue);
                    if (functionPointer == null)
                    {
                        Context.Error_InvalidFunctionPointerDeclaration(pointer);
                        return null;
                    }
                    return functionPointer;
                }
            }
            static Type? ParsePrologue(IScope ParentScope, Parser Context, Token? Type, TokenQueue TokenQueue)
            {
                Token? nextToken;

                if (Type == null) { return null; }
                switch (Type.Value)
                {
                    case "int": return new Int(Type);
                    case "long":
                        nextToken = TokenQueue.ReadNextToken();
                        if (nextToken == null) { return new Long(Type); }
                        switch (nextToken.Value)
                        {
                            case "long": return new LongLong(Type, nextToken);
                            default: TokenQueue.FrontLoadToken(nextToken); return new Long(Type); break;
                        }
                    case "short":
                        nextToken = TokenQueue.ReadNextToken();
                        if (nextToken == null) { return new Short(Type); }
                        switch (nextToken.Value)
                        {
                            case "int": return new Short(Type);
                            default: TokenQueue.FrontLoadToken(nextToken); return new Short(Type); break;
                        }
                    case "signed":
                        nextToken = TokenQueue.ReadNextToken();
                        if (nextToken == null) { return null; }
                        switch (nextToken.Value)
                        {
                            case "short":
                                {
                                    nextToken = TokenQueue.ReadNextToken();
                                    if (nextToken == null) { return new Short(Type); }
                                    switch (nextToken.Value)
                                    {
                                        case "int": return new Short(Type);
                                        default: TokenQueue.FrontLoadToken(nextToken); return new Short(Type); break;
                                    }
                                }
                            case "int": return new Int(Type);
                            case "long":
                                {
                                    nextToken = TokenQueue.ReadNextToken();
                                    if (nextToken == null) { return new Long(Type); }
                                    switch (nextToken.Value)
                                    {
                                        case "int": return new Long(Type);
                                        case "long":
                                            {
                                                Token longToken = nextToken;
                                                nextToken = TokenQueue.ReadNextToken();
                                                if (nextToken == null) { return new LongLong(Type, longToken); }
                                                switch (nextToken.Value)
                                                {
                                                    case "int": return new LongLong(Type, longToken);
                                                    default: TokenQueue.FrontLoadToken(nextToken); return new LongLong(Type, longToken); break;
                                                }
                                            }
                                        default: TokenQueue.FrontLoadToken(nextToken); return new Long(Type); break;
                                    }
                                }
                            case "char": return new SignedChar(Type, nextToken);
                            default: TokenQueue.FrontLoadToken(nextToken); return new Int(Type); break;
                        }
                    case "unsigned":
                        nextToken = TokenQueue.ReadNextToken();
                        if (nextToken == null) { return new Unsigned(Type); }
                        switch (nextToken.Value)
                        {
                            case "char": return new UnsignedChar(Type, nextToken);
                            case "short":
                                {
                                    Token shortToken = nextToken;
                                    nextToken = TokenQueue.ReadNextToken();
                                    if (nextToken == null) { return new Short(Type); }
                                    switch (nextToken.Value)
                                    {
                                        case "int": return new UnsignedShort(Type, shortToken);
                                        default: TokenQueue.FrontLoadToken(nextToken); return new UnsignedShort(Type, shortToken); break;
                                    }
                                }
                            case "int": return new Unsigned(Type);
                            case "long":
                                {
                                    Token longToken = nextToken;
                                    nextToken = TokenQueue.ReadNextToken();
                                    if (nextToken == null) { return new UnsignedLong(Type, longToken); }
                                    switch (nextToken.Value)
                                    {
                                        case "int": return new UnsignedLong(Type, longToken);
                                        case "long":
                                            {
                                                Token secondLongToken = nextToken;
                                                nextToken = TokenQueue.ReadNextToken();
                                                if (nextToken == null) { return new UnsignedLongLong(Type, longToken, secondLongToken); }
                                                switch (nextToken.Value)
                                                {
                                                    case "int": return new UnsignedLongLong(Type, longToken, secondLongToken);
                                                    default: TokenQueue.FrontLoadToken(nextToken); return new UnsignedLongLong(Type, longToken, secondLongToken); break;
                                                }
                                            }
                                        default: TokenQueue.FrontLoadToken(nextToken); return new UnsignedLong(Type, longToken); break;
                                    }
                                }
                            default: TokenQueue.FrontLoadToken(nextToken); return new Unsigned(Type); break;
                        }
                    case "char": return new Char(Type);
                    case "float": return new Float(Type);
                    case "double": return new Double(Type);
                    case "void": return new Void(Type);
                    case "struct": return StructOrUnion.Parse(ParentScope, Context, Type, TokenQueue);
                    case "union": return StructOrUnion.Parse(ParentScope, Context, Type, TokenQueue);
                    case "enum": return Enum.Parse(ParentScope, Context, Type, TokenQueue);
                }
                return ParentScope.ResolveType(Type.Value);
            }
        }
    }
}
