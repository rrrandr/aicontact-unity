using System;
using TMPro;
using UnityEngine;

public class FreeTrialScreenHandler : MonoBehaviour
{
    public bool freeTrialOver;
    public TextMeshProUGUI daysRemainingText;
    [TextArea] public string subscriptionURL;

    private void Start()
    {
        if (string.IsNullOrEmpty(PreferenceManager.TrialStartingDate))
        {
            PreferenceManager.TrialStartingDate = DateTime.Now.ToString();
            daysRemainingText.text = "13";
            daysRemainingText.color = Colors.Instance.darkYellow;
        }
        else
        {
            TimeSpan timeSpan = DateTime.Now - DateTime.Parse(PreferenceManager.TrialStartingDate);

            int remainingDays = 13 - timeSpan.Days;

            if (remainingDays < 0)
            {
                remainingDays = 0;
            }

            daysRemainingText.text = remainingDays.ToString();

            if (remainingDays < 10)
            {
                daysRemainingText.text = "0" + remainingDays.ToString();
            }

            if (remainingDays < 4)
            {
                daysRemainingText.color = Colors.Instance.grey;
            }
            else
            {
                daysRemainingText.color = Colors.Instance.yellow;
            }

            if (remainingDays == 0)
            {
                freeTrialOver = true;
                //UIManager.Instance.DisplaySpecificScreen(GameScreens.SubscriptionScreen);
            }
            else
            {
                freeTrialOver = false;
                //UIManager.Instance.DisplaySpecificScreen(GameScreens.MainScreen);
            }

            if (!string.IsNullOrEmpty(PreferenceManager.SubscriptionId))
            {
                StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.PayPalSubscriptionScreen].GetComponent<PayPalSubscriptionScreenHandler>().CallPayPalSubscriptionAPI(PreferenceManager.SubscriptionId));
            }
            else
            {
                UIManager.Instance.DisplaySpecificScreen(ScreenName.PayPalSubscriptionScreen);
            }
        }
    }

    public void OnYesPleaseButtonClick()
    {
        Application.OpenURL(subscriptionURL);
        UIManager.Instance.DisplaySpecificScreen(ScreenName.PayPalSubscriptionScreen);
    }
}
