using Generators.ToStringGenerator;

namespace TestConsoleApp.Model
{
    [GenerateToString]
    public partial class Person
    {
        public string? FirstName { get; set; }
        private string? MiddleName { get; set; }
        public string? LastName { get; set; }
    }
}
