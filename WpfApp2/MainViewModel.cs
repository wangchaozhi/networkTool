using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp2;

public class MainViewModel : INotifyPropertyChanged
{
    private bool _isWindowVisible;
    private string _message;
    private SolidColorBrush _borderBackgroundColor = new SolidColorBrush(Colors.White); // 默认背景色
    
    

    private  readonly IWindowService windowService;
    public string Message
    {
        get { return _message; }
        set { _message = value; OnPropertyChanged(); }
    }

    
    public bool IsWindowVisible
    {
        get { return _isWindowVisible; }
        set
        {
            _isWindowVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WindowVisibilityText));
        }
    }
    
    public ICommand ToggleWindowCommand { get; private set; }
    public ICommand ExitCommand { get; private set; }
    public ICommand UpdateMessageCommand { get; }
    
    
    
    
    public MainViewModel(IWindowService windowService)
    {
        this.windowService = windowService;
        UpdateMessageCommand = new RelayCommand(UpdateMessage);
        ToggleWindowCommand = new RelayCommand(ToggleWindow);
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        Message = "Initial message";
        IsWindowVisible = true;
    }
    
    
    public SolidColorBrush BorderBackgroundColor
    {
        get => _borderBackgroundColor;
        set
        {
            if (_borderBackgroundColor != value)
            {
                _borderBackgroundColor = value;
                OnPropertyChanged();
            }
        }
    }
    
    private void ToggleWindow()
    {
        IsWindowVisible = !IsWindowVisible;
        if (IsWindowVisible)
            windowService.ShowWindow();
        else
            windowService.HideWindow();
    }

    public string WindowVisibilityText => IsWindowVisible ? "隐藏" : "打开";
    
    
    
    
    private void SetDefaultTheme()
    {
        BorderBackgroundColor = new SolidColorBrush(Colors.White); // 默认主题背景色
        // 其他默认主题相关设置
    }

    private void SetTransparentTheme()
    {
        BorderBackgroundColor = new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF)); // 近乎透明的白色
        // 其他透明主题相关设置
    }
    
    private void SetWheatTheme()
    {
        BorderBackgroundColor = new SolidColorBrush(Colors.Wheat); 
        // 其他透明主题相关设置
    }
    
    private Theme _selectedTheme = Theme.Default;  // 默认选中的主题

    public Theme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                NotifyPropertyChanged(nameof(SelectedTheme));
                UpdateTheme(value);  // 更新主题的方法
            }
        }
    }

    private void UpdateTheme(Theme theme)
    {
        switch (theme)
        {
            case Theme.Default:
                SetDefaultTheme();
                break;
            case Theme.Transparent:
                SetTransparentTheme();
                break;
            case Theme.Wheat:
                SetWheatTheme();
                break;
            // 添加其他主题的处理逻辑
        }
    }

    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(IsWindowVisible))
            CommandManager.InvalidateRequerySuggested();
    }
    
    protected virtual void NotifyPropertyChanged(params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    private void UpdateMessage()
    {
        Message = "Message updated at " + DateTime.Now.ToString("T");
    }

}