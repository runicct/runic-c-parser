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
        public class Attribute
        {
            public class Static : Attribute
            {
#if NET6_0_OR_GREATER
                internal Static(Token? name) : base(name) { }
#else
                internal Static(Token name) : base(name) { }
#endif
                public override string ToString()
                {
                    return "static";
                }
            }
            public class Extern : Attribute
            {
#if NET6_0_OR_GREATER
                internal Extern(Token? name) : base(name) { }
#else
                internal Extern(Token name) : base(name) { }
#endif
                public override string ToString()
                {
                    return "extern";
                }
            }
            public class DllExport : Attribute
            {
#if NET6_0_OR_GREATER
                internal DllExport(Token? name) : base(name) { }
#else
                internal DllExport(Token name) : base(name) { }
#endif
                public override string ToString()
                {
                    return "__declspec(dllexport)";
                }
            }
            public class DllImport : Attribute
            {
#if NET6_0_OR_GREATER
                internal DllImport(Token? name) : base(name) { }
#else
                internal DllImport(Token name) : base(name) { }
#endif
                public override string ToString()
                {
                    return "__declspec(dllimport)";
                }
            }
            public class Deprecated : Attribute
            {
#if NET6_0_OR_GREATER
                internal Deprecated(Token? name) : base(name) { }
#else
                internal Deprecated(Token name) : base(name) { }
#endif
            }
            public class NoInline : Attribute
            {
#if NET6_0_OR_GREATER
                internal NoInline(Token? name) : base(name) { }
#else
                internal NoInline(Token name) : base(name) { }
#endif
            }
            public class NoReturn : Attribute
            {
#if NET6_0_OR_GREATER
                internal NoReturn(Token? name) : base(name) { }
#else
                internal NoReturn(Token name) : base(name) { }
#endif
            }
#if NET6_0_OR_GREATER
            Token? _name;
            public Token? Name { get { return _name; } }
#else
            Token _name;
            public Token Name { get { return _name; } }
#endif
#if NET6_0_OR_GREATER
            internal Attribute(Token? name)
#else
            internal Attribute(Token name)
#endif
            {
                _name = name;
            }
            internal static Attribute[] Parse(Parser Context, TokenQueue tokenQueue)
            {
                List<Attribute> attributes = new List<Attribute>();
#if NET6_0_OR_GREATER
                Token? token = tokenQueue.ReadNextToken();
#else
                Token token = tokenQueue.ReadNextToken();
#endif
                while (true)
                {
                    if (token == null)
                    {
                        return new Attribute[0];
                    }
                    else if(token.Value == "static")
                    {
                        attributes.Add(new Static(token));
                    }
                    else if(token.Value == "extern")
                    {
                        attributes.Add(new Extern(token));
                    }
                    else if (Context.AllowMSVCExtensions && token.Value == "__declspec")
                    {
                        Token declspecToken = token;
                        token = tokenQueue.ReadNextToken();
                        if (token == null)
                        {
                            Context.Error_InvalidFunctionAttribute(declspecToken, null);
                            return null;
                        }
                        if (token.Value != "(")
                        {
                            tokenQueue.FrontLoadToken(token);
                            Context.Error_InvalidFunctionAttribute(declspecToken, null);
                            return null;
                        }
#if NET6_0_OR_GREATER
                        Token? attributeValue = tokenQueue.ReadNextToken();
#else
                        Token attributeValue = tokenQueue.ReadNextToken();
#endif
                        if (attributeValue == null)
                        {
                            Context.Error_InvalidFunctionAttribute(declspecToken, null);
                            return null;
                        }
                        while (true)
                        {
                            switch (attributeValue.Value)
                            {
                                case "dllimport": attributes.Add(new DllImport(token)); break;
                                case "dllexport": attributes.Add(new DllExport(token)); break;
                                case "deprecated": attributes.Add(new Deprecated(token)); break;
                                case "noinline": attributes.Add(new NoInline(token)); break;
                                case "noreturn": attributes.Add(new NoReturn(token)); break;
                                default:
                                    Context.Error_InvalidFunctionAttribute(declspecToken, attributeValue);
                                    break;
                            }

                            token = tokenQueue.ReadNextToken();
                            if (token == null)
                            {
                                Context.Error_InvalidFunctionAttribute(declspecToken, null);
                                return null;
                            }
                            if (token.Value == ")")
                            {
                                break;
                            }
                            attributeValue = token;
                        }
                    }
                    else
                    {
                        tokenQueue.FrontLoadToken(token);
                        return attributes.ToArray();
                    }
                    token = tokenQueue.ReadNextToken();
                }

                return null;
            }

        }
    }
}
