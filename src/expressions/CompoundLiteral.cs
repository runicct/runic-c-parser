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

using System.Collections.Generic;
using System.Text;
using static Runic.C.Parser.Type.StructOrUnion;

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Expression
        {
            public class CompoundLiteralsFields : Expression
            {
                Dictionary<string, Expression> _values;
                public IReadOnlyDictionary<string, Expression> Values { get { return _values; } }
                Token _op;
                public CompoundLiteralsFields(Token op, Dictionary<string, Expression> values) { _op = op; _values = values; }
                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("{");
                    bool first = true;
                    foreach (KeyValuePair<string, Expression> value in _values)
                    {
                        if (!first) { builder.Append(", "); } else { first = false; }
                        builder.Append("." + value.Key + " = ");
                        builder.Append(value.Value.ToString());
                    }
                    builder.Append("}");
                    return builder.ToString();
                }
            }
            internal class CompoundLiteralsList : Expression
            {
                Expression[] _values;
                public Expression[] Values { get { return _values; } }
#if NET6_0_OR_GREATER
                Token? _op;
                internal Token? Op { get { return _op; } }
#else
                Token _op;
                internal Token Op { get { return _op; } }
#endif
#if NET6_0_OR_GREATER
                public CompoundLiteralsList(Token? op, Expression[] values) { _op = op; _values = values; }
#else
                public CompoundLiteralsList(Token op, Expression[] values) { _op = op; _values = values; }
#endif
                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("{");
                    for (int n = 0; n < _values.Length; n++)
                    {
                        if (n != 0) { builder.Append(", "); }
                        builder.Append(_values[n].ToString());
                    }
                    builder.Append("}");
                    return builder.ToString();
                }
            }
            public class CompoundLiteralsStruct : Expression
            {
                Dictionary<Field, Expression> _fields;
                public Dictionary<Field, Expression> Values { get { return _fields; } }
                Type.StructOrUnion _type;
                internal override Type Type { get { return _type; } }
                public Type.StructOrUnion StructType { get { return _type; } }
                CompoundLiteralsList _literalsList;
                internal CompoundLiteralsStruct(CompoundLiteralsList literalsList, Dictionary<Field, Expression> initialization, Type.StructOrUnion type)
                {
                    _literalsList = literalsList;
                    _fields = initialization;
                    _type = type;
                }
#if NET6_0_OR_GREATER
                static Type.StructOrUnion? ResolveType(Type.StructOrUnion? structOrUnion)
#else
                static Type.StructOrUnion ResolveType(Type.StructOrUnion structOrUnion)
#endif
                {
                    if (structOrUnion == null) { return null; }
                    while (structOrUnion.Declaration != null && structOrUnion.Declaration != structOrUnion)
                    {
                        structOrUnion = structOrUnion.Declaration;
                    }
                    if (structOrUnion.Declaration == null && structOrUnion.Name != null && structOrUnion.ParentScope != null)
                    {
#if NET6_0_OR_GREATER
                        IScope? scope = structOrUnion.ParentScope;
                        Type? type = scope.ResolveType(structOrUnion.Name.Value);
#else
                        IScope scope = structOrUnion.ParentScope;
                        Type type = scope.ResolveType(structOrUnion.Name.Value);
#endif
                        if (type == null) { return null; }
                        return type as Type.StructOrUnion;
                    }
                    return structOrUnion as Type.StructOrUnion.StructOrUnionDeclaration;
                }
                internal static CompoundLiteralsStruct Create(Parser context, CompoundLiteralsFields compoundLiteralsFields, Type.StructOrUnion type)
                {
                    return null;
                }
                internal static CompoundLiteralsStruct Create(Parser context, CompoundLiteralsList compoundLiteralsList, Type.StructOrUnion type, bool final, ref int listIndex)
                {
#if NET6_0_OR_GREATER
                    Type.StructOrUnion? structOrUnionTypeDefinition = ResolveType(type);
#else
                    Type.StructOrUnion structOrUnionTypeDefinition = ResolveType(type);
#endif
                    if (structOrUnionTypeDefinition == null)
                    {
                        context.Error_UseOfIncompleteTypeInCompoundLiteral(compoundLiteralsList.Op, type);
                        return new CompoundLiteralsStruct(compoundLiteralsList, new Dictionary<Field, Expression>(), type);
                    }
                    type = structOrUnionTypeDefinition;

                    Dictionary<Field, Expression> result = new Dictionary<Field, Expression>();
                    if (type.Union)
                    {
                        if (listIndex < compoundLiteralsList.Values.Length)
                        {
                            // Grab the first field
                            if (type.Fields != null && type.Fields.Length > 0)
                            {
                                Expression expression = compoundLiteralsList.Values[listIndex];
#if NET6_0_OR_GREATER
                                CompoundLiteralsList? compoundList = expression as CompoundLiteralsList;
                                CompoundLiteralsFields? compoundFields = expression as CompoundLiteralsFields;
                                Type.StructOrUnion? structType = type.Fields[0].Type as Type.StructOrUnion;
#else
                                CompoundLiteralsList compoundList = expression as CompoundLiteralsList;
                                CompoundLiteralsFields compoundFields = expression as CompoundLiteralsFields;
                                Type.StructOrUnion structType = type.Fields[0].Type as Type.StructOrUnion;
#endif

                                if (compoundList != null)
                                {
                                    if (structType == null) { /* Error */ }
                                    else { result.Add(type.Fields[0], Create(context, compoundList, structType)); listIndex++; }
                                }
                                else if (compoundFields != null)
                                {
                                    if (structType == null) { /* Error */ }
                                    else { result.Add(type.Fields[0], Create(context, compoundFields, structType)); listIndex++; }
                                }
                                else
                                {
                                    if (structType != null)
                                    {
                                        result.Add(type.Fields[0], Create(context, compoundLiteralsList, structType, false, ref listIndex));
                                    }
                                    else
                                    {
                                        result.Add(type.Fields[0], expression);
                                        listIndex++;
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        int fieldIndex = 0;
                        for (; listIndex < compoundLiteralsList.Values.Length; fieldIndex++)
                        {
                            if (fieldIndex >= type.Fields.Length)
                            {
                                if (final)
                                {
                                    context.Warning_ExtraFieldInStructInitialization(compoundLiteralsList.Values[fieldIndex]);
                                }
                                break;
                            }
                            Expression expression = compoundLiteralsList.Values[listIndex];
#if NET6_0_OR_GREATER
                            CompoundLiteralsList? compoundList = expression as CompoundLiteralsList;
                            CompoundLiteralsFields? compoundFields = expression as CompoundLiteralsFields;
                            Type.StructOrUnion? structType = type.Fields[fieldIndex].Type as Type.StructOrUnion;
#else
                            CompoundLiteralsList compoundList = expression as CompoundLiteralsList;
                            CompoundLiteralsFields compoundFields = expression as CompoundLiteralsFields;
                            Type.StructOrUnion structType = type.Fields[fieldIndex].Type as Type.StructOrUnion;
#endif
                            if (compoundList != null)
                            {
                                if (structType == null) { /* Error */ }
                                else { result.Add(type.Fields[fieldIndex], Create(context, compoundList, structType)); listIndex++; }
                            }
                            else if (compoundFields != null)
                            {
                                if (structType == null) { /* Error */ }
                                else { result.Add(type.Fields[fieldIndex], Create(context, compoundFields, structType)); listIndex++; }
                            }
                            else
                            {
                                if (structType != null)
                                {
                                    result.Add(type.Fields[fieldIndex], Create(context, compoundLiteralsList, structType, false, ref listIndex));
                                }
                                else
                                {
                                    result.Add(type.Fields[fieldIndex], expression);
                                    listIndex++;
                                }
                            }
                        }
                    }
                    return new CompoundLiteralsStruct(compoundLiteralsList, result, type);
                }
                internal static CompoundLiteralsStruct Create(Parser context, CompoundLiteralsList compoundLiteralsList, Type.StructOrUnion type)
                {
                    int listIndex = 0;
                    return Create(context, compoundLiteralsList, type, true, ref listIndex);
                } 
                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("{");
                    bool first = true;
                    foreach (KeyValuePair<Field, Expression> value in _fields)
                    {
                        if (!first) { builder.Append(", "); } else { first = false; }
                        builder.Append("." + value.Key.Name + " = ");
                        builder.Append(value.Value.ToString());
                    }
                    builder.Append("}");
                    return builder.ToString();
                }
            }
        }
    }
}
