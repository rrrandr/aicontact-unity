using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordInputField : MonoBehaviour
{
    private TMP_InputField passwordInputField;
    public Image icon;
    public Sprite displayedPasswordSprite;
    public Sprite hiddenPasswordSprite;

    private void OnEnable()
    {
        passwordInputField = GetComponent<TMP_InputField>();

        if (passwordInputField.contentType == TMP_InputField.ContentType.Standard)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            icon.sprite = hiddenPasswordSprite;
        }
    }

    public void Toggle()
    {
        if (passwordInputField.contentType == TMP_InputField.ContentType.Standard)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            icon.sprite = hiddenPasswordSprite;    
        }
        else if (passwordInputField.contentType == TMP_InputField.ContentType.Password)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            icon.sprite = displayedPasswordSprite;
        }

        passwordInputField.ActivateInputField();
    }
}
