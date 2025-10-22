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
        public abstract class Break : Statement
        {
            Token _break;
            internal Break(Token @break)
            {
                _break = @break;
            }
            public override string ToString()
            {
                return "break;";
            }
            public class SwitchBreak : Break
            {
                Switch _parent;
                public Switch Switch { get { return _parent; } }
                internal SwitchBreak(Token @break, Switch parent) : base(@break)
                {
                    _parent = parent;
                }
            }
            public class ForBreak : Break
            {
                For _parent;
                public For For { get { return _parent; } }
                internal ForBreak(Token @break, For parent) : base(@break)
                {
                    _parent = parent;
                }
            }
            public class UnscopedForBreak : Break
            {
                UnscopedFor _parent;
                public UnscopedFor For { get { return _parent; } }
                internal UnscopedForBreak(Token @break, UnscopedFor parent) : base(@break)
                {
                    _parent = parent;
                }
            }
            public class WhileBreak : Break
            {
                While _parent;
                public While While { get { return _parent; } }
                internal WhileBreak(Token @break, While parent) : base(@break)
                {
                    _parent = parent;
                }
            }
            public class DoWhileBreak : Break
            {
                DoWhile _parent;
                public DoWhile DoWhile { get { return _parent; } }
                internal DoWhileBreak(Token @break, DoWhile parent) : base(@break)
                {
                    _parent = parent;
                }
            }
        }
    }
}