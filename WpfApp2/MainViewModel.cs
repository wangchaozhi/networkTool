using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp2;

public class MainViewModel : INotifyPropertyChanged
{
    private bool _isWindowVisible;
    private string _message;

    private  readonly IWindowService windowService;
    public string Message
    {
        get { return _message; }
        set { _message = value; OnPropertyChanged(); }
    }

    public bool IsWindowVisible
    {
        get { return _isWindowVisible; }
        set { _isWindowVisible = value; OnPropertyChanged(); }
    }

    public ICommand OpenCommand { get; private set; }
    public ICommand ExitCommand { get; private set; }
    public ICommand UpdateMessageCommand { get; }

    public MainViewModel(IWindowService windowService)
    {
        this.windowService = windowService;
        UpdateMessageCommand = new RelayCommand(UpdateMessage);
        OpenCommand = new RelayCommand(OpenWindow, () => !IsWindowVisible);
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        Message = "Initial message";
        IsWindowVisible = true;
    }

    private void OpenWindow()
    {
        IsWindowVisible = true;
        windowService.ShowWindow();
    }

    private void UpdateMessage()
    {
        Message = "Message updated at " + DateTime.Now.ToString("T");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(IsWindowVisible))
            CommandManager.InvalidateRequerySuggested();
    }
}