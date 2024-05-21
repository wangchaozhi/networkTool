using System;
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
            _settings["Settings"] = JObject.FromObject(new { Theme = "Default", FontStyle = "Arial",Scale="1.0" });
            SaveSettings();
        }
    }


   
    
    public void SetSetting<T>(string settingName, T value)
    {
        _settings["Settings"][settingName] = value.ToString();
        SaveSettings();
    }

    
    public T GetSetting<T>(string settingName)
    {
        var settingValue = _settings["Settings"][settingName];
        if (settingValue != null)
        {
            return (T)Convert.ChangeType(settingValue, typeof(T));
        }
        return default(T);  // 返回类型的默认值，例如对于int是0，对于bool是false等
    }


    private void SaveSettings()
    {
        string json = _settings.ToString(Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(_jsonFilePath, json);
    }
}