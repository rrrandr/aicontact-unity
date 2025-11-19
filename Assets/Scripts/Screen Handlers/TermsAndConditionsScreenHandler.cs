using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TermsAndConditionsScreenHandler : MonoBehaviour
{
    public GameObject endUserLicenseAgreementPanel;
    public GameObject termsOfServicesPanel;
    public GameObject privacyPolicyPanel;

    public Scrollbar endUserLicenseAgreementScrollbar;
    public Scrollbar termsOfServicesScrollbar;
    public Scrollbar privacyPolicyScrollbar;

    private bool processActive = false;

    private void Start()
    {
        endUserLicenseAgreementPanel.SetActive(true);
        termsOfServicesPanel.SetActive(false);
        privacyPolicyPanel.SetActive(false);
        StartCoroutine(ScrollbarCoroutine(endUserLicenseAgreementScrollbar, 0.1f));
    }

    private IEnumerator ScrollbarCoroutine(Scrollbar scrollbar, float time)
    {
        yield return new WaitForSeconds(time);
        scrollbar.value = 1;
    }

    public void OnEndUserLicenseAgreementToggleValueChange()
    {
        if (!processActive)
        {
            processActive = true;
            Invoke(nameof(DisplayTermsOfServicesPanel), 0.5f);
        }
    }

    private void DisplayTermsOfServicesPanel()
    {
        processActive = false;
        endUserLicenseAgreementPanel.SetActive(false);
        termsOfServicesPanel.SetActive(true);
        StartCoroutine(ScrollbarCoroutine(termsOfServicesScrollbar, 0.4f));
    }

    public void OnTermsOfServicesToggleValueChange()
    {
        if (!processActive)
        {
            processActive = true;
            Invoke(nameof(DisplayPrivacyPolicyPanel), 0.5f);
        }
    }

    private void DisplayPrivacyPolicyPanel()
    {
        processActive = false;
        termsOfServicesPanel.SetActive(false);
        privacyPolicyPanel.SetActive(true);
        StartCoroutine(ScrollbarCoroutine(privacyPolicyScrollbar, 0.1f));
    }

    public void OnPrivacyPolicyToggleValueChange()
    {
        if (!processActive)
        {
            processActive = true;
            Invoke(nameof(DisplayMainScreen), 0.5f);
        }
    }

    private void DisplayMainScreen()
    {
        processActive = false;

        if (AICONTACTManager.Instance.appVersion == ApplicationVersion.PayPal)
        {
            PreferenceManager.TermsAndConditionsStatus = "Accepted";
            UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
        }
        else
        {
            if (!string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
            {
                DatabaseManager.Instance.UpdateUser(new UserData() { terms_accepted = "true" }, OnUpdateTermsAcceptedDataComplete);
            }
            else
            {
                UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
            }
        }
    }

    private void OnUpdateTermsAcceptedDataComplete(UnityWebRequest unityWebRequest, UserData userData)
    {
        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            UIManager.Instance.DisplaySpecificScreen(ScreenName.MainScreen);
        }
        else
        {
            DatabaseManager.Instance.UpdateUser(new UserData() { subscription_date = true.ToString() }, OnUpdateTermsAcceptedDataComplete);
        }
    }
}
