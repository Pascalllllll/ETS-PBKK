using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public abstract class ViewModelBase : ObservableObject;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    [ObservableProperty] private ViewModelBase _current;

    public MainViewModel(IServiceProvider sp, Session session)
    {
        _sp = sp;
        _current = Create<AuthViewModel>();
        session.Changed += () =>
        {
            // Update profil tidak perlu membangun ulang shell.
            if (session.CurrentUser is null) Current = Create<AuthViewModel>();
            else if (Current is not ShellViewModel) Current = Create<ShellViewModel>();
        };
    }

    T Create<T>() where T : notnull =>
        Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<T>(_sp);
}

public partial class TagListViewModel : ViewModelBase
{
    public static readonly string[] CommonSkills =
    {
        "Python", "Machine Learning", "Data Science", "React", "Node.js", "UI/UX", "Figma", "Flutter",
        "Go", "Java", "C++", "C#", "Unity", "IoT", "Arduino", "Statistika", "R", "Docker", "Linux", "Networking",
    };

    public ObservableCollection<string> Items { get; }
    public ObservableCollection<string> Suggestions { get; } = new();

    [ObservableProperty] private string _input = "";

    public TagListViewModel(IEnumerable<string>? initial = null)
    {
        Items = new ObservableCollection<string>(initial ?? []);
        Items.CollectionChanged += (_, _) => RefreshSuggestions();
        RefreshSuggestions();
    }

    partial void OnInputChanged(string value) => RefreshSuggestions();

    void RefreshSuggestions()
    {
        Suggestions.Clear();
        var q = Input.Trim();
        foreach (var s in CommonSkills
                     .Where(s => !Items.Contains(s, StringComparer.OrdinalIgnoreCase))
                     .Where(s => q.Length == 0 || s.Contains(q, StringComparison.OrdinalIgnoreCase))
                     .Take(8))
            Suggestions.Add(s);
    }

    [RelayCommand]
    void Add(string? value)
    {
        var v = (value ?? Input).Trim().TrimEnd(',');
        if (v.Length == 0 || v.Length > 30) return;
        if (!Items.Contains(v, StringComparer.OrdinalIgnoreCase) && Items.Count < 10) Items.Add(v);
        Input = "";
    }

    [RelayCommand]
    void Remove(string value) => Items.Remove(value);
}

public abstract partial class SheetViewModel : ViewModelBase
{
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    public Action? Close { get; set; }

    [RelayCommand]
    protected void Cancel() => Close?.Invoke();
}

public partial class ConfirmSheetViewModel(string title, string message, string confirmLabel, bool destructive,
    Func<Task<Result>> onConfirm) : SheetViewModel
{
    public string Title { get; } = title;
    public string Message { get; } = message;
    public string ConfirmLabel { get; } = confirmLabel;
    public bool IsDestructive { get; } = destructive;

    [RelayCommand]
    async Task Confirm()
    {
        IsBusy = true;
        var r = await onConfirm();
        IsBusy = false;
        if (r.Ok) Close?.Invoke();
        else Error = r.Error;
    }
}
