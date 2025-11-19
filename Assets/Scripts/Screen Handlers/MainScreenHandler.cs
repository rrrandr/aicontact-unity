using Mediapipe.Unity;
using Mediapipe.Unity.IrisTracking;
using Newtonsoft.Json;
using SFB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenHandler : MonoBehaviour
{
    [Header("Settings")]
    public Sprite dropDownSprite;
    public Sprite dropUpSprite;
    public Button dropDownButton;
    private bool toggle = true;
    public List<GameObject> faceInCenter;
    public List<GameObject> faceNotInCentered;
    public int distanceBuffer;
    public Color irisColor;
    public Color scleraColor;
    public Color pupilColor;
    public bool autoBrightness;
    public GameObject slidersPanel;
    public BrightnessSlider irisBrightnessSlider;
    public Image irisColorImage;
    public BrightnessSlider scleraBrightnessSlider;
    public Image scleraColorImage;
    public BrightnessSlider pupilBrightnessSlider;
    public Image pupilColorImage;
    public List<Sprite> sliderSprites;
    public List<GameObject> irisButtonRings;

    [Header("Messages")]
    public GameObject faceCenterMessage;
    public GameObject signInMessage;

    [Header("Countdown")]
    public GameObject countdown;
    public TextMeshProUGUI countdownText;
    private float countdownTime = 5 * 60;

    [Header("Glint")]
    public GameObject glintImage;
    public GameObject glintVisibleImage;
    public GameObject glintHiddenImage;
    private bool displayGlint = true;

    [Header("Camera Eye Contact")]
    public GameObject cameraEyeContactButton;

    [Header("Modes")]
    public Toggle webCamModeToggle;
    public Toggle videoModeToggle;

    [Header("Logout")]
    public GameObject logoutButton;

    [Header("Select Camera")]
    [SerializeField]
    ScrollRect cameraScrollRect;

    private void OnEnable()
    {
        AICONTACTManager.Instance.Setup();
    }

    private void Start()
    {
        signInMessage.SetActive(false);
        StartCoroutine(AICONTACTManager.Instance.SpawnLens());
        webCamModeToggle.isOn = true;
        videoModeToggle.isOn = false;
        webCamModeToggle.onValueChanged.AddListener(OnWebCamModeToggleValueChange);
        videoModeToggle.onValueChanged.AddListener(OnVideoModeToggleValueChange);

        Button cameraButton = cameraScrollRect.content.GetComponentInChildren<Button>();
        int count = WebCamTexture.devices.Length;
        for(int i = 0; i < count; i++)
        {
            int index = i;
            Button newButton = Instantiate(cameraButton, cameraScrollRect.content);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = WebCamTexture.devices[i].name;
            newButton.onClick.AddListener(() => OnCameraButtonClick(index));
        }
        cameraButton.gameObject.SetActive(false);
    }
    private void OnCameraButtonClick(int index)
    {
        Debug.Log("Camera Button Clicked: " + index);
        AICONTACTManager.Instance.OnCameraChanged(index);
    }

    private void OnWebCamModeToggleValueChange(bool isOn)
    {
        if(isOn)
        {
            Mode("WebCamera");
        }
    }

    private void OnVideoModeToggleValueChange(bool isOn)
    {
        if(isOn)
        {
            Mode("Video");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToggleBrightnessAdjuster();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleSettingsPanel();
        }

        if (AICONTACTManager.Instance.faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList.Count == 468 && toggle)
        {
            float distance = Vector2.Distance(AICONTACTManager.Instance.faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[195].transform.position, faceInCenter[0].transform.position);
            
            if (distance > distanceBuffer)
            {
                foreach (GameObject item in faceInCenter)
                    item.SetActive(false);

                foreach (GameObject item in faceNotInCentered)
                    item.SetActive(true);

                faceCenterMessage.SetActive(true);
            }
            else
            {
                foreach (GameObject item in faceInCenter)
                    item.SetActive(true);

                foreach (GameObject item in faceNotInCentered)
                    item.SetActive(false);

                faceCenterMessage.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject item in faceInCenter)
                item.SetActive(false);

            foreach (GameObject item in faceNotInCentered)
                item.SetActive(false);

            faceCenterMessage.SetActive(false);
        }

    }

    public IEnumerator CountdownCouroutine()
    {
        countdown.SetActive(true);
        cameraEyeContactButton.SetActive(false);
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            yield return new WaitForSeconds(1);

            remainingTime -= 1;
        }

        countdownText.text = "00:00";
        AICONTACTManager.Instance.CameraEyeContact(EyeContactStates.NoEyeContact);
    }

    public void Setup()
    {
        //1st Implementation
        Invoke(nameof(ToggleBrightnessAdjuster), 1f);

        //2nd Implementation
        
        

        //3rd Implementation
        //irisBrightnessSlider.slider.value = 0.3f;
       // scleraBrightnessSlider.slider.value = 0.5f;
       // pupilBrightnessSlider.slider.value = 0.3f;

        SetNewIrisColorAndBrightness(PreferenceManager.LensNumber);
        SetPupilBrightness();
        SetScleraBrightness();
    }

    public void OnIrisBrightnessSliderValueChange()
    {
        SetIrisBrightness();
    }

    public void OnScleraBrightnessSliderValueChange()
    {
        SetScleraBrightness();
    }

    public void OnPupilBrightnessSliderValueChange()
    {
        SetPupilBrightness();
    }

    public void ToggleBrightnessAdjuster()
    {
        if (AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource == ImageSourceType.WebCamera)
        {
            if (autoBrightness)
            {
                autoBrightness = false;
                slidersPanel.SetActive(false);

                irisBrightnessSlider.text.text = "Iris";
                scleraBrightnessSlider.text.text = "Sclera";
                pupilBrightnessSlider.text.text = "Pupil";

                scleraBrightnessSlider.text.color = Colors._instance.yellow;
                irisBrightnessSlider.text.color = Colors._instance.yellow;
                pupilBrightnessSlider.text.color = Colors._instance.yellow;

                irisBrightnessSlider.lockedSlider.SetActive(false);
                scleraBrightnessSlider.lockedSlider.SetActive(false);
                pupilBrightnessSlider.lockedSlider.SetActive(false);

                irisBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;
                scleraBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;
                pupilBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;
            }
            else
            {
                autoBrightness = true;
                slidersPanel.SetActive(true);

                irisBrightnessSlider.text.text = "Auto-Iris";
                scleraBrightnessSlider.text.text = "Auto-Sclera";
                pupilBrightnessSlider.text.text = "Auto-Pupil";

                scleraBrightnessSlider.text.color = Colors._instance.grey;
                irisBrightnessSlider.text.color = Colors._instance.grey;
                pupilBrightnessSlider.text.color = Colors._instance.grey;

                irisBrightnessSlider.lockedSlider.SetActive(true);
                scleraBrightnessSlider.lockedSlider.SetActive(true);
                pupilBrightnessSlider.lockedSlider.SetActive(true);

                irisBrightnessSlider.sliderHandle.GetComponent<Image>().color = Colors._instance.darkGrey;
                scleraBrightnessSlider.sliderHandle.GetComponent<Image>().color = Colors._instance.darkGrey;
                pupilBrightnessSlider.sliderHandle.GetComponent<Image>().color = Colors._instance.darkGrey;

                //irisBrightnessSlider.slider.value = 1 - AICONTACTManager.Instance.lightIntensity + 0.3f;
                //scleraBrightnessSlider.slider.value = AICONTACTManager.Instance.lightIntensity + 0.2f;
                //pupilBrightnessSlider.slider.value = 1 - AICONTACTManager.Instance.lightIntensity;
                irisBrightnessSlider.slider.value = PreferenceManager.IrisColorBrightness;
                scleraBrightnessSlider.slider.value = PreferenceManager.ScleraColorBrightness;
                pupilBrightnessSlider.slider.value = PreferenceManager.PupilColorBrightness;
            }
        }
        else
        {
            autoBrightness = false;
            slidersPanel.SetActive(false);

            irisBrightnessSlider.text.text = "Iris";
            scleraBrightnessSlider.text.text = "Sclera";
            pupilBrightnessSlider.text.text = "Pupil";

            scleraBrightnessSlider.text.color = Colors._instance.yellow;
            irisBrightnessSlider.text.color = Colors._instance.yellow;
            pupilBrightnessSlider.text.color = Colors._instance.yellow;

            irisBrightnessSlider.lockedSlider.SetActive(false);
            scleraBrightnessSlider.lockedSlider.SetActive(false);
            pupilBrightnessSlider.lockedSlider.SetActive(false);

            irisBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;
            scleraBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;
            pupilBrightnessSlider.sliderHandle.GetComponent<Image>().color = Color.white;

            irisBrightnessSlider.slider.value = PreferenceManager.IrisColorBrightness;
            scleraBrightnessSlider.slider.value = PreferenceManager.ScleraColorBrightness;
            pupilBrightnessSlider.slider.value = PreferenceManager.PupilColorBrightness;
        }
    }

    public IEnumerator DisplaySignInMessageCouroutine()
    {
        signInMessage.SetActive(true);
        yield return new WaitForSeconds(3f);
        signInMessage.SetActive(false);
    }

    public void Mode(string imageSourceType)
    {
        if (AICONTACTManager.Instance.appVersion == ApplicationVersion.PayPal)
        {
            SetMode(imageSourceType);
        }
        else
        {
            if (!string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
            {
                SetMode(imageSourceType);
            }
            else
            {
                StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().DisplaySignInMessageCouroutine());
            }
        }
    }

    private void SetMode(string imageSourceType)
    {
        if (imageSourceType == ImageSourceType.Video.ToString() && AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource != ImageSourceType.Video)
        {
            var extensions = new[] {
                new ExtensionFilter("Video Files", "mp4", "mov", "avi" )
            };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            if (paths.Length != 0 && !string.IsNullOrEmpty(paths[0]))
            {
                webCamModeToggle.isOn = false;
                videoModeToggle.isOn = true;
                AICONTACTManager.Instance.mediaPipeSolution.GetComponent<IrisTrackingSolution>().Stop();
                AICONTACTManager.Instance.filterImage.texture = null;
                AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource = ImageSourceType.Video;
                ImageSourceProvider.ImageSource = AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>().GetImageSource(AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource);
                AICONTACTManager.Instance.mediaPipeSolution.GetComponent<VideoSource>().videoURL = paths[0];
                AICONTACTManager.Instance.mediaPipeSolution.GetComponent<IrisTrackingSolution>().Play();
                StartCoroutine(AICONTACTManager.Instance.SetNonFilterImage());
            }

        }
        else if (imageSourceType == ImageSourceType.WebCamera.ToString() && AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource != ImageSourceType.WebCamera)
        {
            webCamModeToggle.isOn = true;
            videoModeToggle.isOn = false;
            AICONTACTManager.Instance.mediaPipeSolution.GetComponent<IrisTrackingSolution>().Stop();
            AICONTACTManager.Instance.filterImage.texture = null;
            AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource = ImageSourceType.WebCamera;
            ImageSourceProvider.ImageSource = AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>().GetImageSource(AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource);
            AICONTACTManager.Instance.mediaPipeSolution.GetComponent<IrisTrackingSolution>().Play();
            StartCoroutine(AICONTACTManager.Instance.SetNonFilterImage());
        }
    }

    public void OnDropDownButtonClick()
    {
        ToggleSettingsPanel();
    }

    private void ToggleSettingsPanel()
    {
        toggle = !toggle;
        GetComponent<Animator>().SetBool("Toggle", toggle);

        if (toggle)
            dropDownButton.image.sprite = dropDownSprite;
        else
            dropDownButton.image.sprite = dropUpSprite;
    }

    private void SetNewIrisColorAndBrightness(int lensNumber)
    {
        if (AICONTACTManager.Instance.leftEyeIris && AICONTACTManager.Instance.rightEyeIris)
        {
            PreferenceManager.LensNumber = lensNumber;
            irisBrightnessSlider.sliderBackground.sprite = sliderSprites[lensNumber - 1];
            foreach (GameObject ring in irisButtonRings)
                ring.SetActive(false);
            irisButtonRings[lensNumber - 1].SetActive(true);
            SetIrisBrightness();
        }
    }

    private void SetIrisBrightness()
    {
        if (AICONTACTManager.Instance.leftEyeIris && AICONTACTManager.Instance.rightEyeIris)
        {
            if (irisBrightnessSlider.slider.value == 1)
                irisBrightnessSlider.slider.value = 0.99f;

            irisColor = PickColorFromSlider(irisBrightnessSlider.slider, irisBrightnessSlider.sliderBackground);
            AICONTACTManager.Instance.leftEyeIris.SetIrisColor(irisColor);
            AICONTACTManager.Instance.rightEyeIris.SetIrisColor(irisColor);
            irisColorImage.color = irisColor;
            PreferenceManager.IrisColorBrightness = irisBrightnessSlider.slider.value;
        }
    }

    private void SetScleraBrightness()
    {
        SetUnderLinerBrightness();

        if (AICONTACTManager.Instance.leftEyeSclera && AICONTACTManager.Instance.rightEyeSclera)
        {
            if (scleraBrightnessSlider.slider.value == 1)
                scleraBrightnessSlider.slider.value = 0.99f;

            scleraColor = PickColorFromSlider(scleraBrightnessSlider.slider, scleraBrightnessSlider.sliderBackground);
            AICONTACTManager.Instance.leftEyeSclera.SetColor(scleraColor);
            AICONTACTManager.Instance.rightEyeSclera.SetColor(scleraColor);
            scleraColorImage.color = scleraColor;
            PreferenceManager.ScleraColorBrightness = scleraBrightnessSlider.slider.value;
        }
    }

    private void SetUnderLinerBrightness()
    {
        Color lowBrightnessColor = new Color32(0x66, 0x33, 0x00, 0xFF);
        Color highBrightnessColor = new Color32(0xE9, 0x74, 0x51, 0xFF);

        for (int i = 0; i < AICONTACTManager.Instance.leftEyeLowerAnnotationsPoints.Count; i++)
        {
            if (i != 0)
            {
                if (scleraBrightnessSlider.slider.value <= 0.25f)
                {
                    AICONTACTManager.Instance.leftEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = lowBrightnessColor;
                    AICONTACTManager.Instance.rightEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = lowBrightnessColor;
                }
                    

                if (scleraBrightnessSlider.slider.value >= 0.7f)
                {
                    AICONTACTManager.Instance.leftEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = highBrightnessColor;
                    AICONTACTManager.Instance.rightEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = highBrightnessColor;
                }
                    

                float t = Mathf.InverseLerp(0.25f, 0.7f, scleraBrightnessSlider.slider.value);
                AICONTACTManager.Instance.leftEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = Color.Lerp(lowBrightnessColor, highBrightnessColor, t);
                AICONTACTManager.Instance.rightEyeLowerAnnotationsPoints[i].eyeLiner.GetComponent<SpriteRenderer>().color = Color.Lerp(lowBrightnessColor, highBrightnessColor, t);
            }
        }
    }

    private void SetPupilBrightness()
    {
        if (AICONTACTManager.Instance.leftEyeIris && AICONTACTManager.Instance.rightEyeIris)
        {
            if (pupilBrightnessSlider.slider.value == 1)
                pupilBrightnessSlider.slider.value = 0.99f;

            pupilColor = PickColorFromSlider(pupilBrightnessSlider.slider, pupilBrightnessSlider.sliderBackground);
            AICONTACTManager.Instance.leftEyeIris.SetPupilBrightness(pupilColor);
            AICONTACTManager.Instance.rightEyeIris.SetPupilBrightness(pupilColor);
            pupilColorImage.color = pupilColor;
            PreferenceManager.PupilColorBrightness = pupilBrightnessSlider.slider.value;
        }
    }

    public void OnIrisButtonClick(int lensNumber)
    {
        SetNewIrisColorAndBrightness(lensNumber);
    }

    public void OnSubscribeButtonClick()
    {
        UIManager.Instance.DisplaySpecificScreen(ScreenName.InAppPurchaseSubscriptionScreen);

        foreach (GameObject item in faceInCenter)
            item.SetActive(false);

        foreach (GameObject item in faceNotInCentered)
            item.SetActive(false);
    }

    public void OnGlintButtonClick()
    {
        displayGlint = !displayGlint;

        if (displayGlint)
        {
            AICONTACTManager.Instance.leftEyeIris.ToggleGlint(true);
            AICONTACTManager.Instance.rightEyeIris.ToggleGlint(true);
            glintImage.SetActive(true);
            glintVisibleImage.SetActive(true);
            glintHiddenImage.SetActive(false);
        }
        else
        {
            AICONTACTManager.Instance.leftEyeIris.ToggleGlint(false);
            AICONTACTManager.Instance.rightEyeIris.ToggleGlint(false);
            glintImage.SetActive(false);
            glintVisibleImage.SetActive(false);
            glintHiddenImage.SetActive(true);
        }
    }

    public void OnSwitchCameraButtonClick()
    {
        AICONTACTManager.Instance.mediaPipeSolution.GetComponent<WebCamSource>().SelectSource(1);
    }

    public void OnCameraEyeContactButtonClick()
    {
        if (AICONTACTManager.Instance.currentEyeContactState == EyeContactStates.SemiEyeContact)
        {
            AICONTACTManager.Instance.CameraEyeContact(EyeContactStates.FullEyeContact);
            cameraEyeContactButton.GetComponent<Image>().color = Color.white;   
        }
        else if (AICONTACTManager.Instance.currentEyeContactState == EyeContactStates.FullEyeContact)
        {
            AICONTACTManager.Instance.CameraEyeContact(EyeContactStates.SemiEyeContact);
            cameraEyeContactButton.GetComponent<Image>().color = Colors.Instance.yellow;
        }
    }

    public void OnLogoutButtonClick()
    {
        PreferenceManager.Credentials = JsonConvert.SerializeObject(new LoginData());
        UIManager.Instance.DisplaySpecificScreen(ScreenName.InAppPurchaseSubscriptionScreen);
        UIManager.Instance.UIScreensReferences[ScreenName.InAppPurchaseSubscriptionScreen].GetComponent<InAppPurchaseScreenHandler>().OnOpenSignInPanelButtonClick();

        foreach (GameObject item in faceInCenter)
            item.SetActive(false);

        foreach (GameObject item in faceNotInCentered)
            item.SetActive(false);
    }

    private Color PickColorFromSlider(Slider slider, Image sliderBackground)
    {
        Vector2 localCursor = new Vector2(slider.GetComponent<RectTransform>().sizeDelta.x * slider.value, 0);
        Texture2D textureToPickFrom = sliderBackground.GetComponent<Image>().sprite.texture;
        // Calculate pixel coordinates in the texture
        Rect rect = slider.GetComponent<RectTransform>().rect;
        Vector2 normalizedPos = new Vector2(
            (localCursor.x - rect.x) / rect.width,
            (localCursor.y - rect.y) / rect.height);
        int x = Mathf.FloorToInt(normalizedPos.x * textureToPickFrom.width);
        int y = Mathf.FloorToInt(normalizedPos.y * textureToPickFrom.height);

        // Get the color of the clicked pixel
        return textureToPickFrom.GetPixel(x, y);
    }

    public void OnAICONTACTButtonClick()
    {
        Application.OpenURL("https://facestream.ai/aicontact");
    }

}

[System.Serializable]
public class BrightnessSlider
{
    public Slider slider;
    public TextMeshProUGUI text;
    public Image sliderBackground;
    public GameObject lockedSlider;
    public GameObject sliderHandle;
}