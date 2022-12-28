using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class A3EBuildConfigure
{
    //@a3e_help: define difference symbol defending on project
    public static string[] listDebugSymbol = new string[] { "_DEBUG", "_CHEAT_ENABLE" };
    public static string[] listProdSymbol = new string[] { "_PRODUCTION" };

    [MenuItem("A3E/Build_Configure/Setup_Development")]
    public static void AddDebugScripting()
    {
        var target = BuildTargetGroup.Android;
#if UNITY_IOS
		target = BuildTargetGroup.iOS;
#endif
        var originalSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, RemoveScriptingSymbols(originalSymbol, listProdSymbol));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, AddScriptingSymbols(originalSymbol, listDebugSymbol));
    }

    [MenuItem("A3E/Build_Configure/Setup_Production")]
    public static void RemoveDebugScripting()
    {
        var target = BuildTargetGroup.Android;
#if UNITY_IOS
		target = BuildTargetGroup.iOS;
#endif
        var originalSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, RemoveScriptingSymbols(originalSymbol, listDebugSymbol));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, AddScriptingSymbols(originalSymbol, listProdSymbol));
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
}
