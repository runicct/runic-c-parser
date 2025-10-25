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
            public class Dereference : Expression
            {
                Expression _address;
                public Expression Address { get { return _address; } }
                Type _type;
#if NET6_0_OR_GREATER
                internal override Type? Type { get { return _type; } }
#else
                internal override Type Type { get { return _type; } }
#endif
                Token _operator;
                internal Token Operator { get { return _operator; } }
                internal Dereference(Token op, Expression address)
                {
                    _address = address;
                    _operator = op;
#if NET6_0_OR_GREATER
                    Type.Pointer? pointerType = address.Type as Type.Pointer;
#else
                    Type.Pointer pointerType = address.Type as Type.Pointer;
#endif
                    if (pointerType != null) { _type = pointerType.TargetType; }
                    else { _type = new Type.Int(op); }
                }
                public override string ToString()
                {
                    return "(*" + _address + ")";
                }
            }
        }
    }
}