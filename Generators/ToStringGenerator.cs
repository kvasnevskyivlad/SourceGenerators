using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Generators.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create and configure syntax provider to get all classes.
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsSyntaxTarget(node),
                static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static target => target is not null)
            .Collect();

        // Register output generation for each class.
        context.RegisterSourceOutput(classes,
            static (ctx, source) => Execute(ctx, source));

        // Add attribute implementation.
        context.RegisterPostInitializationOutput(PostInitializationOutput);
    }

    private static bool IsSyntaxTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0;
    }

    private static ClassToGenerate? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var attributeSymbol =
            context.SemanticModel.Compilation.GetTypeByMetadataName(
                "Generators.ToStringGenerator.GenerateToStringAttribute");

        if (classSymbol is not null && attributeSymbol is not null)
            foreach (var attributeData in classSymbol.GetAttributes())
                if (attributeSymbol.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                {
                    var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                    var className = classSymbol.Name;
                    var propertyNames = new List<string>();

                    foreach (var memberSymbol in classSymbol.GetMembers())
                        if (memberSymbol.Kind == SymbolKind.Property &&
                            memberSymbol.DeclaredAccessibility == Accessibility.Public)
                            propertyNames.Add(memberSymbol.Name);

                    return new ClassToGenerate(namespaceName, className, propertyNames);
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

    private static void Execute(SourceProductionContext context, ImmutableArray<ClassToGenerate> classesToGenerate)
    {
        foreach (var classToGenerate in classesToGenerate)
        {
            var namespaceName = classToGenerate.NamespaceName;
            var className = classToGenerate.ClassName;
            var fileName = $"{namespaceName}.{className}.g.cs";

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($@"namespace {namespaceName}
{{
    partial class {className}
    {{
        public override string ToString()
        {{
            return $""");

            var first = true;
            foreach (var propertyName in classToGenerate.PropertyNames)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append("; ");

                stringBuilder.Append($"{propertyName}:{{{propertyName}}}");
            }

            stringBuilder.Append(@""";
        }
    }
}
");
            context.AddSource(fileName, stringBuilder.ToString());
        }
    }
}