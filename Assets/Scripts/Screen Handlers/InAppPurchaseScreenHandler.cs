using Newtonsoft.Json;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class InAppPurchaseScreenHandler : MonoBehaviour
{
    [Header("Continue")]
    public GameObject continuePanel;
    public Loader autoSigninLoader;

    [Header("Sign In")]
    public GameObject signInPanel;
    private bool isWaiting;
    public TMP_InputField emailInputFieldSignIn;
    public TMP_InputField passwordInputFieldSignIn;
    public Toggle rememberMeToggle;
    public Loader signinLoader;

    [Header("Sign Up")]
    public GameObject signUpPanel;
    public TMP_InputField emailInputFieldSignUp;
    public TMP_InputField passwordInputFieldSignUp;
    public TMP_InputField confirmPasswordInputFieldSignUp;
    public Loader signupLoader;

    [Header("Buy Subscription")]
    public GameObject buySubsciptionPanel;

    [Header("Message")]
    public GameObject message;
    public TextMeshProUGUI messageText;

    private void OnEnable()
    {
        continuePanel.SetActive(true);
        signInPanel.SetActive(false);
        signUpPanel.SetActive(false);
        buySubsciptionPanel.SetActive(false);
        message.SetActive(false);
    }

    void Start()
    {
         if (!string.IsNullOrEmpty(JsonConvert.DeserializeObject<LoginData>(PreferenceManager.Credentials).email))
         {
            OnOpenSignInPanelButtonClick();
         }
    }

    #region Continue

    public void OnContinueButtonClick()
    {
        if (PreferenceManager.TermsAndConditionsStatus == "Not Accepted")
        {
            UIManager.Instance.DisplaySpecificScreen(ScreenName.TermsAndConditionsScreen);
        }
        else
        {
            UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
        }
    }

    public void OnBackButtonClick()
    {
        continuePanel.SetActive(true);
        signInPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }

    #endregion



    #region Sign In

    public void OnSignInButtonClick()
    {
        if (string.IsNullOrEmpty(emailInputFieldSignIn.text))
        {
            DisplayMessage("Enter your email address");
        }
        else if (string.IsNullOrEmpty(passwordInputFieldSignIn.text))
        {
            DisplayMessage("Enter password");
        }
        else if (!isWaiting)
        {
            isWaiting = true;
            signinLoader.ToggleLoader(true);
            DatabaseManager.Instance.SignInUser(new LoginData() {email = emailInputFieldSignIn.text , password = passwordInputFieldSignIn.text } , OnSignInComplete);
        }
    }

    private void OnSignInComplete(UnityWebRequest unityWebRequest, LoginData loginData)
    {
        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            if (rememberMeToggle.isOn)
            {
                PreferenceManager.Credentials = JsonConvert.SerializeObject(new LoginData() { email = emailInputFieldSignIn.text, password = passwordInputFieldSignIn.text });
            }

            DatabaseManager.Instance.GetUser(loginData.email, OnGetUserComplete);
        }
        else
        {
            isWaiting = false;
            signinLoader.ToggleLoader(false);
            autoSigninLoader.ToggleLoader(false);
            DisplayMessage("Sign-in failed. Please try again later.");
        }
    }

    private void OnGetUserComplete(UnityWebRequest unityWebRequest, UserData user)
    {
        isWaiting = false;
        signinLoader.ToggleLoader(false);
        autoSigninLoader.ToggleLoader(false);
        continuePanel.SetActive(false);

        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            signInPanel.SetActive(false);
            buySubsciptionPanel.SetActive(true);

            //Automatically check user
            if (string.IsNullOrEmpty(user.subscription_date))
            {
                signInPanel.SetActive(false);
                buySubsciptionPanel.SetActive(true);
            }
            else
            {
                if (VerifySubscriptionExpiry(user.subscription_date))
                {
                    if (user.terms_accepted == "false")
                    {
                        UIManager.Instance.DisplaySpecificScreen(ScreenName.TermsAndConditionsScreen);
                    }
                    else
                    {
                        UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
                    }
                }
                else
                {
                    signInPanel.SetActive(false);
                    buySubsciptionPanel.SetActive(true);
                    DisplayMessage("Your subscription has expired");
                }
            }
        }
        else
        {
            DatabaseManager.Instance.GetUser(user.email, OnGetUserComplete);
        }
    }

    public bool VerifySubscriptionExpiry(string subscriptionBuyingDate)
    {
        TimeSpan timeSpan = DateTime.Now - DateTime.Parse(subscriptionBuyingDate);
        int remainingDays = 30 - timeSpan.Days;
        if (remainingDays <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void OnOpenSignUpPanelButtonClick()
    {
        emailInputFieldSignUp.text = "";
        passwordInputFieldSignUp.text = "";
        confirmPasswordInputFieldSignUp.text = "";

        signInPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    #endregion



    #region Sign Up

    public void OnSignUpButtonClick()
    {
        if (string.IsNullOrEmpty(emailInputFieldSignUp.text))
        {
            DisplayMessage("Enter email address");
        }
        else if (string.IsNullOrEmpty(passwordInputFieldSignUp.text))
        {
            DisplayMessage("Enter password");
        }
        else if (string.IsNullOrEmpty(confirmPasswordInputFieldSignUp.text))
        {
            DisplayMessage("Enter confirm password");
        }
        else if (passwordInputFieldSignUp.text.Length < 8)
        {
            DisplayMessage("Enter a password having at least 8 characters");
        }
        else if (passwordInputFieldSignUp.text != confirmPasswordInputFieldSignUp.text)
        {
            DisplayMessage("Your password doesn't match");
        }
        else if (!isWaiting)
        {
            isWaiting = true;
            signupLoader.ToggleLoader(true);
            DatabaseManager.Instance.SignUpUser(new UserData { email = emailInputFieldSignUp.text, password = passwordInputFieldSignUp.text, terms_accepted = "false" }, OnSignUpComplete);
        }
    }

    private void OnSignUpComplete(UnityWebRequest unityWebRequest)
    {
        isWaiting = false;
        signupLoader.ToggleLoader(false);

        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            DisplayMessage("Sign-up successful! Welcome aboard!");
            signUpPanel.SetActive(false);
            buySubsciptionPanel.SetActive(true);
        }
        else
        {
            DisplayMessage("Sign-up failed. Please try again or check your details and ensure all fields are correct.");
        }
    }

    public void OnOpenSignInPanelButtonClick()
    {
        if (!string.IsNullOrEmpty(JsonConvert.DeserializeObject<LoginData>(PreferenceManager.Credentials).email))
        {
            isWaiting = true;
            autoSigninLoader.ToggleLoader(true);
            rememberMeToggle.isOn = false;
            DatabaseManager.Instance.SignInUser(JsonConvert.DeserializeObject<LoginData>(PreferenceManager.Credentials), OnSignInComplete);
        }
        else
        {
            emailInputFieldSignIn.text = "";
            passwordInputFieldSignIn.text = "";
            rememberMeToggle.isOn = false;

            continuePanel.SetActive(false);
            signInPanel.SetActive(true);
            signUpPanel.SetActive(false);
        }
    }

    #endregion



    #region Subscription

    public void OnRestoreButtonClick()
    {

        if (string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.subscription_date))
        {
            DisplayMessage("You haven't subscribed this package");
        }
        else
        {
            if (VerifySubscriptionExpiry(DatabaseManager.Instance.currentUser.subscription_date))
            {
                if (DatabaseManager.Instance.currentUser.terms_accepted == "false")
                {
                    UIManager.Instance.DisplaySpecificScreen(ScreenName.TermsAndConditionsScreen);
                }
                else
                {
                    UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
                }
            }
            else
            {
                DisplayMessage("Your subscription has expired");
            }
        }
    }

    public void OnPurchaseComplete(Product product)
    {
        Debug.Log("Purchase Complete: " + product.definition.id);
        DatabaseManager.Instance.UpdateUser(new UserData() {subscription_date = DateTime.Now.ToString() }, OnUpdateSubscriptionDataComplete);
    }

    private void OnUpdateSubscriptionDataComplete(UnityWebRequest unityWebRequest, UserData userData)
    {
        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            if (DatabaseManager.Instance.currentUser.terms_accepted == "false")
            {
                UIManager.Instance.DisplaySpecificScreen(ScreenName.TermsAndConditionsScreen);
            }
            else
            {
                UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
            }
        }
        else
        {
            DatabaseManager.Instance.UpdateUser(new UserData() { subscription_date = DateTime.Now.ToString() }, OnUpdateSubscriptionDataComplete);
        }
    }

    public void OnPurchaseFailed()
    {
        Debug.Log("Purchase Failed");
    }

    public void OnProductFetched(Product product)
    {
        var subscriptionManager = new SubscriptionManager(product, null);

        var info = subscriptionManager.getSubscriptionInfo();
        Debug.Log(info.getExpireDate());
    }

    public void OnAICONTACTButtonClick()
    {
        Application.OpenURL("https://facestream.ai/aicontact");
    }

    public void OnEULAButtonClick()
    {
        Application.OpenURL("https://facestreamai.com/legal");
    }

    #endregion



    #region Message

    public void DisplayMessage(string messageText)
    {
        StopAllCoroutines();
        message.SetActive(true);
        this.messageText.text = null;
        StartCoroutine(SetMessageSize(messageText));
        StartCoroutine(HideMessage());
    }

    private IEnumerator SetMessageSize(string messageText)
    {
        yield return new WaitForSeconds(0.01f);
        this.messageText.text = messageText;
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(5f);
        message.SetActive(false);
    }

    #endregion
}