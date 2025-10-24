using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.C
{
    public partial class Parser
    {
        public class Intrinsic : Function
        {
            internal Intrinsic(Attribute[] attributes, Type ReturnType, Token Name, FunctionParameter[] FunctionParameters, bool Variadic) : base(attributes, ReturnType, Name, FunctionParameters, Variadic)
            {
            }
        }
    }
}
