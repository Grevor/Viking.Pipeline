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
        public const string ValueParametersPlaceholder = "$ValueParameters$";
        public const string GettersPlaceholder = "$Getters$";
        public const string ClassFieldsPlaceholder = "$ClassFields$";
        public const string ParameterCommentsPlaceholder = "$ParameterComments$";
        public const string TypeParameterCommentsPlaceholder = "$TypeParameterComments$";
        public const string TypeParameterCommentsPlaceholder2 = "$TypeParameterComments2$";


        private string GenericParameters { get; }
        private string ConstructorParameters { get; }
        private string ValueParameters { get; }
        private string ConstructorAssignments { get; }
        private string RepassingParameters { get; }
        private string Getters { get; }
        private string ClassFields { get; }
        private string ParameterComments { get; }
        private string TypeParameterComments { get; }
        private string TypeParameterComments2 { get; }

        public Generator(IGenerator gen)
        {
            var parameters = gen.GetParameters().ToList();
            var hasParameters = parameters.Any();

            GenericParameters = hasParameters ? string.Join(", ", parameters.Select(p => p.TypeParameterName)) : "";
            ConstructorParameters = hasParameters ? string.Join("," + Environment.NewLine + "\t\t\t", parameters.Select(GetConstructorParameter)) : "";
            ConstructorAssignments = hasParameters ? string.Join(Environment.NewLine + "\t\t\t", parameters.Select(GetConstructorAssignment)) : "";
            RepassingParameters = hasParameters ? string.Join(", ", parameters.Select(p => p.ConstructorParameterName)) : "";
            ValueParameters = hasParameters ? string.Join(", ", parameters.Select(GetValueParameter)) : "";
            Getters = hasParameters ? string.Join(", ", parameters.Select(GetValueGetter)) : "";
            ClassFields = hasParameters ? string.Join(Environment.NewLine + "\t\t", parameters.Select(GetClassProperty)) : "";
            ParameterComments = hasParameters ? string.Join(Environment.NewLine + "\t\t", parameters.Select(GetParameterComment)) : "";
            TypeParameterComments = hasParameters ? string.Join(Environment.NewLine + "\t\t", parameters.Select(GetTypeParameterComment)) : "";
            TypeParameterComments2 = hasParameters ? string.Join(Environment.NewLine + "\t", parameters.Select(GetTypeParameterComment)) : "";
        }

        public string GetString(string template)
        {
            return template
                .Replace(GenericParametersPlaceholder, GenericParameters)
                .Replace(ConstructorParametersPlaceholder, ConstructorParameters)
                .Replace(ValueParametersPlaceholder, ValueParameters)
                .Replace(ConstructorAssignmentsPlaceholder, ConstructorAssignments)
                .Replace(RepassingParametersPlaceholder, RepassingParameters)
                .Replace(GettersPlaceholder, Getters)
                .Replace(ParameterCommentsPlaceholder, ParameterComments)
                .Replace(TypeParameterCommentsPlaceholder, TypeParameterComments)
                .Replace(TypeParameterCommentsPlaceholder2, TypeParameterComments2)
                .Replace(ClassFieldsPlaceholder, ClassFields);
        }


        private static string GetFieldType(Parameter parameter) => $"IPipelineStage<{parameter.TypeParameterName}>";
        private static string GetConstructorParameter(Parameter parameter) => $"{GetFieldType(parameter)} {parameter.ConstructorParameterName}";
        private static string GetValueParameter(Parameter parameter) => $"{parameter.TypeParameterName} {parameter.ConstructorParameterName}";
        private static string GetConstructorAssignment(Parameter parameter) => $"{parameter.ClassFieldName} = {parameter.ConstructorParameterName} ?? throw new ArgumentNullException(nameof({parameter.ConstructorParameterName}));";
        private static string GetValueGetter(Parameter parameter) => $"{parameter.ClassFieldName}.GetValue()";

        private static string GetClassProperty(Parameter parameter) => 
            "/// <summary>" + Environment.NewLine +
            $"\t\t/// Input number {parameter.Number}." + Environment.NewLine +
            "\t\t/// </summary>" + Environment.NewLine +
            $"\t\tpublic {GetFieldType(parameter)} {parameter.ClassFieldName} {{ get; }}";

        private static string GetParameterComment(Parameter parameter) =>
            $"/// <param name=\"{parameter.ConstructorParameterName}\">Input number {parameter.Number}.</param>";

        private static string GetTypeParameterComment(Parameter parameter) =>
            $"/// <typeparam name=\"{parameter.TypeParameterName}\">The type of input number {parameter.Number}.</typeparam>";
    }
}
