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
        public class Attribute
        {
            public class Static : Attribute
            {
                internal Static(Token? name) : base(name) { }
                public override string ToString()
                {
                    return "static";
                }
            }
            public class Extern : Attribute
            {
                internal Extern(Token? name) : base(name) { }
                public override string ToString()
                {
                    return "extern";
                }
            }
            public class DllExport : Attribute
            {
                internal DllExport(Token? name) : base(name) { }
                public override string ToString()
                {
                    return "__declspec(dllexport)";
                }
            }
            public class DllImport : Attribute
            {
                internal DllImport(Token? name) : base(name) { }
                public override string ToString()
                {
                    return "__declspec(dllimport)";
                }
            }
            public class Deprecated : Attribute
            {
                internal Deprecated(Token? name) : base(name) { }
            }
            public class NoInline : Attribute
            {
                internal NoInline(Token? name) : base(name) { }
            }
            public class NoReturn : Attribute
            {
                internal NoReturn(Token? name) : base(name) { }
            }
            Token? _name;
            public Token? Name { get { return _name; } }
            internal Attribute(Token? name)
            {
                _name = name;
            }
            internal static Attribute[] Parse(Parser Context, TokenQueue tokenQueue)
            {
                List<Attribute> attributes = new List<Attribute>();
                Token? token = tokenQueue.ReadNextToken();
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
                        Token? attributeValue = tokenQueue.ReadNextToken();
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
