using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static JFLog;

public class JFLocalSave
{
    static private JFLocalSave _instance;
    static public JFLocalSave Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new JFLocalSave();
            }

            return _instance;
        }
    }

    private Dictionary<string, object> _data;
    private static string _localSavePath = $"{Application.persistentDataPath}/local.sav";
    public static string LocalSavePath => _localSavePath;
    
    private JFLocalSave()
    {
        _data = new Dictionary<string, object>();
        if (File.Exists(_localSavePath))
        {
            StreamReader reader = new StreamReader(_localSavePath);
            var data = JFSerialize.Deserialize(reader.ReadToEnd(), out var hasError);
            if (!hasError)
            {
                _data = (Dictionary<string, object>)JFJson.Deserialize(data);
                reader.Close();
            }
            else
            {
                JFLog.Error(LogTag.LOCALSAVE, $"Can not read local user data at: {_localSavePath}. Exeption: {data}");
                JFLog.Error(LogTag.LOCALSAVE, $"Try to create new database");
                reader.Close();
                var file = new FileStream(_localSavePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                file.Write(new byte[0], 0, 0);
                file.Close();
            }

        }
        else
        {
            var file = new FileStream(_localSavePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            file.Close();
        }
    }


    private void Save()
    {
        StreamWriter writer = new StreamWriter(_localSavePath, false);
        writer.Write(JFSerialize.Serialize(JFJson.Serialize(_data)));
        writer.Close();
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_data.ContainsKey(key))
        {
            return Convert.ToInt32(_data[key]);
        }
        return defaultValue;
    }

    public void SetInt(string key, int value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
        Save();
    }

    public void SetInt(string key, long value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
        Save();
    }

    public float GetFloat(string key, float defaultValue = 0.0f)
    {
        if (_data.ContainsKey(key))
        {
            return Convert.ToSingle(_data[key]);
        }
        return defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
        Save();
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (_data.ContainsKey(key))
        {
            return Convert.ToString(_data[key]);
        }
        return defaultValue;
    }

    public void SetString(string key, string value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
        Save();
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_data.ContainsKey(key))
        {
            return Convert.ToBoolean(_data[key]);
        }
        return defaultValue;
    }
    public void SetBool(string key, bool value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
        Save();
    }

    public void SetBool(string key, int value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value != 0;
        }
        else
        {
            _data.Add(key, value != 0);
        }
        Save();
    }

    public void SetObject(string key, Dictionary<string, dynamic> dict)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = dict;
        }
        else
        {
            _data.Add(key, dict);
        }
        Save();
    }

    public Dictionary<string, dynamic> GetObject(string key)
    {
        if (_data.ContainsKey(key))
        {
            return (Dictionary<string, dynamic>)_data[key];
        }
        return new Dictionary<string, dynamic>();
    }

    public bool HasKey(string key)
    {
        return _data.ContainsKey(key);
    }
}
