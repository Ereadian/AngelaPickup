namespace ereadian.builder.UnitTest;

public static class TestUtility
{
    public static string CreateUniqueName(string? prefix = "Data")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}