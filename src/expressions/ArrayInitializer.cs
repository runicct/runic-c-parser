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
            public class ArrayInitializer : Expression
            {
                Expression[] _values;
                public Expression[] Values { get { return _values; } }
                Token _operator;
                public Token Operator { get { return _operator; } }
#if NET6_0_OR_GREATER
                Type.StaticArray? _type;
                internal override Type? Type { get { return _type; } }
                public Type.StaticArray? ArrayType { get { return _type; } }
#else
                Type.StaticArray _type;
                internal override Type Type { get { return _type; } }
                public Type.StaticArray ArrayType { get { return _type; } }
#endif

#if NET6_0_OR_GREATER
                internal ArrayInitializer(Token op, Type.StaticArray? type, Expression[] values)
#else
                internal ArrayInitializer(Token op, Type.StaticArray type, Expression[] values)
#endif
                {
                    _type = type;
                    _values = values;
                }
            }
        }
    }
}