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
        public class Field
        {
            Type? _type;
            public Type? Type { get { return _type; } }
            Token? _name;
            public Token? Name { get { return _name; } }
            uint _index;
            public uint Index { get { return _index; } }
            internal Field(Type? Type, Token? Name, uint Index) { _type = Type; _name = Name; _index = Index; }

            public override string ToString()
            {
                if (_name == null)
                {
                    Type.StructOrUnion structOrUnion = _type as Type.StructOrUnion;
                    if (structOrUnion != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        if (structOrUnion.Union) { builder.AppendLine("union {"); }
                        else { builder.AppendLine("struct {"); }
                        foreach (Field field in structOrUnion.Fields)
                        {
                            builder.Append(field.ToString());
                            builder.AppendLine(";");
                        }
                        builder.Append("}");
                        return builder.ToString();
                    }
                    return "<unk>";
                }
                if (_type == null) { return _name.ToString(); }
                switch (_type)
                {
                    case Type.FunctionPointerType functionPointerType: return functionPointerType.ToString();
                    case Type.StructOrUnion structOrUnion:
                        if (structOrUnion.Declaration == structOrUnion && structOrUnion.Name == _name)
                        {
                            return _type.ToString();
                        }
                        return _type.ToString() + " " + _name.ToString();
                    default:
                        return _type.ToString() + " " + _name.ToString();
                }
            }
        }
    }
}
