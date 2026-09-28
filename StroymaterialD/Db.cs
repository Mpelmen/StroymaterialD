using Npgsql;

public static class Db
{
    public const string ConnStr =
        "Host=localhost;Port=5432;Database=StroyMaterialD;Username=postgres;Password=Passw0rd";

    public static NpgsqlConnection Open()
    {
        var conn = new NpgsqlConnection(ConnStr);
        conn.Open();
        return conn;
    }
}