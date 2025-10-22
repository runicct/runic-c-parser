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
        class Identifier
        {
            internal static bool IsValid(Token Token, Parser.CStandardRevision standardRevision)
            {
                if (Token == null || Token.Value == null || Token.Value.Length == 0)
                {
                    return false;
                }

                if (char.IsDigit(Token.Value[0]))
                {
                    return false;
                }

                switch (Token.Value[0])
                {

                    case ';':
                    case ',':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '?':
                    case '.':
                    case '=':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '&':
                    case '|':
                    case '%':
                    case '^':
                    case '~':
                    case '!':
                    case '>':
                    case '<':
                    case '#':
                    case ':':
                    case '\0':
                    case ' ':
                    case '\'':
                    case '\"':
                        return false;
                }

                switch (Token.Value)
                {
                    case "auto":
                    case "break":
                    case "case":
                    case "char":
                    case "const":
                    case "continue":
                    case "default":
                    case "do":
                    case "double":
                    case "else":
                    case "enum":
                    case "extern":
                    case "float":
                    case "for":
                    case "goto":
                    case "if":
                    case "int":
                    case "long":
                    case "register":
                    case "return":
                    case "short":
                    case "signed":
                    case "sizeof":
                    case "static":
                    case "struct":
                    case "switch":
                    case "typedef":
                    case "union":
                    case "unsigned":
                    case "void":
                    case "volatile":
                    case "while":
                        return false;
                }

                if (standardRevision >= Parser.CStandardRevision.C11)
                {
                    switch (Token.Value)
                    {
                        case "_Static_assert": return false;
                    }
                }

                if (standardRevision >= Parser.CStandardRevision.C23)
                {
                    switch (Token.Value)
                    {
                        case "true":
                        case "false": return false;
                    }
                }

                // NOTE: We trust the lexer here and assume something like test+=2 would
                //       produce test | += | 2. Otherwise we would need to recheck the entire
                //       token value for the presence of invalid char
                return true;
            }
        }
    }
}
