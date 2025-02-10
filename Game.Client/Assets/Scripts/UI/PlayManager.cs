using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Dropdown _ipDropdown;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _logoutButton;

    private List<string> DropOptions = new List<string> { "localhost", "Dev-local", "Dev" };

    void Start()
    {
        _usernameText.text = Globals.PlayFabUsername;
        _ipDropdown.ClearOptions();
        _ipDropdown.AddOptions(DropOptions);
        _ipDropdown.onValueChanged.AddListener(OnIPDropDownChanged);

        _playButton.onClick.AddListener(OnPlayClicked);
        _logoutButton.onClick.AddListener(OnLogoutClicked);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnPlayClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnLogoutClicked();
        }
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("World");
    }

    private void OnLogoutClicked()
    {
        SceneManager.LoadScene("Login");
    }

    private void OnIPDropDownChanged(int index)
    {
        switch (DropOptions[index])
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
    }
}
