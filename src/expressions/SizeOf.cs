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
        public abstract partial class Expression
        {
            public abstract class SizeOf : Expression
            {
                Token _sizeofKeyword;
                public Token Keyword { get { return _sizeofKeyword; } }
                Token _leftParenthesis;
                public Token LeftParenthesis { get { return _leftParenthesis; } }
                Token _rightParenthesis;
                public Token RightParenthesis { get { return _rightParenthesis; } }
                internal SizeOf(Token sizeofKeyword, Token leftParenthesis, Token rightParenthesis)
                {
                    _sizeofKeyword = sizeofKeyword;
                    _leftParenthesis = leftParenthesis;
                    _rightParenthesis = rightParenthesis;
                }
                public class SizeOfExpression : SizeOf
                {
                    Expression _expression;
                    public Expression Expression { get { return _expression; } }

                    internal SizeOfExpression(Token sizeofKeyword, Token leftParenthesis, Expression Expression, Token rightParenthesis) : base(sizeofKeyword, leftParenthesis, rightParenthesis)
                    {
                        _expression = Expression;
                    }
                    public override string ToString()
                    {
                        return "sizeof(" + _expression + ")";
                    }
                }
                public class SizeOfType : SizeOf
                {
                    Type _targetType;
                    public Type TargetType { get { return _targetType; } }
                    internal SizeOfType(Token sizeofKeyword, Token leftParenthesis, Type type, Token rightParenthesis) : base(sizeofKeyword, leftParenthesis, rightParenthesis)
                    {
                        _targetType = type;
                    }
                    public override string ToString()
                    {
                        return "sizeof(" + _targetType.ToString() + ")";
                    }
                }
            }
        }
    }
}