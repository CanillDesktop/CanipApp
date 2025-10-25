using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Shared.ExtensionMethods
{
    public static class EnumExtensionMethods
    {
        private static readonly ConcurrentDictionary<Enum, string> _descricaoCache = new();
        public static string GetDescription<T>(this T value) where T : Enum
        {
            if (_descricaoCache.TryGetValue(value, out var descricao))
                return descricao;

            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            descricao = attr?.Description ?? value.ToString();

            _descricaoCache.TryAdd(value, descricao);
            return descricao;
        }
    }
}
