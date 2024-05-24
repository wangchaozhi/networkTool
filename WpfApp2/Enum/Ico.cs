using System.Collections.Generic;

namespace WpfApp2;

public enum Icon
{
    Default,
    Brown,
    // 可以继续添加其他图标
}

public static class IconExtensions
{
    public static Icon ParseFromString(string iconValue)
    {
        switch (iconValue)
        {
            case "Default":
                return Icon.Default;
            case "Brown":
                return Icon.Brown;
            // 添加其他图标的解析逻辑
            default:
                return Icon.Default; // 默认图标
        }
    }
}

public class IconData
{
    public Icon Type { get; set; }
    public string UpIconPath { get; set; }
    public string DownIconPath { get; set; }

    public IconData(Icon type, string upIconPath, string downIconPath)
    {
        Type = type;
        UpIconPath = upIconPath;
        DownIconPath = downIconPath;
    }
}

public static class IconManager
{
    public static List<IconData> Icons = new List<IconData>
    {
        new IconData(Icon.Default, "pack://application:,,,/res/up.png", "pack://application:,,,/res/down.png"),
        new IconData(Icon.Brown, "pack://application:,,,/res/up1.png", "pack://application:,,,/res/down2.png")
    };
}