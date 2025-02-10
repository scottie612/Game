using Game.Common.Encryption;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFabLoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;

    [SerializeField] private TMP_Text _loginResponse;
    [SerializeField] private Button _login;
    [SerializeField] private Button _register;

    public void Start()
    {
        _loginResponse.text = "";
        _login.onClick.AddListener(OnLoginClicked);
        _register.onClick.AddListener(OnRegisterClicked);
    }

    private void OnLoginClicked()
    {
        _loginResponse.text = "";

        var request = new LoginWithEmailAddressRequest
        {
            Email = _email.text,
            Password = _password.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetPlayerProfile = true,
                GetUserAccountInfo = true,
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            successResult =>
            {
                Debug.Log("Successfully Logged In");
                _loginResponse.text = "Logged In!";
                _loginResponse.color = Color.green;

                //Save Variables
                Globals.SessionTicket = successResult.SessionTicket;
                Globals.PlayFabUserID = successResult.PlayFabId;
                Globals.PlayFabUsername = successResult.InfoResultPayload.AccountInfo.Username;
                Globals.RSAKeypair = EncryptionHelper.GenerateKeyPair();

                //Navigate to Game
                SceneManager.LoadScene("Play");
            },
            failureResult =>
            {
                Debug.Log("Failed to Log In");
                Debug.LogError(failureResult.GenerateErrorReport());
                _loginResponse.text = failureResult.ErrorMessage;
                _loginResponse.color = Color.red;
            }
        );
    }

    private void OnRegisterClicked()
    {
        //Navigate to login Screen
        SceneManager.LoadScene("Register");

    }
}

