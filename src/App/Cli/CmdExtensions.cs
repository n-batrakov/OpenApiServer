using System.Collections.Generic;

using Microsoft.Extensions.CommandLineUtils;

namespace OpenApiServer.Cli
{
    public static class CmdExtensions
    {
        public static bool GetBooleanValue(this CommandOption value) => value.HasValue();

        public static string GetStringValue(this CommandOption value, string defaultValue = default) =>
                value.Value() ?? defaultValue;

        public static IEnumerable<string> GetStringValues(this CommandArgument value, string defaultValue) =>
                value.Values.Count == 0 ? new[] { defaultValue } : value.Values.ToArray();

        public static int GetIntValue(this CommandOption value, int defaultValue = default) =>
                int.TryParse(value.Value(), out var port) ? port : defaultValue;
    }
}