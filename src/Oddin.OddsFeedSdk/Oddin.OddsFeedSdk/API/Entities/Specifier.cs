using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Specifier : ISpecifier
    {
        public string Name { get; }

        public string Type { get; }

        public Specifier(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
