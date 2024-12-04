
public static class ControllObjects
{
    public static bool IsNull<T>(this T type)
    {
        if (type is string)
        {
            return string.IsNullOrEmpty(type as string);
        }

        if (type == null)
        {
            return true;
        }

        return type.Equals(default(T));
    }
}
