using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmPassword;

    [SerializeField] private TMP_Text _registerResponse;
    [SerializeField] private Button _register;
    [SerializeField] private Button _back;
    [SerializeField] private Button _togglePasswordVisibility;
    [SerializeField] private Button _toggleConfirmPasswordVisibility;

    [SerializeField] private Sprite _showPasswordIcon;
    [SerializeField] private Sprite _hidePasswordIcon;

    public void Start()
    {
        _registerResponse.text = "";
        _register.onClick.AddListener(OnRegisterClicked);
        _back.onClick.AddListener(OnBackClicked);
        _togglePasswordVisibility.onClick.AddListener(OnTogglePasswordVisibilityClicked);
        _toggleConfirmPasswordVisibility.onClick.AddListener(OnToggleConfirmPasswordVisibilityClicked);

        _email.Select();
        _email.ActivateInputField();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnRegisterClicked();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeFocus();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
        }
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
        SceneManager.LoadScene("Login");
    }

    private void OnTogglePasswordVisibilityClicked()
    {
        if (_password.inputType == TMP_InputField.InputType.Password)
        {
            _togglePasswordVisibility.image.sprite = _hidePasswordIcon;
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

    private void OnToggleConfirmPasswordVisibilityClicked()
    {
        if (_confirmPassword.inputType == TMP_InputField.InputType.Password)
        {
            _toggleConfirmPasswordVisibility.image.sprite = _hidePasswordIcon;
            _confirmPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            _toggleConfirmPasswordVisibility.image.sprite = _showPasswordIcon;
            _confirmPassword.contentType = TMP_InputField.ContentType.Password;
        }
        _confirmPassword.DeactivateInputField();
        _confirmPassword.ActivateInputField();
    }

    private void ChangeFocus()
    {
        if (_username.isFocused)
        {
            _email.Select();
            _email.ActivateInputField();
        }
        else if (_email.isFocused)
        {
            _password.Select();
            _password.ActivateInputField();
        }
        else if (_password.isFocused)
        {
            _confirmPassword.Select();
            _confirmPassword.ActivateInputField();
        }
        else if (_confirmPassword.isFocused)
        {
            _username.Select();
            _username.ActivateInputField();
        }
    }
}