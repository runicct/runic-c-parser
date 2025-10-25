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
        public class Return : Statement
        {
#if NET6_0_OR_GREATER
            Expression? _value;
            public Expression? Value { get { return _value; } }
#else
            Expression _value;
            public Expression Value { get { return _value; } }
#endif
            Token _returnToken;
            public Token Keyword { get { return _returnToken; } }
#if NET6_0_OR_GREATER
            internal Return(IScope parentScope, Parser context, Token returnToken, Expression? returnValue)
#else
            internal Return(IScope parentScope, Parser context, Token returnToken, Expression returnValue)
#endif
            {
                _returnToken = returnToken;
                _value = returnValue;
            }
            public override string ToString()
            {
                if (_value == null)
                {
                    return "return;";
                }
                return "return " + _value.ToString();
            }
        }
    }
}
