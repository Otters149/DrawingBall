using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    public int Hints
    {
        get { return _hints; }
        private set { _hints = value; }
    }

    public static event Action<int> CoinsUpdated = delegate { };

    [SerializeField]
    int initialHints = 0;

    // Show the current coins value in editor for easy testing
    [SerializeField]
    int _hints;

    // key name to store high score in PlayerPrefs
    const string PPK_HINTS = "SGLIB_HINTS";
    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        // Initialize coins
        Hints = JFLocalSave.Instance.GetInt(PPK_HINTS, initialHints);
    }

    public void AddHints(int amount)
    {
        Hints += amount;


        // Store new coin value
        JFLocalSave.Instance.SetInt(PPK_HINTS, Hints);

        // Fire event
        CoinsUpdated(Hints);
    }

    public void RemoveHints(int amount)
    {
        Hints -= amount;

        // Store new coin value
        JFLocalSave.Instance.SetInt(PPK_HINTS, Hints);

        // Fire event
        CoinsUpdated(Hints);
    }

}
