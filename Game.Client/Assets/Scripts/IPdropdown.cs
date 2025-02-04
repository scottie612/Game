using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class IPdropdown : MonoBehaviour
{
    public TMP_Dropdown _dropdown;

    private List<string> DropOptions = new List<string> { "localhost", "Dev-local", "Dev" };

    void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();

        _dropdown.ClearOptions();
        _dropdown.AddOptions(DropOptions);
    }
    private void Start()
    {
        _dropdown.onValueChanged.AddListener(delegate {
            switch (DropOptions[_dropdown.value]) 
            {
                case "localhost":
                    Globals.ServerIP = "127.0.0.1";
                    break;
                case "Dev-local":
                    Globals.ServerIP = "192.168.1.100";
                    break;
                case "Dev":
                    Globals.ServerIP = "173.168.80.191";
                    break;
                default:
                    break;
            };
        });
    }


}
