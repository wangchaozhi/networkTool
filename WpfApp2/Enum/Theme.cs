namespace WpfApp2;

public enum Theme
{
    Default,
    Transparent,
    Wheat,
    // 可以继续添加其他主题
}
public static class ThemeExtensions
{
    public static Theme ParseFromString(string themeValue)
    {
        switch (themeValue)
        {
            case "Default":
                return Theme.Default;
            case "Transparent":
                return Theme.Transparent;
            case "Wheat":
                return Theme.Wheat;
            // 添加其他主题的解析逻辑
            default:
                return Theme.Default; // 默认主题
        }
    }
}