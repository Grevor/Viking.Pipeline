using System;
using System.IO;

namespace Viking.Pipeline.CodeGenerator
{
    class Program
    {
        static void Main()
        {
            Generate("ReactionTemplate.txt", Environment.CurrentDirectory + @"\..\..\..\..\Viking.Pipeline\Generated\Reactions\ReactionPipelineStage$Number$.cs", 8);
            Generate("OperationTemplate.txt", Environment.CurrentDirectory + @"\..\..\..\..\Viking.Pipeline\Generated\Operations\OperationPipelineStage$Number$.cs", 8);

            //Generate("DispatcherReactionTemplate.txt", Environment.CurrentDirectory + @"\..\..\..\..\Viking.Pipeline.FrameworkWpf\Generated\Dispatcher\DispatcherReactionPipelineStage$Number$.cs", 8);
        }

        private static void Generate(string template, string toFile, int toGenerate)
        {
            var templateText = File.ReadAllText(template);

            var dir = Path.GetDirectoryName(toFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            for (int i = 1; i < toGenerate + 1; ++i)
            {
                var generator = new Generator(new PepeHands(i));
                var fileName = toFile.Replace("$Number$", i.ToString());
                File.WriteAllText(fileName, generator.GetString(templateText));
            }
        }
    }
}
