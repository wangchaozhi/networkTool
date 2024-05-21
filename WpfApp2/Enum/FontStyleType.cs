using System;

namespace WpfApp2;

public enum FontStyleType
{
    Arial,
    TimesNewRoman,
    楷体,
    宋体,
    黑体
    // 添加更多字体样式
}
public static class FontStyleTypeExtensions
{
    public static FontStyleType ParseFromString(string fontStyleValue)
    {
        switch (fontStyleValue)
        {
            case "Arial":
                return FontStyleType.Arial;
            case "TimesNewRoman":
                return FontStyleType.TimesNewRoman;
            case "楷体":
                return FontStyleType.楷体;
            case "宋体":
                return FontStyleType.宋体;
            case "黑体":
                return FontStyleType.黑体;
            default:
                throw new ArgumentException($"Invalid font style value: {fontStyleValue}");
        }
    }
}