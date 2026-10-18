# ETC Desktop

Aplikasi desktop **Elite TC Champions** untuk mencari dan membentuk tim lomba, riset, dan projek antarmahasiswa ITS. Dibuat dengan C# (.NET 8) dan Avalonia UI sebagai tugas ETS Pemrograman Berbasis Kerangka Kerja, Tim 3.

Dokumen kebutuhan lengkap: [docs/PRD.md](docs/PRD.md).

![Ringkasan](docs/screenshots/light-Ringkasan.png)

## Menjalankan

Butuh [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project ETC.Desktop
```

Saat pertama dijalankan, aplikasi membuat database SQLite dan mengisinya dengan data demo dari `etc-be/migrations/script/seed.sql`.

| Akun demo | NRP | Kata sandi |
|---|---|---|
| Hosea Felix Sanjaya (data paling lengkap) | `5025241177` | `etc12345` |
| Muhammad Abid Baihaqi Al Faridzi | `5025241133` | `etc12345` |
| Lyonel Oliver Dwiputra | `5025241145` | `etc12345` |
| Dimas Surya Pratama (mentor, punya permintaan bimbingan masuk) | `5025231247` | `etc12345` |

Mahasiswa demo adalah 84 mahasiswa Teknik Informatika (`5025`) angkatan 23 sampai 26 dengan nama dan NRP asli, diambil dari daftar mahasiswa departemen. Lamaran, keputusan, ulasan, dan prestasi mentor mereka adalah data karangan untuk demo.

ETC hanya untuk mahasiswa; semua akun masuk dengan NRP 10 digit. Layar masuk juga punya tombol **Isi akun demo**.

Lokasi database: `~/.local/share/ETC/etc.db` (Linux), `%LOCALAPPDATA%\ETC\etc.db` (Windows), `~/Library/Application Support/ETC/etc.db` (macOS). Hapus file itu untuk mengulang data demo. Database dari versi yang masih punya akun dosen dibuat ulang otomatis dengan data demo baru. Foto profil disimpan di subfolder `foto` di folder data yang sama.

## Fitur

- **Ringkasan**: pelamar yang menunggu keputusan (terima/tolak langsung), posisi Anda, rekomendasi rekrutmen yang cocok dengan keahlian, rekrutmen yang segera tutup, grafik rekrutmen per bulan, keahlian paling dicari, dan status lamaran.
- **Jelajahi**: cari, filter lomba/riset/projek, urutkan, paginasi, detail, simpan, lamar.
- **Rekrutmen saya**: buat, ubah, tutup/buka, hapus; tinjau pelamar beserta kecocokan keahlian, reputasi, dan ulasan.
- **Lamaran saya**: status lamaran, batalkan yang masih menunggu.
- **Tim saya**: anggota tim, kontak WhatsApp, ulasan bintang setelah kegiatan selesai.
- **Mentor**: daftar kakak tingkat beserta foto, keahlian, dan prestasi; cari berdasarkan nama atau keahlian; tombol **Butuh mentor?** membuka formulir permintaan bimbingan. Mentor menerima atau menolak permintaan di tab Permintaan masuk.
- **Profil**: foto, angkatan, keahlian, pengalaman dan prestasi, tautan LinkedIn/portofolio, mode mentor, rating dan ulasan yang diterima, mode terang/gelap.

Ringkasan juga menampilkan kartu **Lengkapi profil Anda** sampai semua langkah profil selesai. Rekrutmen bisa diberi link detail opsional (guidebook atau halaman resmi lomba).

Shortcut: Ctrl+1 sampai Ctrl+7 untuk halaman, Ctrl+N buka rekrutmen, Ctrl+F cari, Ctrl+\\ sidebar, Escape tutup sheet.

## Struktur

```
ETC.Desktop/
  Models/        Entitas (User, Rekrutmen, Pendaftar, Tim, TimParticipant, History, Bookmark, MentorRequest)
  Data/          EtcDbContext (EF Core), SchemaUpgrade, dan Seeder
  Services/      Logika bisnis dan validasi (Auth, Rekrutmen, Pendaftar, Tim, Dashboard, Mentor)
  ViewModels/    MVVM dengan CommunityToolkit.Mvvm
  Views/         XAML Avalonia
  Controls/      Glyph (ikon), Avatar (foto atau inisial), dan grafik kustom (batang bertumpuk, batang horizontal, meter status)
  Styles/        Token warna terang/gelap, gaya kontrol, ikon
ETC.Tests/       xUnit: test service dan test UI headless
docs/            PRD dan screenshot
```

## Pengujian

```bash
dotnet test
```

51 test: aturan bisnis di service, plus klik-tembus UI dengan Avalonia Headless (masuk, navigasi sidebar, melamar, menerima pelamar, ulasan, hapus dengan konfirmasi, shortcut keyboard, mode gelap, onboarding profil, permintaan mentor).

## Screenshot untuk dokumentasi

Aplikasi bisa merender setiap halaman ke PNG (terang dan gelap) lalu keluar:

```bash
ETC_SNAPSHOT_DIR=docs/screenshots dotnet run --project ETC.Desktop
```

Variabel opsional: `ETC_SNAPSHOT_PAGES` (contoh `auth,Ringkasan,Jelajahi`), `ETC_SNAPSHOT_THEMES` (`light,dark`), `ETC_SNAPSHOT_USER`. `ETC_LOG=1` menampilkan peringatan Avalonia di terminal, `ETC_DB` memakai file database lain.

## Kredit

- Font [Hanken Grotesk](https://fonts.google.com/specimen/Hanken+Grotesk) (SIL Open Font License).
- Ikon [Material Symbols](https://fonts.google.com/icons) (Apache License 2.0).
- Logo dan konsep ETC dari pitch deck Tim 3; skema data dan data demo dari [etc-be](https://github.com/Arthamna/etc-be).
