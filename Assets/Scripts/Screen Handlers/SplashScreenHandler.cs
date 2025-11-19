using UnityEngine;

public class SplashScreenHandler : MonoBehaviour
{
    public bool clearCacheOnStart;

    private void Start()
    {
        if (clearCacheOnStart)
            PlayerPrefs.DeleteAll();

        Invoke(nameof(DisplayNextScreen), 2.5f);
    }

    private void DisplayNextScreen()
    {
        if (AICONTACTManager.Instance.appVersion == ApplicationVersion.PayPal)
        {
            if (!string.IsNullOrEmpty(PreferenceManager.SubscriptionId))
            {
                StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.PayPalSubscriptionScreen].GetComponent<PayPalSubscriptionScreenHandler>().CallPayPalSubscriptionAPI(PreferenceManager.SubscriptionId));
            }
            else
            {
                UIManager.Instance.DisplaySpecificScreen(ScreenName.FreeTrialScreen);
            }
        }
        else
        {
            UIManager.Instance.DisplaySpecificScreen(ScreenName.InAppPurchaseSubscriptionScreen);
        }
    }
}
