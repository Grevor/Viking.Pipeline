using System;
using System.Linq;

namespace Viking.Pipeline.CodeGenerator
{
    public class Generator
    {
        public const string GenericParametersPlaceholder = "$GenericParameters$";
        public const string ConstructorParametersPlaceholder = "$ConstructorParameters$";
        public const string ConstructorAssignmentsPlaceholder = "$ConstructorAssignment$";
        public const string RepassingParametersPlaceholder = "$RepassingParameters$";
        public const string GettersPlaceholder = "$Getters$";
        public const string ClassFieldsPlaceholder = "$ClassFields$";


        private string GenericParameters { get; }
        private string ConstructorParameters { get; }
        private string ConstructorAssignments { get; }
        private string RepassingParameters { get; }
        private string Getters { get; }
        private string ClassFields { get; }

        public Generator(IGenerator gen)
        {
            var parameters = gen.GetParameters().ToList();
            var hasParameters = parameters.Any();

            GenericParameters = hasParameters ? string.Join(", ", parameters.Select(p => p.TypeParameterName)) : "";
            ConstructorParameters = hasParameters ? string.Join("," + Environment.NewLine + "\t\t\t", parameters.Select(GetConstructorParameter)) : "";
            ConstructorAssignments = hasParameters ? string.Join(Environment.NewLine + "\t\t\t", parameters.Select(GetConstructorAssignment)) : "";
            RepassingParameters = hasParameters ? string.Join(", ", parameters.Select(p => p.ConstructorParameterName)) : "";
            Getters = hasParameters ? string.Join(", ", parameters.Select(GetValueGetter)) : "";
            ClassFields = hasParameters ? string.Join(Environment.NewLine + "\t\t", parameters.Select(GetClassProperty)) : "";
        }

        public string GetString(string template)
        {
            return template
                .Replace(GenericParametersPlaceholder, GenericParameters)
                .Replace(ConstructorParametersPlaceholder, ConstructorParameters)
                .Replace(ConstructorAssignmentsPlaceholder, ConstructorAssignments)
                .Replace(RepassingParametersPlaceholder, RepassingParameters)
                .Replace(GettersPlaceholder, Getters)
                .Replace(ClassFieldsPlaceholder, ClassFields);
        }


        private static string GetFieldType(Parameter parameter) => $"IPipelineStage<{parameter.TypeParameterName}>";
        private static string GetConstructorParameter(Parameter parameter) => $"{GetFieldType(parameter)} {parameter.ConstructorParameterName}";
        private static string GetClassProperty(Parameter parameter) => $"public {GetFieldType(parameter)} {parameter.ClassFieldName} {{ get; }}";
        private static string GetConstructorAssignment(Parameter parameter) => $"{parameter.ClassFieldName} = {parameter.ConstructorParameterName} ?? throw new ArgumentNullException(nameof({parameter.ConstructorParameterName}));";
        private static string GetValueGetter(Parameter parameter) => $"{parameter.ClassFieldName}.GetValue()";

    }
}
