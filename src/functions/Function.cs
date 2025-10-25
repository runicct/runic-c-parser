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
        public abstract class Function : Statement
        {
            bool _variadic;
            public bool Variadic { get { return _variadic; } }
            Attribute[] _attributes;
            public Attribute[] Attributes { get { return _attributes; } }
            Type _returnType;
            public Type ReturnType { get { return _returnType; } }
            Token _name;
            public Token Name { get { return _name; } }
            FunctionParameter[] _parameters;
            public FunctionParameter[] FunctionParameters { get { return _parameters; } }
            Type _type;
            public Type Type { get { return _type; } }
            bool _extern = false;
            public bool Extern { get { return _extern; } }
            bool _static = false;
            public bool Static { get { return _static; } }
            ulong _localIndex = 0;
            internal ulong GetNextLocalIndex()
            {
                lock (this) { return _localIndex++; }
            }
            ulong _labelIndex = 0;
            Dictionary<string, Label> _labels = new Dictionary<string, Label>();
#if NET6_0_OR_GREATER
            internal Label GetOrDeclareLabel(Token? name)
#else
            internal Label GetOrDeclareLabel(Token name)
#endif
            {
                lock (this)
                {
                    if ((name == null) || (name.Value == null))
                    {
                        return new Label(null, this, _labelIndex++);
                    }
#if NET6_0_OR_GREATER
                    Label? label;
#else
                    Label label;
#endif
                    if (_labels.TryGetValue(name.Value, out label))
                    {
                        return label;
                    }
                    else
                    {
                        label = new Label(name, this, _labelIndex++);
                        _labels.Add(name.Value, label);
                        return label;
                    }
                }
            }
            internal Function(Attribute[] attributes, Type ReturnType, Token Name, FunctionParameter[] FunctionParameters, bool Variadic)
            {
                _attributes = attributes;
                foreach (Attribute attribute in attributes) 
                {
                    if (attribute is Attribute.Extern) { _extern = true; }
                    if (attribute is Attribute.Static) { _static = true; }
                }
                _returnType = ReturnType;
                _parameters = FunctionParameters;
                _name = Name;
                _variadic = Variadic;
                _type = new Type.FunctionPointerType(ReturnType, Name, FunctionParameters);
            }
        }
    }
}
