using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace UnitTests.Utils
{
    public class InMemoryConfiguration : IConfiguration
    {
        private  IConfiguration Source { get; }

        public InMemoryConfiguration(string root = "", params (string, string)[] values)
                : this(root, values.ToDictionary(x => x.Item1, x => x.Item2))
        {
        }

        public InMemoryConfiguration(string root, IReadOnlyDictionary<string, string> values)
        {
            var configValues = string.IsNullOrEmpty(root)
                                       ? values
                                       : values.ToDictionary(x => $"{root}:{x.Key}", x => x.Value);

            Source = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        }

        public IConfigurationSection GetSection(string key) => Source.GetSection(key);
        public IEnumerable<IConfigurationSection> GetChildren() => Source.GetChildren();
        public IChangeToken GetReloadToken() => Source.GetReloadToken();
        public string this[string key]
        {
            get => Source[key];
            set => Source[key] = value;
        }
    }
}