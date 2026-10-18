using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Data;

// EnsureCreated tidak mengubah database lama. Database dari versi yang masih punya peran dosen
// (kolom Users.Role) dibuat ulang, karena sebagian besar rekrutmen demonya dibuat oleh akun dosen.
public static class SchemaUpgrade
{
    public static void Apply(EtcDbContext db)
    {
        if (HasColumn(db, "Users", "Role")) db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    static bool HasColumn(EtcDbContext db, string table, string column)
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}'";
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }
        finally { conn.Close(); }
    }
}
