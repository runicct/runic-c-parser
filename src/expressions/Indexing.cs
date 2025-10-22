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
            public class Indexing : Expression
            {
                Expression _index;
                public Expression Index { get { return _index; } }
                Expression _target;
                public Expression Target { get { return _target; } }
                Type? _type;
                public Type? Type { get { return _type; } }
                internal Indexing(Token? Token, Expression Target, Expression Index)
                {
                    _target = Target;
                    _index = Index;
                    Type? targetType = Target.Type;
                    if (targetType != null)
                    {
                        switch (targetType)
                        {
                            case Type.Pointer pointer: _type = pointer.TargetType; break;
                            case Type.StaticArray array: _type = array.TargetType; break;
                            default: _type = new Type.Int(Token); break;
                        }
                    }
                }
                public override string ToString()
                {
                    return _target.ToString() + "[" + _index.ToString() + "]";
                }
            }
        }
    }
}
