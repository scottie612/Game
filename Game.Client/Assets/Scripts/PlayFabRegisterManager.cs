using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFabRegisterManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmPassword;

    [SerializeField] private TMP_Text _registerResponse;
    [SerializeField] private Button _register;
    [SerializeField] private Button _back;

    public void Start()
    {
        //Remove later
        PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();

        _registerResponse.text = "";
        _register.onClick.AddListener(OnRegisterClicked);
        _back.onClick.AddListener(OnBackClicked);
    }

    private void OnRegisterClicked()
    {
        Debug.Log("Register Clicked");
        _registerResponse.text = "";


        //Implement Input validation
        //Ensure Passwords match
        if (_password.text != _confirmPassword.text)
        {
            _registerResponse.text = "Passwords Must match";
            _registerResponse.color = Color.red;
            return;
        }

        //Send Register Request to playfab
        var request = new RegisterPlayFabUserRequest
        {
            Email = _email.text,
            DisplayName = _username.text,
            Username = _username.text,
            Password = _password.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            successResult =>
            {
                Debug.Log("Successfully Registered User");
                _registerResponse.text = "Account Created!";
                _registerResponse.color = Color.green;
            },
            failureResult =>
            {
                Debug.Log("Failed to Registered User");
                Debug.LogError(failureResult.GenerateErrorReport());
                _registerResponse.text = failureResult.ErrorMessage;
                _registerResponse.color = Color.red;
            }
        );

    }

    private void OnBackClicked()
    {
        Debug.Log("Back Clicked");
        SceneManager.LoadScene("Login");
    }
}
