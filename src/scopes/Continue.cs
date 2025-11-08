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
        public abstract class Continue : Statement
        {
            Token _continue;
            public Token Keyword { get { return _continue; } }
            internal Continue(Token @continue)
            {
                _continue = @continue;
            }
            public override string ToString()
            {
                return "continue;";
            }
            public class SwitchContinue : Continue
            {
                Switch _parent;
                public Switch Switch { get { return _parent; } }
                internal SwitchContinue(Token @continue, Switch parent) : base(@continue)
                {
                    _parent = parent;
                }
            }
            public class ForContinue : Continue
            {
                For _parent;
                public For For { get { return _parent; } }
                internal ForContinue(Token @continue, For parent) : base(@continue)
                {
                    _parent = parent;
                }
            }
            public class UnscopedForContinue : Continue
            {
                UnscopedFor _parent;
                public UnscopedFor For { get { return _parent; } }
                internal UnscopedForContinue(Token @continue, UnscopedFor parent) : base(@continue)
                {
                    _parent = parent;
                }
            }
            public class WhileContinue : Continue
            {
                While _parent;
                public While While { get { return _parent; } }
                internal WhileContinue(Token @continue, While parent) : base(@continue)
                {
                    _parent = parent;
                }
            }
            public class DoWhileContinue : Continue
            {
                DoWhile _parent;
                public DoWhile DoWhile { get { return _parent; } }
                internal DoWhileContinue(Token @continue, DoWhile parent) : base(@continue)
                {
                    _parent = parent;
                }
            }
        }
    }
}