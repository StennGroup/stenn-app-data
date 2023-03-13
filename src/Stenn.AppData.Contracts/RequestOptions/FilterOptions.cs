using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stenn.AppData.Contracts.RequestOptions
{
    public class Filter
    {
        public IToken CurrentToken { get; set; }
    }

    public interface IToken
    {
    }

    public class Condition : IToken
    {
        public string FieldName { get; set; }
        public object RightValue { get; set; }
        public ConditionType ConditionType { get; set; }
    }

    public class ContainsCondition : IToken
    {
        public string FieldName { get; set; }
        public object[] RightValue { get; set; }
    }

    public class And : IToken
    {
        public IToken First { get; set; }
        public IToken Second { get; set; }
    }

    public class Or : IToken{
        public IToken First { get; set; }
        public IToken Second { get; set; }
    }

    public enum ConditionType
    {
        Unspecified = 0,
        Equals = 1,
        Greater = 2,
        Less = 3,
    }
}
