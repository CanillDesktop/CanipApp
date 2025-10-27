using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Helpers;

public static class DisplayNameHelper
{
    public static string DisplayNameFor<T>(Expression<Func<T>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Expression must be a member expression", nameof(expression));

        var propInfo = memberExpression.Member as PropertyInfo;
        if (propInfo == null)
            throw new ArgumentException("Expression must refer to a property", nameof(expression));

        var displayAttr = propInfo.GetCustomAttribute<DisplayAttribute>();

        return displayAttr?.Name ?? propInfo.Name;
    }

    public static string DisplayNameFor(PropertyInfo prop)
    {
        ArgumentNullException.ThrowIfNull(prop);

        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();

        return displayAttr?.Name ?? prop.Name;
    }
}
