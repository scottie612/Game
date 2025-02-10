using Game.Common.Encryption;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;

    [SerializeField] private TMP_Text _loginResponse;
    [SerializeField] private Button _login;
    [SerializeField] private Button _register;
    [SerializeField] private Button _togglePasswordVisibility;

    [SerializeField] private Sprite _showPasswordIcon;
    [SerializeField] private Sprite _hidePasswordIcon;

    public void Start()
    {
        _loginResponse.text = "";
        _login.onClick.AddListener(OnLoginClicked);
        _register.onClick.AddListener(OnRegisterClicked);
        _togglePasswordVisibility.onClick.AddListener(OnTogglePasswordVisibilityClicked);

        _email.Select();
        _email.ActivateInputField();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnLoginClicked();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeFocus();
        }
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
        //Navigate to Register Screen
        SceneManager.LoadScene("Register");
    }

    private void OnTogglePasswordVisibilityClicked()
    {
        if(_password.inputType == TMP_InputField.InputType.Password)
        {
            _togglePasswordVisibility.image.sprite =_hidePasswordIcon;
            _password.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            _togglePasswordVisibility.image.sprite = _showPasswordIcon;
            _password.contentType = TMP_InputField.ContentType.Password;
        }
        _password.DeactivateInputField();
        _password.ActivateInputField();
    }

    private void ChangeFocus()
    {
        if (_email.isFocused) 
        {
            _password.Select();
            _password.ActivateInputField();
        }else if (_password.isFocused)
        {
            _email.Select();
            _email.ActivateInputField();
        }
    }
}

