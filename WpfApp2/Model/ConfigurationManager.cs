using Newtonsoft.Json.Linq;
using System.IO;

public class ConfigurationManager
{
    private readonly string _jsonFilePath;
    private JObject _settings;

    
    public ConfigurationManager(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;

        if (File.Exists(_jsonFilePath))
        {
            string json = File.ReadAllText(_jsonFilePath);
            _settings = JObject.Parse(json);
        }
        else
        {
            // 如果文件不存在，创建一个新的 JObject 并设置默认值
            _settings = new JObject();
            _settings["Settings"] = JObject.FromObject(new { Theme = "Default", FontStyle = "Arial" });
            SaveSettings();
        }
    }


    public string GetSetting(string settingName)
    {
        return _settings["Settings"][settingName]?.ToString();
    }

    public void SetSetting(string settingName, string value)
    {
        _settings["Settings"][settingName] = value;
        SaveSettings();
    }

    private void SaveSettings()
    {
        string json = _settings.ToString(Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(_jsonFilePath, json);
    }
}