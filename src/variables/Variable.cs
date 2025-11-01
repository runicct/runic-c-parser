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
        public abstract class Variable : Expression
        {
            Type _Type;
            public Type VariableType { get { return _Type; } }
            internal override Type Type { get { return _Type; } }
#if NET6_0_OR_GREATER
            Token? _Name;
            public Token? Name { get { return _Name; } }
#else
            Token _Name;
            public Token Name { get { return _Name; } }
#endif
            Attribute[] _Attributes;
            public Attribute[] Attributes { get { return _Attributes; } }
            bool _extern = false;
            public bool Extern { get { return _extern; } }
            bool _static = false;
            public bool Static { get { return _static; } }
#if NET6_0_OR_GREATER
            internal Variable(Attribute[] attributes, Type type, Token? name)
#else
            internal Variable(Attribute[] attributes, Type type, Token name)
#endif
            {
                _Attributes = attributes;
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is Attribute.Extern) { _extern = true; }
                    if (attribute is Attribute.Static) { _static = true; }
                }
                _Type = type;
                _Name = name;
            }
        }
    }
}
