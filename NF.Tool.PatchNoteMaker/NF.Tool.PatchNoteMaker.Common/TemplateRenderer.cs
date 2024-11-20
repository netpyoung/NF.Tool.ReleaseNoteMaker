using System;
using System.CodeDom.Compiler;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class TemplateRenderer
    {
        public static async Task Render(string templatePath, PatchNoteConfig config, TemplateModel templateModel, string outputPath)
        {
            string assemblyLocation = typeof(PatchNoteTemplateGenerator).Assembly.Location;

            PatchNoteTemplateGenerator generator = new PatchNoteTemplateGenerator(config, templateModel);
            generator.Refs.Add(assemblyLocation);
            generator.Imports.Add(typeof(PatchNoteTemplateGenerator).Namespace);

            bool isSuccess = await generator.ProcessTemplateAsync(templatePath, outputPath);
            if (!isSuccess)
            {
                foreach (CompilerError err in generator.Errors)
                {
                    Console.Error.WriteLine(err);
                }
            }
        }
    }
}
