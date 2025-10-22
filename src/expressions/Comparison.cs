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
            public class Comparison : Expression
            {
                public enum ComparisonOperation
                {
                    Equal = 0,
                    NotEqual = 1,
                    LowerThan = 2,
                    GreaterThan = 3,
                    LowerOrEqual = 4,
                    GreaterOrEqual = 5,
                }
                Expression _left;
                public Expression Left { get { return _left; } }
                Expression _right;
                public Expression Right { get { return _right; } }
                Token? _operator;
                public Token? Operator { get { return _operator; } }
                ComparisonOperation _operation;
                public ComparisonOperation Operation { get { return _operation; } }
                internal Comparison(ComparisonOperation operation, Token? opToken, Expression left, Expression right)
                {
                    _left = left;
                    _right = right;
                    _operator = opToken;
                    _operation = operation;
                }
                public override string ToString()
                {
                    switch (_operation)
                    {
                        case ComparisonOperation.Equal: return "(" + _left + ") == (" + _right + ")";
                        case ComparisonOperation.NotEqual: return "(" + _left + ") != (" + _right + ")";
                        case ComparisonOperation.LowerThan: return "(" + _left + ") < (" + _right + ")";
                        case ComparisonOperation.LowerOrEqual: return "(" + _left + ") <= (" + _right + ")";
                        case ComparisonOperation.GreaterThan: return "(" + _left + ") > (" + _right + ")";
                        case ComparisonOperation.GreaterOrEqual: return "(" + _left + ") >= (" + _right + ")";
                    }
                    return "(" + _left + ") == (" + _right + ")";
                }
            }
        }
    }
}