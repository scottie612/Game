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
        //Remove later
        PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();

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
                GetPlayerProfile = true
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
                var privateKey = EncryptionHelper.GetPrivateKey();
                var publicKey = EncryptionHelper.GetPublicKey();

                UpdateUserDataRequest updateUserDataRequest = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {"PublicKey", publicKey}
                    },
                };
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {"PublicKey", publicKey}
                    },
                },
                result =>
                {
                    Debug.Log("Successfully Stored Public Key in UserData");
                },
                error =>
                {
                    Debug.LogError("Failed to Store Public Key in UserData");
                });

                //Navigate to Game
                SceneManager.LoadScene("World");
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

