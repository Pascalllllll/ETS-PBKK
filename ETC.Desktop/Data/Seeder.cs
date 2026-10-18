using ETC.Desktop.Models;

namespace ETC.Desktop.Data;

// Data demo dari etc-be seed.sql; tanggal digeser relatif ke hari ini agar selalu ada rekrutmen terbuka.
public static class Seeder
{
    public const string DemoPassword = "etc12345";

    record SeedMahasiswa(string Nama, string Jurusan, string Nrp, string[] Spesialisasi);

    record SeedRek(string? Nrp, Kegiatan Kegiatan, string Judul, string Role, string Kriteria,
        string[] Skills, decimal? Fee, int Kuota, int DurasiHari);

    const string DemoNrp = "5025241177";

    // Mahasiswa Teknik Informatika angkatan 23 sampai 26 (nama dan NRP asli).
    static readonly SeedMahasiswa[] Mahasiswa =
    {
        new("Andrew Wallace", "Teknik Informatika", "5025231171", ["Java", "Spring Boot"]),
        new("Amelia Nova Safitri", "Teknik Informatika", "5025231041", ["PHP", "Laravel"]),
        new("Cathleen Gracia", "Teknik Informatika", "5025231018", ["R", "Statistika"]),
        new("Bagus Cahya Saputra", "Teknik Informatika", "5025241067", ["PCB Design", "Microcontroller"]),
        new("Nabila Shafa Rahayu", "Teknik Informatika", "5025241150", ["Data Science", "TensorFlow"]),
        new("Dewey Sutjiadi", "Teknik Informatika", "5025241012", ["Node.js", "Express"]),
        new("Muhammad Abid Baihaqi Al Faridzi", "Teknik Informatika", "5025241133", ["UI/UX", "Figma"]),
        new("Lyonel Oliver Dwiputra", "Teknik Informatika", "5025241145", ["C++", "Arduino"]),
        new("Hosea Felix Sanjaya", "Teknik Informatika", "5025241177", ["Python", "React"]),
        new("Caroline Alverina", "Teknik Informatika", "5025251097", ["Flutter", "Dart"]),
        new("Aston Justin", "Teknik Informatika", "5025251035", ["Go", "Microservices"]),
        new("Gita Renada", "Teknik Informatika", "5025251105", ["SQL", "PostgreSQL"]),
        new("Karin Witamso", "Teknik Informatika", "5025261014", ["Networking", "Cisco"]),
        new("Fiona Angeline Halim", "Teknik Informatika", "5025261088", ["Machine Learning", "Python"]),
        new("Richard Tan Raharjo", "Teknik Informatika", "5025261263", ["Blockchain", "Web3"]),
        new("Dini Anggraini", "Teknik Informatika", "5025261027", ["Algoritma", "Matematika Diskrit"]),
        new("Samuel Valentino", "Teknik Informatika", "5025261096", ["PLC", "SCADA"]),
        new("Budiman Setiono", "Teknik Informatika", "5025251126", ["DevOps", "Docker"]),
        new("Jessica Febiola", "Teknik Informatika", "5025251216", ["SAP", "Business Process"]),
        new("Krisna Widhi Wijaya", "Teknik Informatika", "5025241166", ["Android", "Kotlin"]),
        new("Stephanie Gabriella", "Teknik Informatika", "5025241081", ["React Native", "JavaScript"]),
        new("Bella Angeline Chong", "Teknik Informatika", "5025231073", ["Power BI", "Tableau"]),
        new("Dimas Surya Pratama", "Teknik Informatika", "5025231247", ["Linux", "Shell Scripting"]),
        new("John Nathanael", "Teknik Informatika", "5025261162", ["Vue.js", "TypeScript"]),
        new("Wilbert Bernardi", "Teknik Informatika", "5025261209", ["Kubernetes", "AWS"]),
        new("Hasan Mohammadi", "Teknik Informatika", "5025251037", ["MATLAB", "Simulink"]),
        new("Ilham Husni Pratama", "Teknik Informatika", "5025251073", ["Raspberry Pi", "Python"]),
        new("Malvin Jonathan", "Teknik Informatika", "5025241238", ["Swift", "iOS"]),
        new("Rendy Tanuwijaya", "Teknik Informatika", "5025241099", ["GraphQL", "Apollo"]),
        new("Bradley Widjaja", "Teknik Informatika", "5025231036", ["Unity", "C#"]),
        new("William Hans Chandra", "Teknik Informatika", "5025241138", ["Python", "Machine Learning"]),
        new("Yoseph Kevin Hendrata", "Teknik Informatika", "5025241146", ["React", "TypeScript"]),
        new("Hartmann Kanisius Galla' Massang", "Teknik Informatika", "5025241160", ["Node.js", "Express"]),
        new("Uwais Achmad", "Teknik Informatika", "5025241103", ["Flutter", "Dart"]),
        new("Bramantya Saputra", "Teknik Informatika", "5025241098", ["Figma", "UI/UX"]),
        new("Naufal Fadlil Waafi", "Teknik Informatika", "5025241246", ["Java", "Spring Boot"]),
        new("Rennard Filbert Tanjaya", "Teknik Informatika", "5025241122", ["Go", "Microservices"]),
        new("Willy Marcelius", "Teknik Informatika", "5025241096", ["SQL", "PostgreSQL"]),
        new("Nathanael Oliver Amadhika Yuswana", "Teknik Informatika", "5025241109", ["Docker", "DevOps"]),
        new("Mathias Adya Diwangkara Suganda", "Teknik Informatika", "5025231015", ["Unity", "C#"]),
        new("Faisal Azmi Sirajuddin", "Teknik Informatika", "5025231127", ["C++", "Algoritma"]),
        new("Prinka Shafa Maharatri Achzania", "Teknik Informatika", "5025231262", ["Linux", "Networking"]),
        new("Kartika Nana Naulita", "Teknik Informatika", "5025241021", ["Data Science", "Python"]),
        new("Via Hana Nurma Putri", "Teknik Informatika", "5025241048", ["Statistika", "R"]),
        new("Dilbina Windi Azahra", "Teknik Informatika", "5025241180", ["OpenCV", "Python"]),
        new("Dyah Utami Kesuma Dewi", "Teknik Informatika", "5025241186", ["Arduino", "IoT"]),
        new("Safa Mashita", "Teknik Informatika", "5025241022", ["Kotlin", "Android"]),
        new("Devina Balqis Aurora", "Teknik Informatika", "5025241034", ["PyTorch", "Machine Learning"]),
        new("Leyan Harits", "Teknik Informatika", "5025231288", ["Python", "Machine Learning"]),
        new("Maleka Ghaniya", "Teknik Informatika", "5025241189", ["React", "TypeScript"]),
        new("Jorell Ramos Sinaga", "Teknik Informatika", "5025241202", ["Node.js", "Express"]),
        new("Jeyhan", "Teknik Informatika", "5025241132", ["Flutter", "Dart"]),
        new("Haenu Narendra", "Teknik Informatika", "5025231272", ["Figma", "UI/UX"]),
        new("Maulana Akbar", "Teknik Informatika", "5025231259", ["Java", "Spring Boot"]),
        new("Kayana Danendra", "Teknik Informatika", "5025231286", ["Go", "Microservices"]),
        new("Nabilah Bunga Sulistia", "Teknik Informatika", "5025241073", ["SQL", "PostgreSQL"]),
        new("Callista Fidelya Roba Gultom", "Teknik Informatika", "5025241086", ["Docker", "DevOps"]),
        new("Muhammad Akhdan Alwaafy", "Teknik Informatika", "5025241223", ["Unity", "C#"]),
        new("Muhammad Daffa Ramadhan", "Teknik Informatika", "5025241216", ["C++", "Algoritma"]),
        new("Raihan Rasyid", "Teknik Informatika", "5025241224", ["Linux", "Networking"]),
        new("Bima Novrifa", "Teknik Informatika", "5025241194", ["Data Science", "Python"]),
        new("Agil Lukman Hakim Muchdi", "Teknik Informatika", "5025241037", ["Statistika", "R"]),
        new("Farrel Jatmiko Aji", "Teknik Informatika", "5025241193", ["OpenCV", "Python"]),
        new("Berwyn Rafif Alvaro", "Teknik Informatika", "5025241029", ["Arduino", "IoT"]),
        new("Muhammad Rivaldi Sholihin", "Teknik Informatika", "5025241188", ["Kotlin", "Android"]),
        new("Rafael Mulia Wiratama Simarmata", "Teknik Informatika", "5025241196", ["PyTorch", "Machine Learning"]),
        new("Nasyita Larasati Ertyananda", "Teknik Informatika", "5025241201", ["Python", "Machine Learning"]),
        new("Rahma Sakinah", "Teknik Informatika", "5025241222", ["React", "TypeScript"]),
        new("Safa Maulana Efendi", "Teknik Informatika", "5025241227", ["Node.js", "Express"]),
        new("Aqil Syafiq Dzaky", "Teknik Informatika", "5025241200", ["Flutter", "Dart"]),
        new("Derryell Josua Ekklesio Pandiangan", "Teknik Informatika", "5025241175", ["Figma", "UI/UX"]),
        new("Achmad Najwa Maraf", "Teknik Informatika", "5025231265", ["Java", "Spring Boot"]),
        new("Ahsin Khuluqil Karim", "Teknik Informatika", "5025241063", ["Go", "Microservices"]),
        new("Azil Alriansyah Hidayat", "Teknik Informatika", "5025241070", ["SQL", "PostgreSQL"]),
        new("Muhamad Aziz Romdhoni", "Teknik Informatika", "5025241071", ["Docker", "DevOps"]),
        new("Umi Lailatul Khotimah", "Teknik Informatika", "5025241062", ["Unity", "C#"]),
        new("M. Rizal Febrianto", "Teknik Informatika", "5025231258", ["C++", "Algoritma"]),
        new("Kemal Tangguh Aji Rajasa", "Teknik Informatika", "5025231263", ["Linux", "Networking"]),
        new("Ahmad Faiz Tajuddin", "Teknik Informatika", "5025231291", ["Data Science", "Python"]),
        new("Liem Alfred Haryanto", "Teknik Informatika", "5025241100", ["Statistika", "R"]),
        new("Danish Faiq Ibad Yuadi", "Teknik Informatika", "5025241038", ["OpenCV", "Python"]),
        new("Fauzan Hafiz Amandani", "Teknik Informatika", "5025241087", ["Arduino", "IoT"]),
        new("Nadine Aulia Putri Fanani", "Teknik Informatika", "5025231206", ["Kotlin", "Android"]),
        new("Rafly Al Fahrezi", "Teknik Informatika", "5025241131", ["PyTorch", "Machine Learning"]),
    };

    // Nrp adalah pembuat rekrutmen; null berarti dipilih mahasiswa yang keahliannya paling cocok.
    static readonly SeedRek[] Rekrutmens =
    {
        new(null, Kegiatan.Riset, "Klasifikasi Citra Daun Padi", "Asisten Peneliti ML", "IPK min 3.5, Python, Machine Learning", ["Python", "Machine Learning"], 500000, 2, 180),
        new(null, Kegiatan.Projek, "Portal Layanan Akademik Departemen", "Full Stack Developer", "React, Node.js, pengalaman minimal 6 bulan", ["React", "Node.js"], 750000, 2, 200),
        new(null, Kegiatan.Lomba, "Satria Data 2025", "Anggota Tim Lomba Statistik", "Mahasiswa S1 aktif, tim 3 orang, IPK min 3.0", ["Statistika", "R"], null, 2, 90),
        new(null, Kegiatan.Riset, "Monitoring Kualitas Air Tambak", "Peneliti IoT", "Arduino, sensor IoT, basic C/C++", ["Arduino", "IoT", "C++"], 600000, 1, 200),
        new(null, Kegiatan.Projek, "Deteksi Kepadatan Kendaraan", "Computer Vision Engineer", "OpenCV, Python, Computer Vision dasar", ["OpenCV", "Python"], 800000, 2, 210),
        new(null, Kegiatan.Lomba, "CTF Cyber Jawara", "CTF Team Member", "Keamanan jaringan, pengalaman CTF", ["Networking", "Linux"], null, 3, 75),
        new(null, Kegiatan.Riset, "Dashboard Kinerja UMKM", "Analis Data Bisnis", "Power BI atau Tableau, analisis data bisnis", ["Power BI", "Tableau"], 550000, 1, 210),
        new(null, Kegiatan.Projek, "Mesin Penjadwalan Kuliah", "Backend Developer", "Algoritma lanjut, C++ atau Java", ["Java", "Algoritma"], 700000, 2, 230),
        new(null, Kegiatan.Lomba, "Kontes Robot Indonesia", "Anggota Tim Robot", "Robotika, Arduino atau ROS, tim 2-4 orang", ["Arduino", "ROS"], null, 3, 90),
        new(null, Kegiatan.Riset, "Simulasi Evakuasi Berbasis Game", "Game Researcher", "Unity atau Unreal Engine, C# atau C++", ["Unity", "C#"], 650000, 1, 200),
        new(null, Kegiatan.Projek, "Chatbot Layanan Kemahasiswaan", "NLP Engineer", "NLP, HuggingFace Transformers, Python", ["Python", "NLP"], 900000, 2, 210),
        new(DemoNrp, Kegiatan.Lomba, "Gemastik XIX: Pengembangan Perangkat Lunak", "UI/UX Designer", "Paham Figma, bisa riset pengguna, siap 3 bulan", ["Figma", "UI/UX"], null, 2, 100),
        new(null, Kegiatan.Lomba, "Hackathon Kota Cerdas", "Anggota Tim Hackathon", "Web hackathon 48 jam, tim 2-3 orang", ["React", "Node.js"], null, 2, 60),
        new(null, Kegiatan.Riset, "Pemodelan Curah Hujan Jawa Timur", "Asisten Riset Statistik", "Statistika inferensial, R atau Python", ["Statistika", "R", "Python"], 520000, 1, 210),
        new(null, Kegiatan.Projek, "Otomasi Lini Produksi Mini", "Automation Engineer", "SCADA, PLC, otomasi industri dasar", ["PLC", "SCADA"], 850000, 2, 220),
        new(null, Kegiatan.Lomba, "Kompetisi AI Nasional", "AI Competitor", "Kompetisi AI/ML nasional, track machine learning", ["Machine Learning", "Python"], null, 3, 90),
        new(null, Kegiatan.Riset, "Uji Penetrasi Sistem Informasi Kampus", "Security Researcher", "Penetration testing, ethical hacking", ["Linux", "Networking"], 575000, 1, 200),
        new(null, Kegiatan.Projek, "Migrasi ERP Koperasi Mahasiswa", "SAP Junior Consultant", "SAP ERP, business process analysis", ["SAP", "Business Process"], 950000, 2, 210),
        new(null, Kegiatan.Lomba, "ONMIPA Matematika", "Peserta Olimpiade Matematika", "Olimpiade matematika, kalkulus dan aljabar", ["Matematika Diskrit", "Algoritma"], null, 2, 60),
        new(null, Kegiatan.Riset, "Kendali Lengan Robot Adaptif", "Peneliti Kontrol Sistem", "Kontrol sistem, MATLAB dan Simulink", ["MATLAB", "Simulink"], 610000, 1, 210),
        new(null, Kegiatan.Projek, "Tur Virtual Museum Surabaya", "VR Developer", "Unity VR/AR, C#, pengalaman 3D modeling", ["Unity", "C#"], 1000000, 2, 200),
        new(null, Kegiatan.Lomba, "Data Science Competition Arkavidia", "Data Scientist Kompetisi", "Data science competition, pengalaman Kaggle", ["Python", "Data Science"], null, 2, 90),
        new(null, Kegiatan.Riset, "Arsitektur Serverless untuk SIAKAD", "Cloud Researcher", "Cloud architecture, AWS atau GCP, DevOps", ["AWS", "DevOps"], 720000, 1, 200),
        new(DemoNrp, Kegiatan.Projek, "Aplikasi Peminjaman Ruang Lab", "Mobile Developer", "Flutter atau React Native, pernah rilis aplikasi", ["Flutter", "React Native"], 400000, 2, 120),
        new(null, Kegiatan.Projek, "Segmentasi Citra Medis Sederhana", "Image Processing Developer", "Image processing, scikit-image, Python", ["Python", "OpenCV"], 680000, 2, 210),
        new(null, Kegiatan.Lomba, "Lomba Desain PCB Nasional", "PCB Designer", "Desain PCB, KiCad atau Altium Designer", ["PCB Design"], null, 1, 75),
        new(null, Kegiatan.Riset, "Deep Learning untuk Deteksi Retak Jalan", "Deep Learning Researcher", "Deep learning, PyTorch, GPU computing", ["PyTorch", "Python"], 850000, 2, 210),
        new(null, Kegiatan.Projek, "Pipeline CI/CD Lab Jaringan", "DevSecOps Engineer", "DevSecOps, Docker, Kubernetes, CI/CD pipeline", ["Docker", "Kubernetes"], 900000, 2, 200),
        new(null, Kegiatan.Lomba, "Business Case Competition", "Business Analyst Kompetisi", "Business case competition, analisis pasar", ["Business Process", "Power BI"], null, 3, 90),
        new(null, Kegiatan.Riset, "Implementasi Kriptografi Ringan", "Cryptography Researcher", "Kriptografi modern, implementasi algoritma", ["Kriptografi", "C++"], 630000, 1, 200),
        new(null, Kegiatan.Projek, "Robot Pengantar Dokumen Gedung", "Robotics Developer", "Robot otonom, ROS 2, Python", ["ROS", "Python"], 950000, 2, 210),
        new(null, Kegiatan.Lomba, "Game Jam 72 Jam", "Game Developer Kompetisi", "Game jam 72 jam, Unity atau Godot, tim 2-3", ["Unity", "C#"], null, 2, 45),
        new(null, Kegiatan.Riset, "Reinforcement Learning Lampu Lalu Lintas", "RL Researcher", "Reinforcement learning, OpenAI Gym, PyTorch", ["PyTorch", "Machine Learning"], 780000, 1, 210),
        new(null, Kegiatan.Projek, "Layanan Notifikasi Kampus", "Backend Microservices Engineer", "Microservices, Go, gRPC, Apache Kafka", ["Go", "Microservices"], 1100000, 2, 200),
        new(null, Kegiatan.Lomba, "Olimpiade Statistika Nasional", "Statistics Competitor", "Olimpiade statistika, inferensi dan regresi", ["Statistika", "R"], null, 2, 90),
        new(null, Kegiatan.Riset, "Sensor Network Smart Grid", "Smart Grid Researcher", "Smart grid, IoT sensor network, MQTT", ["IoT", "Raspberry Pi"], 700000, 1, 200),
        new(DemoNrp, Kegiatan.Lomba, "Hology 8.0 Web Development", "Frontend Developer", "React atau Vue, paham Git, aktif berdiskusi", ["React", "TypeScript"], null, 2, 70),
        new(null, Kegiatan.Projek, "MLOps untuk Model Presensi Wajah", "MLOps Engineer", "MLOps, model deployment, FastAPI, monitoring", ["Python", "Docker"], 1050000, 2, 210),
        new(null, Kegiatan.Lomba, "CTF Nasional Kategori Web & Pwn", "CTF Team Captain", "CTF nasional, tim 4 orang, kategori web & pwn", ["Linux", "Shell Scripting"], null, 3, 75),
        new(null, Kegiatan.Riset, "Transformasi Digital Proses Bisnis", "Digital Transform Researcher", "Digital transformation, analisis ERP & BPM", ["SAP", "Business Process"], 660000, 1, 210),
        new(null, Kegiatan.Projek, "Compiler Bahasa Edukasi", "Compiler Engineer", "Compiler design, LLVM IR, bahasa pemrograman", ["C++", "Algoritma"], 800000, 2, 200),
        new(null, Kegiatan.Lomba, "KRTI Drone Competition", "Drone Pilot Researcher", "KRTI drone competition, autonomous flight mode", ["ROS", "Python"], null, 3, 90),
        new(null, Kegiatan.Riset, "Spatial Computing untuk Pembelajaran", "Spatial Computing Researcher", "Spatial computing, WebXR, AR/MR development", ["Unity", "TypeScript"], 875000, 1, 210),
    };

    static readonly string[] Alasan =
    {
        "Ingin mendalami bidang ini dalam lingkungan riset nyata",
        "Tertarik membangun produk berskala besar bersama tim",
        "Ingin mengasah skill lomba bersama mentor berpengalaman",
        "Pengalaman saya cocok dengan topik ini",
        "Ingin belajar dari kakak tingkat yang sudah berpengalaman",
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

    // Mentor diambil dari mahasiswa angkatan 23; prestasinya karangan untuk demo.
    record SeedMentorMhs(string Nrp, string[] Skills, string Pengalaman);

    static readonly SeedMentorMhs[] MentorMahasiswa =
    {
        new("5025231171", ["Java", "Spring Boot", "System Design"],
            "Finalis Gemastik XVII kategori Pengembangan Perangkat Lunak\nBackend intern di startup edutech selama 6 bulan\nAsisten praktikum Pemrograman Berorientasi Objek"),
        new("5025231018", ["R", "Statistika", "Data Science"],
            "Juara 2 lomba analisis data tingkat nasional 2025\nAsisten praktikum Statistika selama 2 semester"),
        new("5025231036", ["Unity", "C#", "Game Development"],
            "Juara 1 game jam regional 2025\nMerilis dua game indie di itch.io"),
        new("5025231247", ["Linux", "Shell Scripting", "Networking"],
            "Top 10 CTF tingkat mahasiswa 2025\nAsisten Laboratorium Arsitektur dan Jaringan Komputer"),
        new("5025231288", ["Python", "Machine Learning", "Computer Vision"],
            "Juara 3 Kompetisi AI Nasional 2025\nAsisten praktikum Kecerdasan Buatan"),
        new("5025231127", ["C++", "Algoritma", "Competitive Programming"],
            "Finalis ICPC Asia Jakarta Regional 2025\nPengajar kelas persiapan olimpiade pemrograman di himpunan mahasiswa"),
        new("5025231272", ["Figma", "UI/UX", "User Research"],
            "Juara 1 lomba UI/UX tingkat nasional 2025\nUI/UX designer intern di perusahaan fintech selama 4 bulan"),
        new("5025231286", ["Go", "Microservices", "Cloud Computing"],
            "Finalis Hackathon Kota Cerdas 2025\nBackend engineer intern di startup logistik"),
    };

    public static void Seed(EtcDbContext db, DateTime? now = null)
    {
        if (db.Users.Any()) return;

        var today = (now ?? DateTime.Now).Date;
        var rng = new Random(20261015);
        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, 10);

        var mahasiswa = Mahasiswa.Select(m => new User
        {
            Nama = m.Nama,
            Jurusan = m.Jurusan,
            NoPengenal = m.Nrp,
            Angkatan = AngkatanDariNrp(m.Nrp),
            Spesialisasi = m.Spesialisasi.ToList(),
        }).ToList();

        var users = mahasiswa;
        for (int i = 0; i < users.Count; i++)
        {
            users[i].NoTelp = $"08123456{i:0000}";
            users[i].PasswordHash = hash;
            users[i].CreatedAt = users[i].UpdatedAt = today.AddDays(-400 + i);
        }
        db.Users.AddRange(users);

        var demo = mahasiswa.Single(m => m.NoPengenal == DemoNrp);
        // Di antara mahasiswa dengan keahlian paling cocok, yang paling sedikit membuat rekrutmen dipilih lebih dulu.
        var dibuat = new Dictionary<User, int>();
        User PickCreator(SeedRek s)
        {
            var creator = s.Nrp is { } nrp
                ? mahasiswa.Single(m => m.NoPengenal == nrp)
                : mahasiswa.Where(m => m != demo)
                    .OrderByDescending(m => m.Spesialisasi.Count(s.Skills.Contains))
                    .ThenBy(m => dibuat.GetValueOrDefault(m))
                    .First();
            dibuat[creator] = dibuat.GetValueOrDefault(creator) + 1;
            return creator;
        }
        int n = Rekrutmens.Length;

        for (int i = 0; i < n; i++)
        {
            var s = Rekrutmens[i];
            var creator = PickCreator(s);
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
                LinkDetail = s.Judul.StartsWith("Gemastik") ? "https://gemastik.kemdikbud.go.id" : null,
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
                    // Akun demo menyisakan anggota terakhir tanpa ulasan, supaya alur memberi ulasan bisa dicoba.
                    if (rng.Next(3) > 0 && !(ketua.UserId == demo.UserId && m == tim.Participants[^1]))
                        db.Histories.Add(NewReview(rng, ketua.UserId, m.UserId, tim, rek.TanggalSelesai));
                }
            }
        }

        foreach (var r in db.ChangeTracker.Entries<Rekrutmen>().Select(e => e.Entity)
                     .Where(r => r.IsOpen && r.UserId != demo.UserId).Take(3).ToList())
            db.Bookmarks.Add(new Bookmark { RekrutmenId = r.RekrutmenId, UserId = demo.UserId });

        SeedMentors(db, users, today);
        db.SaveChanges();
    }

    public static int? AngkatanDariNrp(string nrp) =>
        nrp.Length == 10 && nrp.All(char.IsDigit) ? 2000 + int.Parse(nrp[4..6]) : null;

    static void SeedMentors(EtcDbContext db, List<User> users, DateTime today)
    {
        foreach (var s in MentorMahasiswa)
        {
            var m = users.Single(u => u.NoPengenal == s.Nrp);
            m.IsMentor = true;
            m.Spesialisasi = s.Skills.ToList();
            m.Pengalaman = s.Pengalaman;
            m.LinkedInUrl = $"https://www.linkedin.com/in/{Slug(m.Nama)}";
            m.PortofolioUrl = $"https://github.com/{Slug(m.Nama)}";
        }

        var demo = users.Single(u => u.NoPengenal == DemoNrp);
        var mentors = users.Where(u => u.IsMentor).ToList();
        User M(string nrp) => mentors.First(u => u.NoPengenal == nrp);
        var mahasiswaLain = users.Where(u => !u.IsMentor && u != demo).ToList();

        db.MentorRequests.AddRange(
            new MentorRequest
            {
                MentorId = M("5025231288").UserId, UserId = demo.UserId, Kegiatan = Kegiatan.Lomba,
                Topik = "Persiapan Kompetisi AI Nasional",
                Pesan = "Tim kami butuh arahan memilih arsitektur model dan strategi validasi sebelum babak penyisihan.",
                Status = StatusBimbingan.Accepted, CreatedAt = today.AddDays(-20).AddHours(10), DecidedAt = today.AddDays(-18).AddHours(9),
            },
            new MentorRequest
            {
                MentorId = M("5025231171").UserId, UserId = demo.UserId, Kegiatan = Kegiatan.Projek,
                Topik = "Review arsitektur backend aplikasi peminjaman lab",
                Pesan = "Ingin minta masukan soal struktur API dan skema database sebelum mulai sprint pertama.",
                CreatedAt = today.AddDays(-2).AddHours(14),
            },
            new MentorRequest
            {
                MentorId = M("5025231247").UserId, UserId = mahasiswaLain[0].UserId, Kegiatan = Kegiatan.Lomba,
                Topik = "Strategi CTF kategori jaringan",
                Pesan = "Kami baru pertama kali ikut CTF dan ingin tahu materi jaringan apa yang perlu dipelajari dulu.",
                CreatedAt = today.AddDays(-1).AddHours(16),
            },
            new MentorRequest
            {
                MentorId = M("5025231247").UserId, UserId = mahasiswaLain[3].UserId, Kegiatan = Kegiatan.Riset,
                Topik = "Ide PKM keamanan perangkat IoT",
                Pesan = "Saya tertarik dengan keamanan perangkat IoT dan ingin berdiskusi soal ruang lingkup yang realistis.",
                CreatedAt = today.AddDays(-4).AddHours(11),
            });
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
