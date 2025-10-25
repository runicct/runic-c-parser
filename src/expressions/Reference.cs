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
            public class Reference : Expression
            {
                Token _operator;
                public Token Operator { get { return _operator; } }
                internal Reference(Token op)
                {
                    _operator = op;
                }
                public override string ToString()
                {
                    return "(& unk)";
                }
            }
            public class VariableReference : Reference
            {
                Variable _variable;
                public Variable Variable { get { return _variable; } }
                Type _type;
#if NET6_0_OR_GREATER
                internal override Type? Type { get { return _type; } }
#else
                internal override Type Type { get { return _type; } }
#endif
                internal VariableReference(Token op, Variable variable) : base(op)
                {
                    _type = variable.Type.MakePointer(op);
                    _variable = variable;
                }
                public override string ToString()
                {
                    return "(&" + _variable.ToString() + ")";
                }
            }
            public class MemberReference : Reference
            {
                Variable _variable;
                public Variable Variable { get { return _variable; } }
                Field[] _fields;
                public Field[] Fields { get { return _fields; } }
                Type _type;
#if NET6_0_OR_GREATER
                internal override Type? Type { get { return _type; } }
#else
                internal override Type Type { get { return _type; } }
#endif
                internal MemberReference(Token op, Variable variable, Field[] fields) : base(op)
                {
                    _variable = variable;
                    _fields = fields;
                    if (_fields.Length > 0 && _fields[_fields.Length - 1] != null)
                    {
#if NET6_0_OR_GREATER
                        Type? fieldType = _fields[_fields.Length - 1].Type;
#else
                        Type fieldType = _fields[_fields.Length - 1].Type;
#endif
                        if (fieldType != null) { _type = fieldType.MakePointer(op); }
                        else { _type = variable.Type.MakePointer(op); }
                    }
                    else
                    {
                        _type = variable.Type.MakePointer(op);
                    }
                }
                public override string ToString()
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("(&");
                    stringBuilder.Append(_variable.Name);
                    for (int n = 0; n < _fields.Length; n++)
                    {
                        stringBuilder.Append(".");
                        stringBuilder.Append(_fields[n].Name);
                    }
                    stringBuilder.Append(")");
                    return stringBuilder.ToString();
                }
            }
            public class IndexingReference : Reference
            {
                Expression _target;
                public Expression Target { get { return _target; } }
                Expression _index;
                public Expression Index { get { return _index; } }
                Type _type;
#if NET6_0_OR_GREATER
                internal override Type? Type { get { return _type; } }
#else
                internal override Type Type { get { return _type; } }
#endif
                internal IndexingReference(Token op, Expression target, Expression index) : base(op)
                {
                    _target = target;
                    _index = index;
#if NET6_0_OR_GREATER
                    Type? targetType = target.Type;
#else
                    Type targetType = target.Type;
#endif
                    if (targetType != null)
                    {
                        switch (targetType)
                        {
                            case Type.Pointer pointer: _type = pointer.MakePointer(op); break;
                            case Type.StaticArray array: _type = array.MakePointer(op); break;
                            default: _type = (new Type.Void(op)).MakePointer(op); break;
                        }
                    }
                }
                public override string ToString()
                {
                    return "&(" + _target.ToString() + "[" + _index.ToString() + "])";
                }
            }

        }
    }
}