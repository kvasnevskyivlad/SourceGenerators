using Generators.ToStringGenerator;

namespace TestNugetPackageConsoleApp
{
    [GenerateToString]
    public partial class MyClass
    {
        public string? Test1 { get; set; }
        public int Test2 { get; set; }
    }
}
