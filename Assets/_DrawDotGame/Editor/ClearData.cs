using System.IO;
using UnityEditor;
using UnityEngine;

public class ClearData
{
    [MenuItem("JuiceFruit/Clear Data/Clear PlayerPrefs")]
    static void ClearGamePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    [MenuItem("JuiceFruit/Clear Data/Clear Local Database")]
    static void ClearLocalData()
    {
        var path = JFLocalSave.LocalSavePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    [MenuItem("JuiceFruit/Clear Data/Clear All")]
    static void ClearAll()
    {
        ClearGamePlayerPrefs();
        ClearLocalData();
    }
}
