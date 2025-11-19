using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public ApplicationScreen[] UIScreens;
    public Dictionary<ScreenName, GameObject> UIScreensReferences = new Dictionary<ScreenName, GameObject>();

    public static UIManager _instance;
    public static UIManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        SetupScreens();
    }

    private void SetupScreens()
    {
        foreach (ApplicationScreen screen in UIScreens)
        {
            UIScreensReferences.Add(screen.screenName, screen.screen);
        }
        DisplaySpecificScreen(ScreenName.SplashScreen);
    }


    public void DisplayScreen(ScreenName screenName)
    {
        UIScreensReferences[screenName].gameObject.SetActive(true);
    }

    public void DisplaySpecificScreen(ScreenName screenName)
    {
        foreach (var item in UIScreens)
        {
            UIScreensReferences[item.screenName].gameObject.SetActive(false);
        }
        UIScreensReferences[screenName].gameObject.SetActive(true);
    }

    public void HideScreen(ScreenName screenName)
    {
        UIScreensReferences[screenName].gameObject.SetActive(false);
    }
}

[System.Serializable]
public class ApplicationScreen
{
    public ScreenName screenName;
    public GameObject screen;
}

public enum ScreenName
{
    SplashScreen,
    FreeTrialScreen,
    PayPalSubscriptionScreen,
    InAppPurchaseSubscriptionScreen,
    TermsAndConditionsScreen,
    MainScreen
}

