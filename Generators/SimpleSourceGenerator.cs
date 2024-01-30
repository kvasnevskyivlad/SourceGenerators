using Microsoft.CodeAnalysis;

namespace Generators
{
    [Generator]
    public class SimpleSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // определяем генерируемый код
            var code = @"
            namespace TestSG
            {
                public static class Welcome 
                {
                    public const string Name = ""Vlad"";
                    public static void Print() => Console.WriteLine($""Hello {Name}!"");
                }
            }";
            context.AddSource("testsg.welcome.generated.cs", code);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // инициализация не нужна
        }
    }
}
