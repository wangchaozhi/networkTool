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
    
    public ICommand SetDefaultThemeCommand { get; private set; }
    public ICommand SetTransparentThemeCommand { get; private set; }
    
    
    



    
    
    public MainViewModel(IWindowService windowService)
    {
        this.windowService = windowService;
        UpdateMessageCommand = new RelayCommand(UpdateMessage);
        ToggleWindowCommand = new RelayCommand(ToggleWindow);
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        SetDefaultThemeCommand = new RelayCommand(() => SetTheme(true));
        SetTransparentThemeCommand = new RelayCommand(() => SetTheme(false));
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
    
    private void SetTheme(bool isDefault)
    {
        IsDefaultThemeSelected = isDefault;
        IsTransparentThemeSelected = !isDefault;
        if (isDefault)
        {
            SetDefaultTheme();
        }
        else
        {
            SetTransparentTheme();
        }


        // 实际的主题应用逻辑
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

    
    
    private bool _isDefaultThemeSelected = true; // 假设默认主题是初始选中的
    private bool _isTransparentThemeSelected = false;

    // public bool IsDefaultThemeSelected
    // {
    //     get => _isDefaultThemeSelected;
    //     set
    //     {
    //         if (_isDefaultThemeSelected != value)
    //         {
    //             _isDefaultThemeSelected = value;
    //             OnPropertyChanged();
    //         }
    //     }
    // }
    //
    // public bool IsTransparentThemeSelected
    // {
    //     get => _isTransparentThemeSelected;
    //     set
    //     {
    //         if (_isTransparentThemeSelected != value)
    //         {
    //             _isTransparentThemeSelected = value;
    //             OnPropertyChanged();
    //         }
    //     }
    // }
    //
    public bool IsDefaultThemeSelected
    {
        get => _isDefaultThemeSelected;
        set
        {
            if (_isDefaultThemeSelected != value)
            {
                _isDefaultThemeSelected = value;
                IsTransparentThemeSelected = !value; // 自动反转另一个选项
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTransparentThemeSelected));
            }
        }
    }

    public bool IsTransparentThemeSelected
    {
        get => _isTransparentThemeSelected;
        set
        {
            if (_isTransparentThemeSelected != value)
            {
                _isTransparentThemeSelected = value;
                IsDefaultThemeSelected = !value; // 自动反转另一个选项
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDefaultThemeSelected));
            }
        }
    }


    
    
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(IsWindowVisible))
            CommandManager.InvalidateRequerySuggested();
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

}