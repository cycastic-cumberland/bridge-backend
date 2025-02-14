namespace Bridge.Infrastructure.Data;

public static class DataAccessRoot
{
    public static string AssemblyName => (typeof(DataAccessRoot).Assembly.FullName?.Split(',') ?? [])[0].Trim();
}