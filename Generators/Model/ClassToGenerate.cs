using System;
using System.Collections.Generic;
using System.Linq;

namespace Generators.Model;

internal class ClassToGenerate : IEquatable<ClassToGenerate?>
{
    public ClassToGenerate(
        string namespaceName,
        string className,
        IEnumerable<string> propertyNames)
    {
        NamespaceName = namespaceName;
        ClassName = className;
        PropertyNames = propertyNames;
    }

    public string NamespaceName { get; }
    public string ClassName { get; }
    public IEnumerable<string> PropertyNames { get; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ClassToGenerate);
    }

    public bool Equals(ClassToGenerate? other)
    {
        return other is not null &&
               NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName &&
               PropertyNames.SequenceEqual(other.PropertyNames);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (NamespaceName.GetHashCode() * 397) ^ ClassName.GetHashCode();
        }
    }
}