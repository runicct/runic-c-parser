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
            public abstract class Increment : Expression
            {
                Token? _operator;
                internal Increment(Token? op)
                {
                    _operator = op;
                }
                public abstract class Postfix : Increment
                {
                    public class Variable : Postfix
                    {
                        Parser.Variable _target;
                        public Parser.Variable Target { get { return _target; } }
                        internal Variable(Token? op, Parser.Variable target) : base(op)
                        {
                            _target = target;
                        }
                        public override string ToString()
                        {
                            return _target.Name + "++";
                        }
                    }

                    public class Dereference : Postfix
                    {
                        Expression _address;
                        public Expression Address { get { return _address; } }
                        internal Dereference(Token? op, Expression address) : base(op)
                        {
                            _address = address;
                        }
                        public override string ToString()
                        {
                            return "(*" + "(" + _address.ToString() + "))++";
                        }
                    }
                    public class Indexing : Postfix
                    {
                        Expression _target;
                        public Expression Target { get { return _target; } }
                        Expression _index;
                        public Expression Index { get { return _index; } }

                        internal Indexing(Token? op, Expression target, Expression index) : base(op)
                        {
                            _target = target;
                            _index = index;
                        }
                        public override string ToString()
                        {
                            return "(" + _target.ToString() + "[" + _index.ToString() + "])++";
                        }
                    }
                    public class Member : Postfix
                    {
                        Parser.Variable _target;
                        public Parser.Variable Target { get { return _target; } }
                        Field[] _fields;
                        public Field[] Fields { get { return _fields; } }

                        internal Member(Token? op, Parser.Variable variable, Field[] fields) : base(op)
                        {
                            _target = variable;
                            _fields = fields;
                        }
                    }
                    public Postfix(Token? op) : base (op)
                    {

                    }

                }
                public abstract class Prefix : Increment
                {
                    public class Variable : Prefix
                    {
                        Parser.Variable _target;
                        public Parser.Variable Target { get { return _target; } }
                        internal Variable(Token? op, Parser.Variable target) : base(op)
                        {
                            _target = target;
                        }
                        public override string ToString()
                        {
                            return "++" + _target.Name;
                        }
                    }
                    public class Dereference : Prefix
                    {
                        Expression _address;
                        public Expression Address { get { return _address; } }
                        internal Dereference(Token? op, Expression address) : base(op)
                        {
                            _address = address;
                        }
                        public override string ToString()
                        {
                            return "++(*" + "(" + _address.ToString() + "))";
                        }
                    }
                    public class Indexing : Prefix
                    {
                        Expression _target;
                        public Expression Target { get { return _target; } }
                        Expression _index;
                        public Expression Index { get { return _index; } }

                        internal Indexing(Token? op, Expression target, Expression index) : base(op)
                        {
                            _target = target;
                            _index = index;
                        }
                        public override string ToString()
                        {
                            return "++(" + _target.ToString() + "[" + _index.ToString() + "])";
                        }
                    }
                    public class Member : Prefix
                    {
                        Parser.Variable _target;
                        public Parser.Variable Target { get { return _target; } }
                        Field[] _fields;
                        public Field[] Fields { get { return _fields; } }

                        internal Member(Token? op, Parser.Variable variable, Field[] fields) : base(op)
                        {
                            _target = variable;
                            _fields = fields;
                        }
                    }
                    public Prefix(Token? op) : base(op)
                    {

                    }
                }
            }
        }
    }
}