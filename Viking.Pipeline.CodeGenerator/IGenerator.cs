using System.Collections.Generic;

namespace Viking.Pipeline.CodeGenerator
{
    public class Parameter
    {
        public Parameter(string typeParameterName, string constructorParameterName, string classFieldName)
        {
            TypeParameterName = typeParameterName;
            ConstructorParameterName = constructorParameterName;
            ClassFieldName = classFieldName;
        }

        public string TypeParameterName { get; }
        public string ConstructorParameterName { get; }
        public string ClassFieldName { get; }
    }

    public interface IGenerator
    {
        string ClassName { get; }
        IEnumerable<Parameter> GetParameters();
    }
}
