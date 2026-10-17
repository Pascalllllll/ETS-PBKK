using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Data;

// EnsureCreated tidak mengubah database lama, jadi kolom dan tabel baru ditambahkan di sini tanpa menghapus data.
public static class SchemaUpgrade
{
    static readonly (string Table, string Column, string Type)[] Columns =
    {
        ("Users", "Angkatan", "INTEGER NULL"),
        ("Users", "Pengalaman", "TEXT NULL"),
        ("Users", "LinkedInUrl", "TEXT NULL"),
        ("Users", "PortofolioUrl", "TEXT NULL"),
        ("Users", "FotoPath", "TEXT NULL"),
        ("Users", "IsMentor", "INTEGER NOT NULL DEFAULT 0"),
        ("Rekrutmens", "LinkDetail", "TEXT NULL"),
    };

    public static void Apply(EtcDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        try
        {
            foreach (var (table, column, type) in Columns)
                if (!HasColumn(conn, table, column))
                    Exec(conn, $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {type}");

            Exec(conn, """
                CREATE TABLE IF NOT EXISTS "MentorRequests" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_MentorRequests" PRIMARY KEY,
                    "MentorId" TEXT NOT NULL,
                    "UserId" TEXT NOT NULL,
                    "Kegiatan" TEXT NOT NULL,
                    "Topik" TEXT NOT NULL,
                    "Pesan" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "DecidedAt" TEXT NULL,
                    CONSTRAINT "FK_MentorRequests_Users_MentorId" FOREIGN KEY ("MentorId") REFERENCES "Users" ("UserId") ON DELETE CASCADE,
                    CONSTRAINT "FK_MentorRequests_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
                )
                """);
            Exec(conn, """CREATE INDEX IF NOT EXISTS "IX_MentorRequests_MentorId" ON "MentorRequests" ("MentorId")""");
            Exec(conn, """CREATE INDEX IF NOT EXISTS "IX_MentorRequests_UserId" ON "MentorRequests" ("UserId")""");
        }
        finally { conn.Close(); }
    }

    static bool HasColumn(System.Data.Common.DbConnection conn, string table, string column)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}'";
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    static void Exec(System.Data.Common.DbConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
