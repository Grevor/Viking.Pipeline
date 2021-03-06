﻿using System.Collections.Generic;

namespace Viking.Pipeline.CodeGenerator
{
    public class Parameter
    {
        public Parameter(string number, string typeParameterName, string constructorParameterName, string classFieldName)
        {
            Number = number;
            TypeParameterName = typeParameterName;
            ConstructorParameterName = constructorParameterName;
            ClassFieldName = classFieldName;
        }

        public string Number { get; }
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
