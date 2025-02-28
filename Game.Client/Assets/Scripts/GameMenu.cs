using UnityEngine;
using UnityEngine.UI;

public class GameMenu : Menu
{
    [SerializeField] private Button _disconnectButton;

    private void Start()
    {
        _disconnectButton.onClick.AddListener(OnDisconnectClicked);
        gameObject.SetActive(false);
    }

    private void OnDisconnectClicked()
    {
        NetworkManager.Instance.Disconnect();
    }
}
