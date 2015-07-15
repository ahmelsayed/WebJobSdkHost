using Microsoft.Azure.WebJobs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebJobSdkHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var syntax = Directory
                .GetFiles(Environment.CurrentDirectory, "*.cs")
                .Select(File.ReadAllText)
                .Select(c => CSharpSyntaxTree.ParseText(c));

            var refs = new[]
            {
                typeof(object),
                typeof(Enumerable),
                typeof(JobHost),
                typeof(QueueAttribute),
                typeof(CloudStorageAccount),
                typeof(JToken)
            }
            .Select(s => MetadataReference.CreateFromFile(s.Assembly.Location));

            var compilation = CSharpCompilation.Create(Path.GetRandomFileName(), syntax, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
                    failures.ToList().ForEach(e => Console.WriteLine("{0}: {1}", e.Id, e.GetMessage()));
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());
                    var config = new JobHostConfiguration
                    {
                        TypeLocator = new DynamicAssemblyTypeLocator(assembly),
                    };
                    var host = new JobHost(config);
                    host.RunAndBlock();
                }
            }
        }
    }
}
