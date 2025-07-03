using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CaptainPayment.Stripe.Extension;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return (enumValue
                .GetType()
                .GetField(enumValue.ToString())
                ?.GetCustomAttributes(typeof(DisplayAttribute), false)
            is DisplayAttribute[] { Length: > 0 } attrs
            ? attrs[0].Name
            : enumValue.ToString().ToLower()) ?? string.Empty;
    }
    
    public static string GetDisplayName<T>(this T enumValue) where T : Enum
    {
        var fieldInfo = typeof(T).GetField(enumValue.ToString());

        if (fieldInfo == null)
            return enumValue.ToString().ToLower();

        var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString().ToLower();
    }
}