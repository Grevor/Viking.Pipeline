using System.Collections.Generic;
using System.Linq;

namespace Viking.Pipeline.CodeGenerator
{
    public class PepeHands : IGenerator
    {
        public string ClassName { get; } = "";
        private List<Parameter> Parameters { get; }

        public PepeHands(int parameters) => Parameters = Enumerable.Range(1, parameters).Select(Get).ToList();

        public static Parameter Get(int p)
        {
            var s = p.ToString();
            var n = "Input" + s;
            return new Parameter("T" + n, "input" + s, n);
        }

        public IEnumerable<Parameter> GetParameters() => Parameters;
    }
}
