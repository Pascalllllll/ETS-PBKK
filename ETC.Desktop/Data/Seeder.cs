using ETC.Desktop.Models;

namespace ETC.Desktop.Data;

// Data demo dari etc-be seed.sql; tanggal digeser relatif ke hari ini agar selalu ada rekrutmen terbuka.
public static class Seeder
{
    public const string DemoPassword = "etc12345";

    record SeedDosen(string Nama, string? Rumpun, string? Nip);

    record SeedMahasiswa(string Nama, string Jurusan, string Nrp, string[] Spesialisasi);

    record SeedRek(string Creator, Kegiatan Kegiatan, string Judul, string Role, string Kriteria,
        string[] Skills, decimal? Fee, int Kuota, int DurasiHari);

    const string DemoNrp = "5025241177";

    static readonly Dictionary<string, string> Rumpun = new()
    {
        ["RPL"] = "Rekayasa Perangkat Lunak",
        ["KBJ"] = "Komputasi Berbasis Jaringan",
        ["KCV"] = "Intelligent Vision Computing",
        ["AJK"] = "Architecture & Computer Networking",
        ["IGS"] = "Interaction of Graphics & Art",
        ["AP"] = "Programming Algorithm",
        ["MI"] = "Information Management",
        ["DTK"] = "Basic Applied Computing",
    };

    // Dosen tanpa NIP diberi ID DEMO yang jelas bukan NIP asli.
    static readonly SeedDosen[] Dosen =
    {
        new("Dr. Ir. Siti Rochimah, S.T.", "RPL", null),
        new("Daniel Oranova Siahaan", "RPL", null),
        new("Sarwosri", "RPL", null),
        new("Umi Laili Yuhana", "RPL", null),
        new("Rizky Januar Akbar", "RPL", null),
        new("Tohari Ahmad", "KBJ", null),
        new("Waskitho Wibisono", "KBJ", null),
        new("Hudan Studiawan", "KBJ", null),
        new("Baskoro Adi P.", "KBJ", null),
        new("Prof. Ir. Ary Mazharuddin Shiddiqi, S.Kom., M.Comp.Sc., Ph.D., IPM.", "KBJ", "19810620 200501 1 003"),
        new("Dr.Eng. Nanik Suciati", "KCV", null),
        new("Dr. Chastine Fatichah", "KCV", null),
        new("Prof. Ir. Handayani Tjandrasa", "KCV", null),
        new("Dr. Agus Zainal Arifin", "KCV", null),
        new("Diana Purwitasari, S.Kom., M.Sc.", "KCV", "19780410 200312 2 001"),
        new("Dini Adni Navastara", "KCV", null),
        new("Ir. Muchammad Husni", "AJK", null),
        new("Henning Titi Ciptaningtyas, S.Kom., M.Kom.", "AJK", "19840708 201012 2 004"),
        new("Prof. Ir. Supeno Djanali", "AJK", null),
        new("Dr. Royyana Muslim Ijtihadie", "AJK", null),
        new("Dr. Radityo Anggoro", "AJK", null),
        new("Imam Kuswardayan", "IGS", null),
        new("Dr.Eng. Darlis Herumurti", "IGS", null),
        new("Wijayanti Nurul Khotimah", "IGS", null),
        new("Anny Yuniarti", "IGS", null),
        new("Ridho Rahman Hariadi", "IGS", null),
        new("Ir. F.X. Arunanto", "AP", null),
        new("Dr.tech. Ir. Raden Venantius Hari Ginardi", "AP", null),
        new("Fajar Baskoro, S.Kom., M.T.", "AP", "19740403 199903 1 002"),
        new("Dwi Sunaryono, S.Kom., M.Kom.", "AP", "19720528 199702 1 001"),
        new("Rully Sulaiman, S.Kom., M.Kom.", "AP", "19700213 199402 1 001"),
        new("Prof. Drs. Ec. Ir. Riyanarto Sarno", "MI", null),
        new("Victor Hariadi", "DTK", null),
        new("Prof. Dr. Ir. Joko Lianto Buliali", "DTK", null),
        new("Ir. Suhadi Lili, M.T.I.", null, "19690728 199303 1 001"),
    };

    // NRP berformat asli (prodi + angkatan), nama fiktif kecuali tiga anggota Tim 3.
    static readonly SeedMahasiswa[] Mahasiswa =
    {
        new("Dewi Anggraeni", "Teknik Informatika", "5025231101", ["Java", "Spring Boot"]),
        new("Eka Prasetyo", "Teknik Informatika", "5025231102", ["PHP", "Laravel"]),
        new("Fina Maharani", "Teknik Informatika", "5025231103", ["R", "Statistika"]),
        new("Gilang Saputra", "Teknik Informatika", "5025241101", ["PCB Design", "Microcontroller"]),
        new("Hana Puspita", "Teknik Informatika", "5025241102", ["Data Science", "TensorFlow"]),
        new("Ilham Maulana", "Teknik Informatika", "5025241103", ["Node.js", "Express"]),
        new("Muhammad Abid Baihaqi Al Faridzi", "Teknik Informatika", "5025241133", ["UI/UX", "Figma"]),
        new("Lyonel Oliver Dwiputra", "Teknik Informatika", "5025241145", ["C++", "Arduino"]),
        new("Hosea Felix Sanjaya", "Teknik Informatika", "5025241177", ["Python", "React"]),
        new("Jasmine Putri", "Teknik Informatika", "5025251101", ["Flutter", "Dart"]),
        new("Kevin Hidayat", "Teknik Informatika", "5025251102", ["Go", "Microservices"]),
        new("Laila Nur", "Teknik Informatika", "5025251103", ["SQL", "PostgreSQL"]),
        new("Mario Setiawan", "Teknik Informatika", "5025261101", ["Networking", "Cisco"]),
        new("Nadia Fitri", "Teknik Informatika", "5025261102", ["Machine Learning", "Python"]),
        new("Omar Abdullah", "Teknik Informatika", "5025261103", ["Blockchain", "Web3"]),
        new("Putri Ayu", "Rekayasa Perangkat Lunak", "5053261101", ["Algoritma", "Matematika Diskrit"]),
        new("Rizki Ramadhan", "Rekayasa Perangkat Lunak", "5053261102", ["PLC", "SCADA"]),
        new("Sari Wulandari", "Rekayasa Perangkat Lunak", "5053251101", ["DevOps", "Docker"]),
        new("Taufik Hidayat", "Rekayasa Perangkat Lunak", "5053251102", ["SAP", "Business Process"]),
        new("Ulfa Rahmawati", "Rekayasa Perangkat Lunak", "5053241101", ["Android", "Kotlin"]),
        new("Vino Pratama", "Rekayasa Perangkat Lunak", "5053241102", ["React Native", "JavaScript"]),
        new("Winda Sari", "Rekayasa Perangkat Lunak", "5053231101", ["Power BI", "Tableau"]),
        new("Xander Putra", "Rekayasa Perangkat Lunak", "5053231102", ["Linux", "Shell Scripting"]),
        new("Yuni Astuti", "Rekayasa Kecerdasan Artifisial", "5054261101", ["Vue.js", "TypeScript"]),
        new("Zaki Maulana", "Rekayasa Kecerdasan Artifisial", "5054261102", ["Kubernetes", "AWS"]),
        new("Adelia Putri", "Rekayasa Kecerdasan Artifisial", "5054251101", ["MATLAB", "Simulink"]),
        new("Bagas Nugraha", "Rekayasa Kecerdasan Artifisial", "5054251102", ["Raspberry Pi", "Python"]),
        new("Clara Octaviani", "Rekayasa Kecerdasan Artifisial", "5054241101", ["Swift", "iOS"]),
        new("Dimas Saputro", "Rekayasa Kecerdasan Artifisial", "5054241102", ["GraphQL", "Apollo"]),
        new("Erni Susanti", "Rekayasa Kecerdasan Artifisial", "5054231101", ["Unity", "C#"]),
    };

    // Creator adalah kode rumpun dosen pembuat, atau "demo" untuk akun Hosea.
    static readonly SeedRek[] Rekrutmens =
    {
        new("KCV", Kegiatan.Riset, "Klasifikasi Citra Daun Padi", "Asisten Peneliti ML", "IPK min 3.5, Python, Machine Learning", ["Python", "Machine Learning"], 500000, 2, 180),
        new("RPL", Kegiatan.Projek, "Portal Layanan Akademik Departemen", "Full Stack Developer", "React, Node.js, pengalaman minimal 6 bulan", ["React", "Node.js"], 750000, 2, 200),
        new("DTK", Kegiatan.Lomba, "Satria Data 2025", "Anggota Tim Lomba Statistik", "Mahasiswa S1 aktif, tim 3 orang, IPK min 3.0", ["Statistika", "R"], null, 2, 90),
        new("AJK", Kegiatan.Riset, "Monitoring Kualitas Air Tambak", "Peneliti IoT", "Arduino, sensor IoT, basic C/C++", ["Arduino", "IoT", "C++"], 600000, 1, 200),
        new("KCV", Kegiatan.Projek, "Deteksi Kepadatan Kendaraan", "Computer Vision Engineer", "OpenCV, Python, Computer Vision dasar", ["OpenCV", "Python"], 800000, 2, 210),
        new("KBJ", Kegiatan.Lomba, "CTF Cyber Jawara", "CTF Team Member", "Keamanan jaringan, pengalaman CTF", ["Networking", "Linux"], null, 3, 75),
        new("MI", Kegiatan.Riset, "Dashboard Kinerja UMKM", "Analis Data Bisnis", "Power BI atau Tableau, analisis data bisnis", ["Power BI", "Tableau"], 550000, 1, 210),
        new("AP", Kegiatan.Projek, "Mesin Penjadwalan Kuliah", "Backend Developer", "Algoritma lanjut, C++ atau Java", ["Java", "Algoritma"], 700000, 2, 230),
        new("DTK", Kegiatan.Lomba, "Kontes Robot Indonesia", "Anggota Tim Robot", "Robotika, Arduino atau ROS, tim 2-4 orang", ["Arduino", "ROS"], null, 3, 90),
        new("IGS", Kegiatan.Riset, "Simulasi Evakuasi Berbasis Game", "Game Researcher", "Unity atau Unreal Engine, C# atau C++", ["Unity", "C#"], 650000, 1, 200),
        new("KCV", Kegiatan.Projek, "Chatbot Layanan Kemahasiswaan", "NLP Engineer", "NLP, HuggingFace Transformers, Python", ["Python", "NLP"], 900000, 2, 210),
        new("demo", Kegiatan.Lomba, "Gemastik XIX: Pengembangan Perangkat Lunak", "UI/UX Designer", "Paham Figma, bisa riset pengguna, siap 3 bulan", ["Figma", "UI/UX"], null, 2, 100),
        new("RPL", Kegiatan.Lomba, "Hackathon Kota Cerdas", "Anggota Tim Hackathon", "Web hackathon 48 jam, tim 2-3 orang", ["React", "Node.js"], null, 2, 60),
        new("DTK", Kegiatan.Riset, "Pemodelan Curah Hujan Jawa Timur", "Asisten Riset Statistik", "Statistika inferensial, R atau Python", ["Statistika", "R", "Python"], 520000, 1, 210),
        new("AJK", Kegiatan.Projek, "Otomasi Lini Produksi Mini", "Automation Engineer", "SCADA, PLC, otomasi industri dasar", ["PLC", "SCADA"], 850000, 2, 220),
        new("KCV", Kegiatan.Lomba, "Kompetisi AI Nasional", "AI Competitor", "Kompetisi AI/ML nasional, track machine learning", ["Machine Learning", "Python"], null, 3, 90),
        new("KBJ", Kegiatan.Riset, "Uji Penetrasi Sistem Informasi Kampus", "Security Researcher", "Penetration testing, ethical hacking", ["Linux", "Networking"], 575000, 1, 200),
        new("MI", Kegiatan.Projek, "Migrasi ERP Koperasi Mahasiswa", "SAP Junior Consultant", "SAP ERP, business process analysis", ["SAP", "Business Process"], 950000, 2, 210),
        new("AP", Kegiatan.Lomba, "ONMIPA Matematika", "Peserta Olimpiade Matematika", "Olimpiade matematika, kalkulus dan aljabar", ["Matematika Diskrit", "Algoritma"], null, 2, 60),
        new("DTK", Kegiatan.Riset, "Kendali Lengan Robot Adaptif", "Peneliti Kontrol Sistem", "Kontrol sistem, MATLAB dan Simulink", ["MATLAB", "Simulink"], 610000, 1, 210),
        new("IGS", Kegiatan.Projek, "Tur Virtual Museum Surabaya", "VR Developer", "Unity VR/AR, C#, pengalaman 3D modeling", ["Unity", "C#"], 1000000, 2, 200),
        new("KCV", Kegiatan.Lomba, "Data Science Competition Arkavidia", "Data Scientist Kompetisi", "Data science competition, pengalaman Kaggle", ["Python", "Data Science"], null, 2, 90),
        new("KBJ", Kegiatan.Riset, "Arsitektur Serverless untuk SIAKAD", "Cloud Researcher", "Cloud architecture, AWS atau GCP, DevOps", ["AWS", "DevOps"], 720000, 1, 200),
        new("demo", Kegiatan.Projek, "Aplikasi Peminjaman Ruang Lab", "Mobile Developer", "Flutter atau React Native, pernah rilis aplikasi", ["Flutter", "React Native"], 400000, 2, 120),
        new("KCV", Kegiatan.Projek, "Segmentasi Citra Medis Sederhana", "Image Processing Developer", "Image processing, scikit-image, Python", ["Python", "OpenCV"], 680000, 2, 210),
        new("AJK", Kegiatan.Lomba, "Lomba Desain PCB Nasional", "PCB Designer", "Desain PCB, KiCad atau Altium Designer", ["PCB Design"], null, 1, 75),
        new("KCV", Kegiatan.Riset, "Deep Learning untuk Deteksi Retak Jalan", "Deep Learning Researcher", "Deep learning, PyTorch, GPU computing", ["PyTorch", "Python"], 850000, 2, 210),
        new("AJK", Kegiatan.Projek, "Pipeline CI/CD Lab Jaringan", "DevSecOps Engineer", "DevSecOps, Docker, Kubernetes, CI/CD pipeline", ["Docker", "Kubernetes"], 900000, 2, 200),
        new("MI", Kegiatan.Lomba, "Business Case Competition", "Business Analyst Kompetisi", "Business case competition, analisis pasar", ["Business Process", "Power BI"], null, 3, 90),
        new("KBJ", Kegiatan.Riset, "Implementasi Kriptografi Ringan", "Cryptography Researcher", "Kriptografi modern, implementasi algoritma", ["Kriptografi", "C++"], 630000, 1, 200),
        new("DTK", Kegiatan.Projek, "Robot Pengantar Dokumen Gedung", "Robotics Developer", "Robot otonom, ROS 2, Python", ["ROS", "Python"], 950000, 2, 210),
        new("IGS", Kegiatan.Lomba, "Game Jam 72 Jam", "Game Developer Kompetisi", "Game jam 72 jam, Unity atau Godot, tim 2-3", ["Unity", "C#"], null, 2, 45),
        new("KCV", Kegiatan.Riset, "Reinforcement Learning Lampu Lalu Lintas", "RL Researcher", "Reinforcement learning, OpenAI Gym, PyTorch", ["PyTorch", "Machine Learning"], 780000, 1, 210),
        new("RPL", Kegiatan.Projek, "Layanan Notifikasi Kampus", "Backend Microservices Engineer", "Microservices, Go, gRPC, Apache Kafka", ["Go", "Microservices"], 1100000, 2, 200),
        new("DTK", Kegiatan.Lomba, "Olimpiade Statistika Nasional", "Statistics Competitor", "Olimpiade statistika, inferensi dan regresi", ["Statistika", "R"], null, 2, 90),
        new("AJK", Kegiatan.Riset, "Sensor Network Smart Grid", "Smart Grid Researcher", "Smart grid, IoT sensor network, MQTT", ["IoT", "Raspberry Pi"], 700000, 1, 200),
        new("demo", Kegiatan.Lomba, "Hology 8.0 Web Development", "Frontend Developer", "React atau Vue, paham Git, aktif berdiskusi", ["React", "TypeScript"], null, 2, 70),
        new("RPL", Kegiatan.Projek, "MLOps untuk Model Presensi Wajah", "MLOps Engineer", "MLOps, model deployment, FastAPI, monitoring", ["Python", "Docker"], 1050000, 2, 210),
        new("KBJ", Kegiatan.Lomba, "CTF Nasional Kategori Web & Pwn", "CTF Team Captain", "CTF nasional, tim 4 orang, kategori web & pwn", ["Linux", "Shell Scripting"], null, 3, 75),
        new("MI", Kegiatan.Riset, "Transformasi Digital Proses Bisnis", "Digital Transform Researcher", "Digital transformation, analisis ERP & BPM", ["SAP", "Business Process"], 660000, 1, 210),
        new("AP", Kegiatan.Projek, "Compiler Bahasa Edukasi", "Compiler Engineer", "Compiler design, LLVM IR, bahasa pemrograman", ["C++", "Algoritma"], 800000, 2, 200),
        new("DTK", Kegiatan.Lomba, "KRTI Drone Competition", "Drone Pilot Researcher", "KRTI drone competition, autonomous flight mode", ["ROS", "Python"], null, 3, 90),
        new("IGS", Kegiatan.Riset, "Spatial Computing untuk Pembelajaran", "Spatial Computing Researcher", "Spatial computing, WebXR, AR/MR development", ["Unity", "TypeScript"], 875000, 1, 210),
    };

    static readonly string[] Alasan =
    {
        "Ingin mendalami bidang ini dalam lingkungan riset nyata",
        "Tertarik membangun produk berskala besar bersama tim",
        "Ingin mengasah skill lomba bersama mentor berpengalaman",
        "Pengalaman saya cocok dengan topik ini",
        "Ingin berkontribusi dan belajar dari dosen ahlinya",
        "Pengalaman analisis data cocok untuk kegiatan ini",
        "Antusias di bidang ini sejak awal kuliah",
        "Siap berkomitmen penuh sampai kegiatan selesai",
        "Skill yang saya punya saling melengkapi dengan kebutuhan tim",
    };

    static readonly string[] Ulasan =
    {
        "Komunikatif dan selalu tepat waktu.",
        "Kontribusi teknisnya besar, dokumentasi rapi.",
        "Cepat belajar hal baru, perlu lebih aktif saat diskusi.",
        "Sangat membantu saat mendekati tenggat.",
        "Bertanggung jawab, hasil kerja sesuai kesepakatan.",
        "Ide-idenya membuat solusi tim lebih matang.",
    };

    public static void Seed(EtcDbContext db, DateTime? now = null)
    {
        if (db.Users.Any()) return;

        var today = (now ?? DateTime.Now).Date;
        var rng = new Random(20261015);
        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, 10);

        var dosen = Dosen.Select((d, i) => new User
        {
            Nama = d.Nama,
            Jurusan = "Teknik Informatika",
            NoPengenal = d.Nip ?? $"DEMO-{d.Rumpun ?? "IF"}-{i + 1:00}",
            Role = UserRole.Dosen,
            Bio = d.Rumpun is null ? null : $"Rumpun {d.Rumpun} ({Rumpun[d.Rumpun]})",
        }).ToList();
        var mahasiswa = Mahasiswa.Select(m => new User
        {
            Nama = m.Nama,
            Jurusan = m.Jurusan,
            NoPengenal = m.Nrp,
            Role = UserRole.Mahasiswa,
            Spesialisasi = m.Spesialisasi.ToList(),
        }).ToList();
        dosen.Single(d => d.Nama.Contains("Ary Mazharuddin")).Bio = "Kepala Departemen Teknik Informatika, Rumpun KBJ";

        var users = dosen.Concat(mahasiswa).ToList();
        for (int i = 0; i < users.Count; i++)
        {
            users[i].NoTelp = $"08123456{i:0000}";
            users[i].PasswordHash = hash;
            users[i].CreatedAt = users[i].UpdatedAt = today.AddDays(-400 + i);
        }
        db.Users.AddRange(users);

        var demo = mahasiswa.Single(m => m.NoPengenal == DemoNrp);
        var nextByRumpun = new Dictionary<string, int>();
        User PickCreator(string code)
        {
            if (code == "demo") return demo;
            var pool = dosen.Where((_, i) => Dosen[i].Rumpun == code).ToList();
            var k = nextByRumpun.GetValueOrDefault(code);
            nextByRumpun[code] = k + 1;
            return pool[k % pool.Count];
        }
        int n = Rekrutmens.Length;

        for (int i = 0; i < n; i++)
        {
            var s = Rekrutmens[i];
            var creator = PickCreator(s.Creator);
            var created = today.AddDays(-(n - 1 - i) * 8.6 - 3).AddHours(9 + i % 7);
            // Jendela 60 sampai 95 hari: sekitar sepuluh rekrutmen terbaru masih dibuka.
            var batas = created.Date.AddDays(60 + i * 7 % 36);
            var mulai = batas.AddDays(7);
            var rek = new Rekrutmen
            {
                UserId = creator.UserId,
                Kegiatan = s.Kegiatan,
                Judul = s.Judul,
                Role = s.Role,
                Kriteria = s.Kriteria,
                Deskripsi = Deskripsi(s),
                Skills = s.Skills.ToList(),
                Kuota = s.Kuota,
                BatasPendaftaran = batas,
                TanggalMulai = mulai,
                TanggalSelesai = mulai.AddDays(s.DurasiHari),
                Fee = s.Fee,
                ContactPerson = $"wa.me/62{creator.NoTelp[1..]}",
                CreatedAt = created,
            };
            var tim = new Tim { RekrutmenId = rek.RekrutmenId, TipeTim = s.Kegiatan, CreatedAt = created };
            tim.Participants.Add(new TimParticipant { TimId = tim.TimId, UserId = creator.UserId, MemberKe = 1 });
            rek.Tim = tim;
            db.Rekrutmens.Add(rek);

            bool closed = batas < today;
            // Mahasiswa dengan keahlian cocok lebih mungkin melamar.
            var pool = mahasiswa.Where(m => m.UserId != creator.UserId && m != demo)
                .OrderByDescending(m => m.Spesialisasi.Count(x => s.Skills.Contains(x)) * 3 + rng.Next(4)).ToList();
            int applicantCount = closed ? rng.Next(2, 6) : rng.Next(1, 5);
            var applicants = pool.Take(applicantCount).ToList();

            // Akun demo tidak melamar rekrutmen yang cocok, supaya tetap muncul sebagai rekomendasi.
            if (creator != demo && ((closed && i % 4 == 1) || (!closed && i % 2 == 0 && !s.Skills.Intersect(demo.Spesialisasi).Any())))
                applicants.Insert(0, demo);

            int accepted = 0, member = 2;
            foreach (var u in applicants)
            {
                var applied = created.AddDays(rng.Next(1, Math.Max(2, ((batas < today ? batas : today) - created).Days)));
                if (applied > today) applied = today.AddHours(-rng.Next(2, 30));
                var p = new Pendaftar
                {
                    RekrutmenId = rek.RekrutmenId,
                    UserId = u.UserId,
                    AlasanMendaftar = rng.Next(5) == 0 ? null : Alasan[rng.Next(Alasan.Length)],
                    CvUrl = $"https://drive.google.com/cv/{Slug(u.Nama)}",
                    PortofolioUrl = $"https://github.com/{Slug(u.Nama)}",
                    CreatedAt = applied,
                };
                if (closed)
                {
                    p.Status = accepted < s.Kuota && rng.Next(3) > 0 ? StatusPendaftar.Accepted : StatusPendaftar.Rejected;
                    p.DecidedAt = batas.AddDays(rng.Next(0, 4));
                }
                else if (accepted < s.Kuota - 1 && rng.Next(4) == 0)
                {
                    p.Status = StatusPendaftar.Accepted;
                    p.DecidedAt = applied.AddDays(1);
                }
                if (p.Status == StatusPendaftar.Accepted)
                {
                    accepted++;
                    tim.Participants.Add(new TimParticipant { TimId = tim.TimId, UserId = u.UserId, MemberKe = member++ });
                }
                db.Pendaftars.Add(p);
            }
            rek.IsOpen = !closed && accepted < s.Kuota;

            if (rek.TanggalSelesai < today && tim.Participants.Count > 1)
            {
                var ketua = tim.Participants[0];
                foreach (var m in tim.Participants.Skip(1))
                {
                    db.Histories.Add(NewReview(rng, m.UserId, ketua.UserId, tim, rek.TanggalSelesai));
                    if (rng.Next(3) > 0)
                        db.Histories.Add(NewReview(rng, ketua.UserId, m.UserId, tim, rek.TanggalSelesai));
                }
            }
        }

        foreach (var r in db.ChangeTracker.Entries<Rekrutmen>().Select(e => e.Entity)
                     .Where(r => r.IsOpen && r.UserId != demo.UserId).Take(3).ToList())
            db.Bookmarks.Add(new Bookmark { RekrutmenId = r.RekrutmenId, UserId = demo.UserId });

        db.SaveChanges();
    }

    static History NewReview(Random rng, Guid target, Guid reviewer, Tim tim, DateTime selesai) => new()
    {
        UserId = target,
        ReviewerUserId = reviewer,
        TimId = tim.TimId,
        Rating = rng.Next(10) < 7 ? 5 : rng.Next(3, 5),
        Deskripsi = Ulasan[rng.Next(Ulasan.Length)],
        CreatedAt = selesai.AddDays(rng.Next(1, 6)),
    };

    static string Deskripsi(SeedRek s) => s.Kegiatan switch
    {
        Kegiatan.Lomba => $"Mencari {s.Role.ToLower()} untuk {s.Judul}. Tim akan latihan rutin setiap minggu dan membagi tugas sesuai keahlian masing-masing.",
        Kegiatan.Riset => $"Riset \"{s.Judul}\" membutuhkan {s.Role.ToLower()} yang mau terlibat dari pengumpulan data sampai penulisan laporan.",
        _ => $"Projek \"{s.Judul}\" sedang mencari {s.Role.ToLower()}. Pekerjaan dilakukan dalam sprint dua mingguan bersama tim.",
    };

    static string Slug(string nama) => nama.ToLower().Replace(' ', '-');
}
