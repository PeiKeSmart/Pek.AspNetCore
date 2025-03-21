﻿using Microsoft.Extensions.Options;

using NewLife.Serialization;

namespace Pek.Configs;

/// <summary>
/// 配置文件管理器
/// </summary>
public static class ConfigFileHelper
{
    private static IConfiguration? _config;

    /// <summary>
    /// 得到对象属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? Get<T>(String? name = null) where T : class, new()
    {
        try
        {
            //节点名称
            var sectionName = String.IsNullOrWhiteSpace(name) ? typeof(T).Name : name;
            if (typeof(T).IsGenericType)
            {
                var genericArgTypes = typeof(T).GetGenericArguments();
                sectionName = genericArgTypes[0].Name;
            }
            //判断配置文件是否有节点
            if (_config?.GetChildren().FirstOrDefault(i => i.Key == sectionName) == null)
                return null;

            //节点对象反序列化
            var spList = new ServiceCollection().AddOptions()
                                           .Configure<T>(options => _config.GetSection(sectionName))
                                           .BuildServiceProvider();
            return spList.GetService<IOptionsMonitor<T>>()?.CurrentValue;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 得到简单类型的属性
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static String? Get(String sectionName) => _config?.GetSection(sectionName).Value;

    /// <summary>
    /// 设置配置项
    /// </summary>
    /// <param name="file"></param>
    /// <param name="env"></param>
    public static void Set(String file = "appsettings.json", IHostEnvironment? env = null)
    {
        if (env != null)
        {
            _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile(file, true, true)
                     .AddJsonFile($"{file.Split(['.'], StringSplitOptions.RemoveEmptyEntries)[0]}.{env.EnvironmentName}.json", true)
                     .Build();
        }
        else
        {
            _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile(file, true, true)
                     .Build();
        }
    }

    #region 配置文件导入
    /// <summary>
    /// 从Settings导入配置文件
    /// </summary>
    public static void SetConfig(IConfigurationBuilder config)
    {
        var dir = Path.GetFullPath(AppContext.BaseDirectory);
        var settingsFolder = Path.Combine(dir, "Settings");

        // 查找 Settings目录
        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/Data");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/..");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/bin");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (Directory.Exists(settingsFolder))
        {
            var settings = Directory.GetFiles(settingsFolder, "*.json");
            settings.ToList().ForEach(setting =>
            {
                config.AddJsonFile(setting, optional: false, reloadOnChange: true);
            });
        }
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// 从Settings导入配置文件
    /// </summary>
    public static void SetConfig(this ConfigurationManager config)
    {
        var dir = Path.GetFullPath(AppContext.BaseDirectory);
        var settingsFolder = Path.Combine(dir, "Settings");

        // 查找 Settings目录
        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/Data");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/..");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (!Directory.Exists(settingsFolder))
        {
            dir = Path.GetFullPath(AppContext.BaseDirectory + "/bin");
        }

        settingsFolder = Path.Combine(dir, "Settings");

        if (Directory.Exists(settingsFolder))
        {
            var settings = Directory.GetFiles(settingsFolder, "*.json");
            settings.ToList().ForEach(setting =>
            {
                config.AddJsonFile(setting, optional: false, reloadOnChange: true);
            });
        }
    }
#endif

    #endregion

    #region 编辑appsettings.json
    public static Object? GetAppSetting(String key, String? filePath = null)
    {
        if (filePath == null)
        {
            filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        }
        else
        {
            filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        }

        var json = File.ReadAllText(filePath);
        var jsonObj = JsonParser.Decode(json);

        var strArray = key.Split(":");

        if (strArray.Length == 1)
        {
            return jsonObj?[key];
        }
        else if (strArray.Length == 2)
        {
            if (jsonObj?.ContainsKey(strArray[0]) == false)
            {
                return null;
            }
            var dic = jsonObj?[strArray[0]]?.ToDictionary(); 
            return dic?[strArray[1]];
        }
        else if (strArray.Length == 3)
        {
            if (jsonObj?.ContainsKey(strArray[0]) == false)
            {
                return null;
            }
            var dic = jsonObj?[strArray[0]]?.ToDictionary();
            if (dic?.ContainsKey(strArray[1]) == false)
            {
                return null;
            }
            var dic1 = dic?[strArray[1]]?.ToDictionary();
            return dic1?[strArray[2]];
        }

        return null;
    }

    /// <summary>
    /// 增加或者更新AppSetting文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="filePath"></param>
    public static void AddOrUpdateAppSetting<T>(String key, T value, String? filePath = null)
    {
        if (filePath == null)
        {
            filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        }
        else
        {
            filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        }

        var json = File.ReadAllText(filePath);
        var jsonObj = JsonParser.Decode(json)!;

        var strArray = key.Split(":");

        if (strArray.Length == 1)
        {
            jsonObj[key] = value;
        }
        else if (strArray.Length == 2)
        {
            if (!jsonObj.ContainsKey(strArray[0]))
            {
                jsonObj[strArray[0]] = new Dictionary<String, Object>();
            }
            var dic = jsonObj[strArray[0]]!.ToDictionary();
            dic[strArray[1]] = value;
            jsonObj[strArray[0]] = dic;
        }
        else if (strArray.Length == 3)
        {
            if (!jsonObj.ContainsKey(strArray[0]))
            {
                jsonObj[strArray[0]] = new Dictionary<String, Object>();
            }
            var dic = jsonObj[strArray[0]]!.ToDictionary();
            if (!dic.ContainsKey(strArray[1]))
            {
                dic[strArray[1]] = new Dictionary<String, Object>();
            }
            var dic1 = dic[strArray[1]]!.ToDictionary();
            dic1[strArray[2]] = value;
            dic[strArray[1]] = dic1;
            jsonObj[strArray[0]] = dic;
        }

        var output = jsonObj.ToJson(true);

        File.WriteAllText(filePath, output);
    }
    #endregion
}