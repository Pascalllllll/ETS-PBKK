using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Data;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class AuthViewModel(AuthService auth) : ViewModelBase
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsLogin))] private bool _isRegister;
    public bool IsLogin => !IsRegister;

    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _loginId = "";
    [ObservableProperty] private string _loginPassword = "";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(StepTitle), nameof(IsStep1), nameof(IsStep2), nameof(IsStep3), nameof(StepText))]
    private int _step = 1;
    public bool IsStep1 => Step == 1;
    public bool IsStep2 => Step == 2;
    public bool IsStep3 => Step == 3;
    public string StepText => $"Langkah {Step} dari 3";
    public string StepTitle => Step switch
    {
        1 => "Siapa Anda?",
        2 => "Bagaimana tim menghubungi Anda?",
        _ => "Apa keahlian Anda?",
    };

    [ObservableProperty, NotifyPropertyChangedFor(nameof(NoPengenalLabel), nameof(NoPengenalFormat))] private bool _isDosen;
    public string NoPengenalLabel => IsDosen ? "NIP" : "NRP";
    public string NoPengenalFormat => IsDosen ? "18 digit, contoh 19810620 200501 1 003" : "10 digit, contoh 5025241XXX";
    [ObservableProperty] private string _nama = "";
    [ObservableProperty] private string _noPengenal = "";
    [ObservableProperty] private string? _jurusan;
    [ObservableProperty] private string _noTelp = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _confirmPassword = "";
    public TagListViewModel Skills { get; } = new();
    public IReadOnlyList<string> JurusanOptions => Labels.Jurusan;

    public string DemoHint => $"Akun demo memakai kata sandi {Seeder.DemoPassword}.";

    [RelayCommand]
    async Task Login()
    {
        Error = null;
        IsBusy = true;
        var r = await auth.LoginAsync(LoginId, LoginPassword);
        IsBusy = false;
        if (!r.Ok) Error = r.Error;
    }

    [RelayCommand]
    void FillDemo(string kind)
    {
        LoginId = kind == "dosen" ? "19810620 200501 1 003" : "5025241177";
        LoginPassword = Seeder.DemoPassword;
        Error = null;
    }

    [RelayCommand]
    void ShowRegister() { IsRegister = true; Step = 1; Error = null; }

    [RelayCommand]
    void ShowLogin() { IsRegister = false; Error = null; }

    [RelayCommand]
    void Next()
    {
        Error = Step switch
        {
            1 => AuthService.ValidateIdentity(Nama, IsDosen ? UserRole.Dosen : UserRole.Mahasiswa, NoPengenal),
            2 => AuthService.ValidateContact(NoTelp),
            _ => null,
        };
        if (Error is null && Step < 3) Step++;
    }

    [RelayCommand]
    void Back()
    {
        Error = null;
        if (Step > 1) Step--;
        else ShowLogin();
    }

    [RelayCommand]
    async Task Register()
    {
        Error = AuthService.ValidatePassword(Password, ConfirmPassword);
        if (Error is not null) return;
        IsBusy = true;
        var r = await auth.RegisterAsync(new RegisterRequest(Nama, IsDosen ? UserRole.Dosen : UserRole.Mahasiswa,
            NoPengenal, Jurusan, NoTelp, Skills.Items.ToList(), Password, ConfirmPassword));
        IsBusy = false;
        if (!r.Ok) Error = r.Error;
    }
}
