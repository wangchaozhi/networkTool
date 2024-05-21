using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfApp2;


public class MainViewModel : INotifyPropertyChanged
{
    private bool _isWindowVisible;
    private string _message;
    private SolidColorBrush _borderBackgroundColor = new SolidColorBrush(Colors.White); // 默认背景色
    private readonly ConfigurationManager _configManager;
    
    

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
    
    
    
    
    public MainViewModel(IWindowService windowService,ConfigurationManager configManager)
    {
        this.windowService = windowService;
        _configManager = configManager;
        var fontStyleSetting = _configManager.GetSetting("FontStyle");
        CurrentFontStyle = FontStyleTypeExtensions.ParseFromString(fontStyleSetting);
        
        
        // 从配置文件中获取默认主题值，并转换为 Theme 枚举类型
        var themeSetting = _configManager.GetSetting("Theme");
        SelectedTheme = ThemeExtensions.ParseFromString(themeSetting);
        
        
        
        // 从配置文件中获取默认主题值，并转换为 Theme 枚举类型
        var scaleSetting = _configManager.GetSetting("Scale");
        if (double.TryParse(scaleSetting, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            CurrentScale = Math.Round(scale, 1);
        }

        
        UpdateMessageCommand = new RelayCommand(UpdateMessage);
        ToggleWindowCommand = new RelayCommand(ToggleWindow);
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        Message = "Initial message";
        IsWindowVisible = true;
        // 初始化定时器
        _visibilityTimer = new DispatcherTimer();
        _visibilityTimer.Interval = TimeSpan.FromSeconds(1);
        _visibilityTimer.Tick += VisibilityTimer_Tick;
        // ShowLabelWithDelay();
    }
    
    // private async Task ShowLabelWithDelay()
    // {
    //     LabelVisibility = Visibility.Visible;
    //     await Task.Delay(2000); // 等待2秒
    //     LabelVisibility = Visibility.Collapsed;
    // }
    
    
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
        _configManager.SetSetting("Theme", theme.ToString());
    }
    
    
    
    
    private FontFamily _fontFamily = new FontFamily("Arial");
    public FontFamily FontFamily
    {
        get => _fontFamily;
        set
        {
            if (_fontFamily != value)
            {
                _fontFamily = value;
                NotifyPropertyChanged(nameof(FontFamily));
            }
        }
    }
    
   
    
    private FontStyleType _currentFontStyle = FontStyleType.Arial; // 默认值
    
    public FontStyleType CurrentFontStyle
    {
        get => _currentFontStyle;
        set
        {
            if (_currentFontStyle != value)
            {
                _currentFontStyle = value;
                NotifyPropertyChanged(nameof(CurrentFontStyle));
                UpdateFontStyle(value);  // 直接更新字体样式
            }
        }
    }
    
    private void UpdateFontStyle(FontStyleType fontStyle)
    {
        switch (fontStyle)
        {
            case FontStyleType.Arial:
                FontFamily = new FontFamily("Arial");
                break;
            case FontStyleType.TimesNewRoman:
                FontFamily = new FontFamily("Times New Roman");
                break;
            case FontStyleType.楷体:
                FontFamily = new FontFamily("楷体");
                break;
            case FontStyleType.宋体:
                FontFamily = new FontFamily("宋体");
                break; 
            case FontStyleType.黑体:
                FontFamily = new FontFamily("黑体");
                break;
            // 添加其他字体样式的处理逻辑
        }
        _configManager.SetSetting("FontStyle", fontStyle.ToString());
       
    }
    
    private double _currentScale = 1.0; // 默认缩放比例为1倍
    
    public double CurrentScale
    {
        get => _currentScale;
        set
        {
            if (_currentScale != value)
            {
                _currentScale = value;
                NotifyPropertyChanged(nameof(CurrentScale));
                UpdateScale(value);  // Correctly calls UpdateScale without recursion
            }
        }
    }
    private readonly double baseWidth = 136; // 初始窗口宽度
    private readonly double baseHeight = 68; // 初始窗口高度

    private void UpdateScale(double scale)
    {
        // 根据缩放比例调整窗口的宽度和高度
        double newWidth = baseWidth * scale;
        double newHeight = baseHeight * scale;

        // 使用 WindowService 调整窗口尺寸
        windowService.ResizeWindow(newWidth, newHeight);
        // 更新 ViewModel 中的缩放比例，如果需要通知UI变化
        CurrentScale = scale;
        _configManager.SetSetting("Scale",scale.ToString("F1") );
    }

    
    
    
    private int _cornerRadiusValue = 25;
    private Visibility _labelVisibility = Visibility.Collapsed;
    private DispatcherTimer _visibilityTimer;
    
    
    public int CornerRadiusValue
    {
        get { return _cornerRadiusValue; }
        set
        {
            if (_cornerRadiusValue != value)
            {
                _cornerRadiusValue = value;
                OnPropertyChanged(nameof(CornerRadiusValue));
                LabelVisibility = Visibility.Visible; // 显示标签
                _visibilityTimer.Stop(); // 停止前一个定时器
                _visibilityTimer.Start(); // 开始定时器，准备隐藏标签
            }
        }
    }

    public Visibility LabelVisibility
    {
        get { return _labelVisibility; }
        set
        {
            if (_labelVisibility != value)
            {
                _labelVisibility = value;
                OnPropertyChanged(nameof(LabelVisibility));
            }
        }
    }

    private void VisibilityTimer_Tick(object sender, EventArgs e)
    {
        LabelVisibility = Visibility.Collapsed; // 隐藏标签
        _visibilityTimer.Stop();
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