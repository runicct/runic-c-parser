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
        public class FunctionDefinition : Function, IScope
        {
            Scope _body;
            internal Scope Body { get { return _body; } }
#if NET6_0_OR_GREATER
            public IScope? ParentScope { get { return _body.ParentScope; } }
#else
            public IScope ParentScope { get { return _body.ParentScope; } }
#endif

#if NET6_0_OR_GREATER
            public Type? ResolveType(string Name) { return _body.ResolveType(Name); }
#else
            public Type ResolveType(string Name) { return _body.ResolveType(Name); }
#endif

#if NET6_0_OR_GREATER
            public Variable? ResolveVariable(string Name)
#else
            public Variable ResolveVariable(string Name)
#endif
            {
                return _body.ResolveVariable(Name);
            }
#if NET6_0_OR_GREATER
            public Function? ResolveFunction(string Name)
#else
            public Function ResolveFunction(string Name)
#endif
            {
                if (Name == this.Name.Value) { return this; }
                return _body.ResolveFunction(Name);
            }
#if NET6_0_OR_GREATER
            public Type.Enum.Member? ResolveEnumMember(string Name)
#else
            public Type.Enum.Member ResolveEnumMember(string Name)
#endif
            {
                return _body.ResolveEnumMember(Name);
            }
#if NET6_0_OR_GREATER
            public IScope? GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#else
            public IScope GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#endif
            internal FunctionDefinition(IScope ParentScope, Parser Context, Attribute[] Attributes, Type ReturnType,  Token Name, FunctionParameter[] FunctionParameters, bool Variadic) : base (Attributes, ReturnType, Name, FunctionParameters, Variadic)
            {
                _body = new Scope.FunctionScope(ParentScope, Context, this);
                foreach (FunctionParameter parameter in FunctionParameters)
                {
                    if (parameter.Name != null)
                    {
                        _body.AddVariable(parameter);
                    }
                }
                _body.AddFunction(this);
            }
            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(ReturnType.ToString());
                builder.Append(" ");
                builder.Append(Name);
                builder.Append("(");
                for (int n = 0; n < FunctionParameters.Length; n++)
                {
                    if (n != 0) { builder.Append(", "); }
                    builder.Append(FunctionParameters[n].ToString());
                }
                builder.Append(") {");
                return builder.ToString();
            }


        }
    }
}
