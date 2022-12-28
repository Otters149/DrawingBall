using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class A3EBuildEditor : EditorWindow
{
    public string buildnumber;
    public static string appVersion;
    public static string originalSymbol = "";

    //@a3e_help: todo upgrade editor
    public static List<string> scenes = new List<string>
    {
        //Example: "Assets/_ProjectAssets/Scenes/InitScene.unity"
        "Assets/_DrawDotGame/Scenes/FirstScene.unity",
        "Assets/_DrawDotGame/Scenes/GameScene.unity"
    };

    [MenuItem("A3E/Builds/Android")]
    private static void Init()
    {
        A3EBuildEditor window = (A3EBuildEditor)GetWindow(typeof(A3EBuildEditor));
        window.name = "OttersBuildTool";
        appVersion = string.Join("", Application.version.Split('.'));
        DateTime now = DateTime.Now;
        int nowInt = ((now.Year % 100) * 100 + now.Month) * 100 + now.Day;
        int preBuildnumber = EditorPrefs.GetInt("BuildNumber", nowInt * 100);
        if (preBuildnumber % 100 == nowInt)
        {
            window.buildnumber = "" + (preBuildnumber + 1);
        }
        else
        {
            window.buildnumber = "" + (nowInt * 100 + 1);
        }
        EditorPrefs.SetInt("BuildNumber", Convert.ToInt32(window.buildnumber));
    }

    private void OnGUI()
    {
        buildnumber = EditorGUILayout.TextField("Build Number", buildnumber);
        if (string.IsNullOrEmpty(buildnumber))
        {
            return;
        }

        PlayerSettings.Android.bundleVersionCode = int.Parse(buildnumber);

        if (GUILayout.Button("Build Dev"))
        {
            EditorPrefs.SetInt("BuildNumber", Convert.ToInt32(buildnumber));
            A3EBuildConfigure.AddDebugScripting();
            originalSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, RemoveScriptingSymbols(originalSymbol, "PRODUCTION"));

            BuildAndroid("Dev", BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.SymlinkSources | BuildOptions.WaitForPlayerConnection);

            buildnumber = "" + (EditorPrefs.GetInt("BuildNumber") + 1);
        }

        if (GUILayout.Button("Build Production"))
        {
            EditorPrefs.SetInt("BuildNumber", Convert.ToInt32(buildnumber));
            A3EBuildConfigure.RemoveDebugScripting();
            originalSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, AddScriptingSymbols(originalSymbol, "PRODUCTION"));

            BuildAndroid("Prod", BuildOptions.None);

            buildnumber = "" + (EditorPrefs.GetInt("BuildNumber") + 1);
        }
    }

    public static string RemoveScriptingSymbols(string original, params string[] symbol)
    {
        var result = original;
        foreach (var sym in symbol)
            result = RemoveScriptingSymbols(result, sym);
        return result;
    }

    public static string RemoveScriptingSymbols(string original, string symbol)
    {
        var symbols = original.Split(';');
        string newsymbol = "";

        for (int i = 0; i < symbols.Length; i++)
            if (symbols[i] != symbol)
            {
                newsymbol += ";" + symbols[i];
            }
        newsymbol.Remove(0, 1);
        return newsymbol;
    }
    public static string AddScriptingSymbols(string original, params string[] symbol)
    {
        var result = original;
        foreach (var sym in symbol)
            result = AddScriptingSymbols(result, sym);
        return result;
    }

    public static string AddScriptingSymbols(string original, string symbol)
    {
        var symbols = original.Split(';');
        string newsymbol = "";
        bool hasProd = false;
        for (int i = 0; i < symbols.Length; i++)
        {
            if (symbols[i] == symbol)
                hasProd = true;
            newsymbol += ";" + symbols[i];
        }

        if (hasProd == false)
            newsymbol += ";" + symbol;

        newsymbol.Remove(0, 1);
        return newsymbol;
    }

    public void BuildAndroid(string name, BuildOptions buildOptions)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = "Builds/" + name + "_" + appVersion + "_" + buildnumber + ".apk";
        buildPlayerOptions.target = BuildTarget.Android;

        //PlayerSettings.Android.useCustomKeystore = true;
        //PlayerSettings.Android.keystoreName = "D:/_workspace/devs/_ONR2/ProjectSettings/superrunner2.keystore";
        //PlayerSettings.Android.keystorePass = "ULrsxgrHoKtvgu7kSK46";
        //PlayerSettings.Android.keyaliasName = "superrunner2";
        //PlayerSettings.Android.keyaliasPass = "ULrsxgrHoKtvgu7kSK46";
        BuildPipeline.BuildPlayer(scenes.ToArray(), buildPlayerOptions.locationPathName, buildPlayerOptions.target, buildOptions);
        this.Close();
    }
}
