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
        public class Scope : IScope
        {
            internal class FunctionScope : Scope
            {
                FunctionDefinition _functionDefinition;
                internal FunctionDefinition FunctionDefinition { get { return _functionDefinition; } }
                public FunctionScope(IScope ParentScope, Parser Context, FunctionDefinition FunctionDefinition) : base(ParentScope, Context)
                {
                    _functionDefinition = FunctionDefinition;
                }
                internal override Function? GetParentFunction()
                {
                    return _functionDefinition;
                }
                public override IScope? GetBreakContinueScope() { return null; }
            }
            internal class IfScope : Scope
            {
                If _if;
                internal If If { get { return _if; } }
                public IfScope(IScope ParentScope, Parser Context, If If) : base(ParentScope, Context)
                {
                    _if = If;
                }
            }
            internal class ElseScope : Scope
            {
                Else _else;
                internal Else Else { get { return _else; } }
                public ElseScope(IScope ParentScope, Parser Context, Else Else) : base(ParentScope, Context)
                {
                    _else = Else;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            internal class WhileScope : Scope
            {
                While _while;
                internal While While { get { return _while; } }
                public WhileScope(IScope ParentScope, Parser Context, While While) : base(ParentScope, Context)
                {
                    _while = While;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            internal class SwitchScope : Scope
            {
                Switch _switch;
                internal Switch Switch { get { return _switch; } }
                public SwitchScope(IScope ParentScope, Parser Context, Switch Switch) : base(ParentScope, Context)
                {
                    _switch = Switch;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            internal class DoWhileScope : Scope
            {
                DoWhile _doWhile;
                internal DoWhile DoWhile { get { return _doWhile; } }
                public DoWhileScope(IScope ParentScope, Parser Context, DoWhile DoWhile) : base(ParentScope, Context)
                {
                    _doWhile = DoWhile;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            internal class ForScope : Scope
            {
                For _for;
                internal For For { get { return _for; } }
                public ForScope(IScope ParentScope, Parser Context, For For) : base(ParentScope, Context)
                {
                    _for = For;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            internal class UnscopedForScope : Scope
            {
                UnscopedFor _for;
                internal UnscopedFor For { get { return _for; } }
                public UnscopedForScope(IScope ParentScope, Parser Context, UnscopedFor For) : base(ParentScope, Context)
                {
                    _for = For;
                }
                
                public bool CheckLiveness(Statement statement, Scope scope)
                {
                    if (_for == statement) { return true; }
                    if (scope != this) { return true; }
                    return false;
                }
                public override IScope? GetBreakContinueScope() { return this; }
            }
            Parser _context;
            public Parser Context { get { return _context; } }
            IScope _parentScope;
            public Scope(IScope ParentScope, Parser Context)
            {
                _parentScope = ParentScope;
                _context = Context;
            }
            Dictionary<string, Type> _types = new Dictionary<string, Type>();
            public virtual IScope? GetBreakContinueScope()
            {
                return _parentScope.GetBreakContinueScope();
            }
            internal void AddType(Token Name, Type Type, bool SkipRedefineError = false)
            {
                Type? previousType = null;
                if (_types.TryGetValue(Name.Value, out previousType))
                {
                    if (!SkipRedefineError)
                    {
                        _context.Error_TypeWasAlreadyDeclared(previousType, Name, Type);
                    }
                    return;
                }
                _types.Add(Name.Value, Type);
                Type.Enum.EnumDeclation? enumType = Type as Type.Enum.EnumDeclation;
                if (enumType != null)
                {
                    AddEnumValues(enumType);
                }
            }
            Dictionary<string, Type.Enum.Member> _enumMembers = new Dictionary<string, Type.Enum.Member>();
            internal void AddEnumValues(Type.Enum.EnumDeclation Enum)
            {
                foreach (Type.Enum.Member member in Enum.Members)
                {
                    Type.Enum.Member? existingMember = null;
                    if (_enumMembers.TryGetValue(member.Name.Value, out existingMember))
                    {
                        // Error
                    }
                    else
                    {
                        _enumMembers.Add(member.Name.Value, member);
                    }
                }
            }
            Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
            internal void AddVariable(Variable Variable)
            {
                Variable? previousVaraible = null;
                if (_variables.TryGetValue(Variable.Name.Value, out previousVaraible))
                {
                    GlobalVariable? globalVariable = Variable as GlobalVariable;
                    GlobalVariable? previousGlobalVariable = previousVaraible as GlobalVariable;
                    if (globalVariable != null && previousGlobalVariable != null)
                    {
                        if (previousGlobalVariable.Extern && !globalVariable.Extern)
                        {
                            // __TODO__ Check the type
                            _variables[Variable.Name.Value] = globalVariable;
                            return;
                        }
                        else if (globalVariable.Extern)
                        {
                            // __TODO__ Check the type
                            return;
                        }
                    }
                    _context.Error_VariableWasAlreadyDeclared(previousVaraible, Variable);
                    return;
                }
                _variables.Add(Variable.Name.Value, Variable);
            }
            Dictionary<string, Function> _functions = new Dictionary<string, Function>();
            internal void AddFunction(Function Function)
            {
                Function? previousFunction = null;
                if (_functions.TryGetValue(Function.Name.Value, out previousFunction))
                {
                    return;
                }
                _functions.Add(Function.Name.Value, Function);
            }
            internal virtual Function? GetParentFunction()
            {
                IScope? parentScope = ParentScope;
                while (parentScope != null)
                {
                    Scope? typedParentScope = _parentScope as Scope;
                    if (typedParentScope != null)
                    {
                        return typedParentScope.GetParentFunction();
                    }
                    else
                    {
                        parentScope = parentScope.ParentScope;
                    }
                }
                return null;
            }
            public IScope? ParentScope { get { return _parentScope; } }
            public Type? ResolveType(string Name)
            {
                Type? type = null;
                if (_types.TryGetValue(Name, out type))
                {
                    return type;
                }
                if (_parentScope != null)
                {
                    return _parentScope.ResolveType(Name);
                }
                return null;
            }
            public virtual Variable? ResolveVariable(string Name)
            {
                Variable? varaible = null;
                if (_variables.TryGetValue(Name, out varaible))
                {
                    return varaible;
                }
                if (_parentScope != null)
                {
                    return _parentScope.ResolveVariable(Name);
                }
                return null;
            }
            public Type.Enum.Member? ResolveEnumMember(string Name)
            {
                Type.Enum.Member? member = null;
                if (_enumMembers.TryGetValue(Name, out member))
                {
                    return member;
                }
                if (_parentScope != null)
                {
                    return _parentScope.ResolveEnumMember(Name);
                }
                return null;
            }
            public Function? ResolveFunction(string Name)
            {
                Function? function = null;
                if (_functions.TryGetValue(Name, out function))
                {
                    return function;
                }
                if (_parentScope != null)
                {
                    return _parentScope.ResolveFunction(Name);
                }
                return null;
            }
            public class Enter : Statement
            {
                protected Scope _scope;
                public Scope Scope { get { return _scope; } }
                Token _token;
                internal Enter(Token token, Scope scope)
                {
                    _scope = scope;
                }
                protected Enter()
                {
                    _scope = null;
                }
                public override string ToString()
                {
                    return "{";
                }
            }
            public class Exit : Statement
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                Token _token;
                internal Exit(Token token, Scope scope)
                {
                    _token = token;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitFunctionDefinition : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                FunctionDefinition _functionDefinition;
                public FunctionDefinition FunctionDefinition { get { return _functionDefinition; } }
                internal ExitFunctionDefinition(Token token, Scope scope, FunctionDefinition FunctionDefinition) : base (token, scope)
                {
                    _functionDefinition = FunctionDefinition;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitIf : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                If _if;
                public If If { get { return _if; } }
                internal ExitIf(Token token, Scope scope, If If) : base (token, scope)
                {
                    _if = If;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitElse : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                Else _else;
                public Else Else { get { return _else; } }
                internal ExitElse(Token token, Scope scope, Else Else) : base (token, scope)
                {
                    _else = Else;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitWhile : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                While _while;
                public While While { get { return _while; } }
                internal ExitWhile(Token token, Scope scope, While While) : base(token, scope)
                {
                    _while = While;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitSwitch : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                Switch _switch;
                public Switch Switch { get { return _switch; } }
                internal ExitSwitch(Token token, Scope scope, Switch Switch) : base(token, scope)
                {
                    _switch = Switch;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
            public class ExitDoWhile : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                DoWhile _doWhile;
                public DoWhile DoWhile { get { return _doWhile; } }
                Expression _condition;
                public Expression Condition { get { return _condition; } }
                internal ExitDoWhile(Token token, Scope scope, DoWhile DoWhile, Expression Condition) : base(token, scope)
                {
                    _doWhile = DoWhile;
                    _scope = scope;
                    _condition = Condition;
                }
                public override string ToString()
                {
                    return "} while (" + _condition + ");";
                }
            }
            public class ExitFor : Exit
            {
                Scope _scope;
                public Scope Scope { get { return _scope; } }
                For _for;
                public For For { get { return _for; } }
                internal ExitFor(Token token, Scope scope, For For) : base(token, scope)
                {
                    _for = For;
                    _scope = scope;
                }
                public override string ToString()
                {
                    return "}";
                }
            }
        }
    }
}
