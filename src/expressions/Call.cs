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
        public abstract partial class Expression
        {
            public class Call : Expression
            {
                Function _function;
                public Function Function { get { return _function; } }
                Expression[] _paramters;
                public Expression[] Parameters { get { return _paramters; } }
                Token _leftParenthesis;
                Token _rightParenthesis;
                internal Call(Function function, Token leftParenthesis, Expression[] parameter, Token rightParenthesis)
                {
                    _function = function;
                    _paramters = parameter;
                    _leftParenthesis = leftParenthesis;
                    _rightParenthesis = rightParenthesis;
                }
                public override string ToString() 
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int n = 0; n < _paramters.Length; n++)
                    {
                        if (n != 0)
                        {
                            stringBuilder.Append(", ");
                        }
                        stringBuilder.Append(_paramters[n].ToString());
                    }
                    return _function.Name + "(" + stringBuilder.ToString() + ")";
                }
            }
        }
    }
}