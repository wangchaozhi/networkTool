using System;
using Newtonsoft.Json.Linq;
using System.IO;
using WpfApp2;

public class ConfigurationManager
{
    private readonly string _jsonFilePath;
    private JObject _settings;

    public ConfigurationManager(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;

        if (File.Exists(_jsonFilePath))
        {
            try
            {
                string json = File.ReadAllText(_jsonFilePath);
                _settings = JObject.Parse(json);
                EnsureSettingsShape();
            }
            catch
            {
                BackupInvalidSettingsFile();
                _settings = CreateDefaultSettings();
                SaveSettings();
            }
        }
        else
        {
            _settings = CreateDefaultSettings();
            SaveSettings();
        }
    }

    public string GetVersion()
    {
        var version = _settings["Version"];
        return version != null ? version.ToString() : ApplicationInfo.Version; // 如果版本号未设置，则返回未知
    }

    public void SetVersion(string version)
    {
        _settings["Version"] = version;
        SaveSettings();
    }


    public void SetSetting<T>(string settingName, T value)
    {
        var settings = GetSettings();
        settings[settingName] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
        SaveSettings();
    }

    public T GetSetting<T>(string settingName)
    {
        var settingValue = GetSettings()[settingName];
        if (settingValue != null)
        {
            // 使用 ToObject<T>() 来转换 JSON 数据到指定类型
            return settingValue.ToObject<T>()!;
        }

        var defaultSettings = (JObject)CreateDefaultSettings()["Settings"]!;
        var defaultValue = defaultSettings[settingName];
        return defaultValue != null ? defaultValue.ToObject<T>()! : default!;
    }

    private void SaveSettings()
    {
        string json = _settings.ToString(Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(_jsonFilePath, json);
    }

    private static JObject CreateDefaultSettings()
    {
        return new JObject
        {
            ["Version"] = ApplicationInfo.Version,
            ["Settings"] = JObject.FromObject(new
            {
                Theme = "Default",
                FontStyle = "Arial",
                Scale = 1.0,
                RadiusValue = 25,
                Icon = "Default"
            })
        };
    }

    private void EnsureSettingsShape()
    {
        var defaults = CreateDefaultSettings();

        if (_settings["Version"] == null)
        {
            _settings["Version"] = defaults["Version"]!.DeepClone();
        }

        if (_settings["Settings"] is not JObject settings)
        {
            _settings["Settings"] = defaults["Settings"]!.DeepClone();
            return;
        }

        foreach (var property in defaults["Settings"]!.Children<JProperty>())
        {
            if (settings[property.Name] == null)
            {
                settings[property.Name] = property.Value.DeepClone();
            }
        }
    }

    private JObject GetSettings()
    {
        EnsureSettingsShape();
        return (JObject)_settings["Settings"]!;
    }

    private void BackupInvalidSettingsFile()
    {
        string backupPath = $"{_jsonFilePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
        File.Copy(_jsonFilePath, backupPath, true);
    }
}
