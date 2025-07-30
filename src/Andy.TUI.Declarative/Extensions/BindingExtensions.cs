using System.Linq.Expressions;
using System.Reflection;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Extensions;

/// <summary>
/// Extensions for creating simple bindings in declarative UI.
/// </summary>
public static class BindingExtensions
{
    /// <summary>
    /// Creates a simple binding from a property expression.
    /// Usage: this.Bind(() => myProperty)
    /// </summary>
    public static Binding<T> Bind<T>(this object source, Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));

        var propertyInfo = memberExpression.Member as PropertyInfo;
        var fieldInfo = memberExpression.Member as FieldInfo;

        if (propertyInfo == null && fieldInfo == null)
            throw new ArgumentException("Expression must reference a property or field", nameof(propertyExpression));

        var getter = propertyExpression.Compile();
        var propertyName = memberExpression.Member.Name;

        Action<T> setter;
        if (propertyInfo != null)
        {
            if (!propertyInfo.CanWrite)
                throw new ArgumentException($"Property '{propertyName}' is read-only", nameof(propertyExpression));
            setter = value => propertyInfo.SetValue(source, value);
        }
        else
        {
            setter = value => fieldInfo!.SetValue(source, value);
        }

        return new Binding<T>(getter, setter, propertyName);
    }
}