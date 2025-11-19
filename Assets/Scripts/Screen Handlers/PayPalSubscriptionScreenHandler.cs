using System.IO;
using System.Net;
using System.Text;
using System;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PayPalSubscriptionScreenHandler : MonoBehaviour
{
    [SerializeField][TextArea] private string clientId;
    [SerializeField][TextArea] private string clientSecretKey;
    public GameObject subsciptionVerificationPanel;
    public GameObject errorPanel;
    public TMP_InputField subsciptionVerificationPanelInputField;
    public TMP_InputField errorPanelInputField;
    public TextMeshProUGUI errorMessageText;

    private bool apiCalled = false;
    [SerializeField][TextArea] private List<string> errorMessages;
    [SerializeField] private APIResponse apiResponse;

    private void Start()
    {
        subsciptionVerificationPanel.SetActive(true);
        errorPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !apiCalled)
        {
            ValidateSubscription();
        }
    }

    public void ValidateSubscription()
    {
        TMP_InputField inputField;
        if (subsciptionVerificationPanel.activeInHierarchy)
        {
            inputField = subsciptionVerificationPanelInputField;
        }
        else
        {
            inputField = errorPanelInputField;
        }

        if (!string.IsNullOrEmpty(inputField.text))
        {
            if (inputField.text.Length == 14 && inputField.text.Contains("-"))
            {
                StartCoroutine(CallPayPalSubscriptionAPI(inputField.text));
            }
            else
            {
                DisplayValidationErrorPanel(0);
            }
        }
    }

    private void DisplayValidationErrorPanel(int errorNumber)
    {
        if (!gameObject.activeInHierarchy)
        {
            UIManager.Instance.DisplaySpecificScreen(ScreenName.PayPalSubscriptionScreen);
        }

        errorMessageText.text = errorMessages[errorNumber];
        subsciptionVerificationPanel.SetActive(false);
        errorPanel.SetActive(false);
        errorPanel.SetActive(true);
    }

    public IEnumerator CallPayPalSubscriptionAPI(string subscriptionId)
    {
        apiCalled = true;
        string url = "https://api.paypal.com/v1/billing/subscriptions/" + subscriptionId;

        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Method = "GET";

        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecretKey}"));
        httpWebRequest.Headers["Authorization"] = "Basic " + credentials;

        //httpWebRequest.Headers["X-PAYPAL-SECURITY-CONTEXT"] = "{\"consumer\":{\"accountNumber\":1181198218909172527,\"merchantId\":\"5KW8F2FXKX5HA\"},\"merchant\":{\"accountNumber\":1659371090107732880,\"merchantId\":\"2J6QB8YJQSJRJ\"},\"apiCaller\":{\"clientId\":\"AdtlNBDhgmQWi2xk6edqJVKklPFyDWxtyKuXuyVT-OgdnnKpAVsbKHgvqHHP\",\"appId\":\"APP-6DV794347V142302B\",\"payerId\":\"2J6QB8YJQSJRJ\",\"accountNumber\":\"1659371090107732880\"},\"scopes\":[\"https://api-m.paypal.com/v1/subscription/.*\",\"https://uri.paypal.com/services/subscription\",\"openid\"]}";
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Accept = "application/json";

        try
        {
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string result = reader.ReadToEnd();
                            Debug.Log(result);
                            apiResponse = JsonUtility.FromJson<APIResponse>(result);

                            if (apiResponse.status == "CANCELLED")
                            {
                                DisplayValidationErrorPanel(1);
                            }
                            else if (!SubscriptionTimeOver(apiResponse.billing_info.next_billing_time))
                            {
                                DisplayValidationErrorPanel(2);
                            }
                            else
                            {
                                PreferenceManager.SubscriptionId = subscriptionId;

                                if (PreferenceManager.TermsAndConditionsStatus == "Not Accepted")
                                {
                                    UIManager.Instance.DisplaySpecificScreen(ScreenName.TermsAndConditionsScreen);
                                }
                                else
                                {
                                    UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (WebException ex)
        {
            if (ex.Response != null)
            {
                using (Stream errorStream = ex.Response.GetResponseStream())
                {
                    if (errorStream != null)
                    {
                        using (StreamReader reader = new StreamReader(errorStream))
                        {
                            string errorResult = reader.ReadToEnd();
                            Debug.LogError("Error: " + errorResult);
                            DisplayValidationErrorPanel(0);
                        }
                    }
                }
            }
        }

        yield return null;
        apiCalled = false;
    }

    private bool SubscriptionTimeOver(string subscriptionDueDataAndTime)
    {
        DateTime time1 = DateTime.ParseExact(subscriptionDueDataAndTime, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
        DateTime time2 = DateTime.Now;

        TimeSpan timeDifference = time1 - time2;
        Debug.Log($"Time Difference: {timeDifference.Days} days, {timeDifference.Hours} hours, {timeDifference.Minutes} minutes, {timeDifference.Seconds} seconds");

        if (timeDifference.Days >= 0 && timeDifference.Hours >= 0 && timeDifference.Minutes >= 0 && timeDifference.Seconds >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnPayPalAccountButtonClick()
    {
        Application.OpenURL("https://www.paypal.com/");
    }
    
    public void OnAICONTACTButtonClick()
    {
        Application.OpenURL("https://facestream.ai/aicontact");
    }
}

[Serializable]
public class APIResponse
{
    public string status;
    public BillingInfo billing_info;
}

[Serializable]
public class BillingInfo
{
    public string next_billing_time;
}