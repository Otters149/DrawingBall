using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Text>().text = "v" + Application.version;
    }
}
