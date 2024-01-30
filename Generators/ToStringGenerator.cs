using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators
{
    [Generator]
    public class ToStringGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Create and configure syntax provider to get all classes.
            var classes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsSyntaxTarget(node),
                static (ctx, _) => GetSemanticTarget(ctx))
                .Where(static target => target is not null);

            // Register output generation for each class.
            context.RegisterSourceOutput(classes,
                static (ctx, source) => Execute(ctx, source!));

            // Add attribute implementation.
            context.RegisterPostInitializationOutput(PostInitializationOutput);
        }

        private static bool IsSyntaxTarget(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0;
        }

        private static ClassDeclarationSyntax? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var attributeSymbol =
                context.SemanticModel.Compilation.GetTypeByMetadataName(
                    "Generators.ToStringGenerator.GenerateToStringAttribute");

            if (classSymbol is not null && attributeSymbol is not null)
            {
                foreach (var attributeData in classSymbol.GetAttributes())
                {
                    if (attributeSymbol.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        return classDeclarationSyntax;
                    }
                }
            }

            return null;
        }

        private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("Generators.ToStringGenerator.GenerateToStringAttribute.g.cs",
                @"namespace Generators.ToStringGenerator
{
    internal class GenerateToStringAttribute : System.Attribute {}
}");
        }

        private static void Execute(SourceProductionContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.Parent is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
            {
                var namespaceName = namespaceDeclarationSyntax.Name.ToString();
                var className = classDeclarationSyntax.Identifier.Text;
                var fileName = $"{namespaceName}.{className}.g.cs";

                var stringBuilder = new StringBuilder();
                stringBuilder.Append($@"namespace {namespaceName}
{{
    partial class {className}
    {{
        public override string ToString()
        {{
            return $""");

            for (var i = 0; i < classDeclarationSyntax.Members.Count; i++)
            {
                if (classDeclarationSyntax.Members[i] is PropertyDeclarationSyntax propertyDeclarationSyntax &&
                    propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
                {
                    if (i > 0) stringBuilder.Append("; ");

                    var propertyName = propertyDeclarationSyntax.Identifier.Text;
                    stringBuilder.Append($"{propertyName}:{{{propertyName}}}");
                }
            }

            stringBuilder.Append($@""";
        }}
    }}
}}
");

            context.AddSource(fileName, stringBuilder.ToString());
            }
        }
    }
}