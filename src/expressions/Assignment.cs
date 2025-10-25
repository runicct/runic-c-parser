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
            public abstract class Assignment : Expression
            {
                public class VariableAssignment : Assignment
                {
#if NET6_0_OR_GREATER
                    Token? _op;
                    Variable? _target;
                    public Variable? Target { get { return _target; } }
#else
                    Token _op;
                    Variable _target;
                    public Variable Target { get { return _target; } }
#endif

#if NET6_0_OR_GREATER
                    internal VariableAssignment(Token? op, Variable? target, Expression value) : base(op, value)
#else
                    internal VariableAssignment(Token op, Variable target, Expression value) : base(op, value)
#endif
                    {
                        _op = op;
                        _target = target;
                    }
                    public override string ToString()
                    {
                        if (_target != null) { return _target.Name + " = " + _value; }
                        return _value.ToString();
                    }
                }
                public class MemberAssignment : Assignment
                {
#if NET6_0_OR_GREATER
                    Variable? _target;
                    public Variable? Target { get { return _target; } }
                    Field[]? _fields;
                    public Field[] Fields { get { return _fields; } }
#else
                    Variable _target;
                    public Variable Target { get { return _target; } }
                    Field[] _fields;
                    public Field[] Fields { get { return _fields; } }
#endif
#if NET6_0_OR_GREATER
                    internal MemberAssignment(Token? op, Variable? variable, Field[] fields, Expression value) : base(op, value)
#else
                    internal MemberAssignment(Token op, Variable variable, Field[] fields, Expression value) : base(op, value)
#endif
                    {
                        _target = variable;
                        _fields = fields;
                    }
                    public override string ToString()
                    {
                        if (_target != null) { return _target.Name + " = " + _value; }
                        return _value.ToString();
                    }
                }
                public class IndexingAssignment : Assignment
                {
                    Expression _target;
                    public Expression Target { get { return _target; } }
                    Expression _index;
                    public Expression Index { get { return _index; } }
#if NET6_0_OR_GREATER
                    internal IndexingAssignment(Token? op, Expression target, Expression index, Expression value) : base(op, value)
#else
                    internal IndexingAssignment(Token op, Expression target, Expression index, Expression value) : base(op, value)
#endif
                    {
                        _target = target;
                        _index = index;
                    }
                    public override string ToString()
                    {
                        return _target.ToString() + "[" + _index.ToString() + "] = " + _value;
                    }
                }
                public class DereferenceAssignment : Assignment
                {
#if NET6_0_OR_GREATER

                    Token? _derefOp;
                    Expression _target;
                    public Expression Target { get { return _target; } }
#else

                    Token _derefOp;
                    Expression _target;
                    public Expression Target { get { return _target; } }
#endif
#if NET6_0_OR_GREATER
                    internal DereferenceAssignment(Token? op, Token? derefOp, Expression target, Expression value) : base(op, value)
#else
                    internal DereferenceAssignment(Token op, Token derefOp, Expression target, Expression value) : base(op, value)
#endif
                    {
                        _derefOp = derefOp;
                        _target = target;
                    }
                    public override string ToString()
                    {
                        return "(*" + _target.ToString() + ") = " + _value.ToString();
                    }
                }
                Expression _value;
                public Expression Value { get { return _value; } }
                Token _operator;
                public Token Operator { get { return _operator; } }
                internal Assignment(Token op, Expression value)
                {
                    _operator = op;
                    _value = value;
                }

            }
        }
    }
}