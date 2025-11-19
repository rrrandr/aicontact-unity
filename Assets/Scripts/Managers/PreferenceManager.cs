using Newtonsoft.Json;
using UnityEngine;

public static class PreferenceManager
{
    public static string Credentials
    {
        get { return PlayerPrefs.GetString("CREDENTIALS", JsonConvert.SerializeObject(new LoginData())); }

        set { PlayerPrefs.SetString("CREDENTIALS", value); }
    }

    public static string TrialStartingDate
    {
        get { return PlayerPrefs.GetString("TRIAL_STARTING_DATE", null); }

        set { PlayerPrefs.SetString("TRIAL_STARTING_DATE", value); }
    }

    public static string SubscriptionId
    {
        get { return PlayerPrefs.GetString("SUBSCRIPTION_ID", null); }

        set { PlayerPrefs.SetString("SUBSCRIPTION_ID", value); }
    }

    public static string TermsAndConditionsStatus
    {
        get { return PlayerPrefs.GetString("TERMS_AND_CONDITIONS_STATUS", "Not Accepted"); }

        set { PlayerPrefs.SetString("TERMS_AND_CONDITIONS_STATUS", value); }
    }

    public static int LensNumber
    {
        get { return PlayerPrefs.GetInt("LENS_NUMBER", 1); }

        set { PlayerPrefs.SetInt("LENS_NUMBER", value); }
    }

    public static float IrisColorBrightness
    {
        get { return PlayerPrefs.GetFloat("IRIS_COLOR_BRIGHTNESS", 0.5f); }

        set { PlayerPrefs.SetFloat("IRIS_COLOR_BRIGHTNESS", value); }
    }

    public static float ScleraColorBrightness
    {
        get { return PlayerPrefs.GetFloat("SCLERA_COLOR_BRIGHTNESS", 1f); }

        set { PlayerPrefs.SetFloat("SCLERA_COLOR_BRIGHTNESS", value); }
    }
    
    public static float PupilColorBrightness
    {
        get { return PlayerPrefs.GetFloat("PUPIL_COLOR_BRIGHTNESS", 0.5f); }

        set { PlayerPrefs.SetFloat("PUPIL_COLOR_BRIGHTNESS", value); }
    }
}
