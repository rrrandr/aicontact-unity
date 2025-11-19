using Mediapipe.Unity;
using Mediapipe.Unity.IrisTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using UniCamEx;
#endif

public class AICONTACTManager : MonoBehaviour
{
    [Header("MAC OS")]
    public ApplicationVersion appVersion;

    [Header("Camera")]
    public GameObject camera;

    [Header("UI")]
    public Canvas _canvas;
    public WebCamTexture webCamTexture;
    public Texture2D _texture2D;
    public RawImage _image;
    private Coroutine countdownCouroutine;

    [Header("Split Screen")]

    [SerializeField] Toggle faceOutlineToggle;
    [SerializeField] FaceOutline[] faceOutlines;
    [SerializeField] Toggle splitScreenToggle;
    public bool isSplitScreen;
    public RectTransform filterBody;
    public RectTransform nonFilterBody;
    public RawImage filterImage;
    public RawImage nonFilterImage;
    public Image splitScreenIcon;
    private bool setStream = false;
    private bool isFilterOutline = false;

    [Header("Light Intensity Detector")]
    public float lightIntensity;
    public GameObject lowLightWarning;
    private Coroutine lowLightCoroutine;

    [Header("Low Resolution")]
    public GameObject lowResolution;

    [Header("Media Pipe Solution")]
    public GameObject mediaPipeSolution;

    [Header("Face Annotation")]
    public FaceLandmarkListAnnotation faceLandmarkListAnnotation;

    [Header("Iris Annotation")]
    public IrisLandmarkListAnnotation leftIrisLandmarkListAnnotation;
    public IrisLandmarkListAnnotation rightIrisLandmarkListAnnotation;

    [Header("Filters")]
    public string currentFilter;
    public List<Filter> filters;

    [Header("Iris")]
    public Iris irisPrefab;
    public Iris leftEyeIris;
    public Iris rightEyeIris;
    private Coroutine eyeContactTimingCoroutine;

    [Header("Sclera")]
    public Sclera scleraPrefab;
    public Sclera leftEyeSclera;
    public Sclera rightEyeSclera;

    [Header("Glint")]
    public Glint glintPrefab;
    public Glint leftEyeGlint;
    public Glint rightEyeGlint;

    [Header("Pupil Dilation")]
    public float dilationSpeed;
    private string value;
    private float minValue;
    private float maxValue;
    private float currentValue;

    [Header("Filter Toggle")]
    public bool eyesOpen;
    private bool openEyesLastState;
    public bool straightFace;
    private bool straightFaceLastState;
    public float angle;
    public string faceDirection;
    private FadeType lastFadeType;
    public float fadeSpeed;
    public EyeContactStates currentEyeContactState;
    [Range(0, 3)] public float straightFaceBuffer;

    [Header("Iris Layer")]
    public EyeMask layerPrefab;
    private bool layerGenerated;

    [Header("Iris Liner")]
    public EyeLiner eyeLinerPrefab;

    [Header("Eye Layer Points")]
    private float[] increment = { 0, 0.5f, 1f, 1.5f, 2f, 1.5f, 1f, 0.5f, 0 };
    public List<AnnotationPoint> leftEyeUpperAnnotationsPoints;
    public List<AnnotationPoint> leftEyeLowerAnnotationsPoints;
    public List<AnnotationPoint> rightEyeUpperAnnotationsPoints;
    public List<AnnotationPoint> rightEyeLowerAnnotationsPoints;

    [Header("Eyes Tracking")]
    public float threshold;
    public List<GameObject> lookingDownwardTracking;

    [Header("Expand AR Eyes")]
    public bool expandEyes;

    public static AICONTACTManager _instance;
    public static AICONTACTManager Instance
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

    public void Setup()
    {
        if (layerGenerated)
        {
            StopCoroutine(countdownCouroutine);

            if (appVersion == ApplicationVersion.InAppPurchase && string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
            {
                UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().logoutButton.SetActive(false);
                countdownCouroutine = StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().CountdownCouroutine());
            }
            else
            {
                UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().logoutButton.SetActive(true);
                UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().countdown.SetActive(false);
                UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().cameraEyeContactButton.SetActive(true);
            }
        }
    }

    private void Start()
    {

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

        camera.GetComponent<UnityCam>().enabled = false;
        camera.GetComponent<UniCamExSender>().enabled = true;

#else

        camera.GetComponent<UnityCam>().enabled = true;
        //camera.GetComponent<UniCamExSender>().enabled = false;

#endif

        layerGenerated = false;
        nonFilterBody.anchoredPosition = new Vector2(-605f, 100f);
        nonFilterBody.sizeDelta = new Vector2(1200f, 670f);
    }

    public IEnumerator SetNonFilterImage()
    {
        while (!filterImage.texture)
            yield return null;

        nonFilterImage.texture = filterImage.texture;
        nonFilterImage.rectTransform.sizeDelta = filterImage.rectTransform.sizeDelta;
        nonFilterImage.uvRect = filterImage.uvRect;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.X) && UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].activeInHierarchy)
        {
            ToggleSideBySideView(!isSplitScreen);
        }

        if (filterImage.texture != null && !setStream)
        {
            StartCoroutine(SetNonFilterImage());
            isSplitScreen = false;

            if (appVersion == ApplicationVersion.PayPal)
            {
                ToggleSideBySideView(true);
                ToggleFilterOutline(true);
            }
            else if (appVersion == ApplicationVersion.InAppPurchase && !string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
            {
                if (!string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
                {
                    ToggleSideBySideView(true);
                    ToggleFilterOutline(true);
                }
            }
            else
            {
                nonFilterBody.gameObject.SetActive(false);
                ToggleSideBySideView(false);
                ToggleFilterOutline(false);
            }

            setStream = true;
        }

        if (layerGenerated)
        {
            Vector3 direction = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[4].transform.position - faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[168].transform.position;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            angle += 90;

            if (angle >= 20)
                faceDirection = "Right";
            else if (angle <= -20)
                faceDirection = "Left";
            else
                faceDirection = "Straight";

            if (angle > 20 || angle < -20)
            {
                straightFaceLastState = straightFace = false;
            }
            else if (angle <= 18 && angle >= -18 && !straightFaceLastState)
            {
                straightFaceLastState = straightFace = true;
            }

            float xAxisOffset = angle * 0.25f;

            leftEyeIris.xAxisOffset = -xAxisOffset;
            rightEyeIris.xAxisOffset = xAxisOffset;

            //float distance = Vector3.Distance(faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[264].transform.position, faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[356].transform.position);

            //if (distance > straightFaceBuffer)
            //{
            //    straightFaceLastState = straightFace = false;
            //}
            //else if (distance <= (straightFaceBuffer - 0.25f) && !straightFaceLastState)
            //{
            //    straightFaceLastState = straightFace = true;
            //}

            //Looking downward tracking

            lookingDownwardTracking[0].transform.position = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[190].transform.position;
            lookingDownwardTracking[1].transform.position = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[133].transform.position;
            lookingDownwardTracking[2].transform.position = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[133].transform.position;
            lookingDownwardTracking[3].transform.position = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[159].transform.position;

            Vector3 direction1 = lookingDownwardTracking[0].transform.position - lookingDownwardTracking[1].transform.position;
            direction1 = Vector3.Normalize(direction1);
            lookingDownwardTracking[1].transform.up = direction1;

            Vector3 direction2 = lookingDownwardTracking[3].transform.position - lookingDownwardTracking[2].transform.position;
            direction2 = Vector3.Normalize(direction2);
            lookingDownwardTracking[2].transform.up = direction2;

            float AngleDiff = Quaternion.Angle(lookingDownwardTracking[1].transform.rotation, lookingDownwardTracking[2].transform.rotation);

            AngleDiff -= 80;

            if (AngleDiff > 10)
            {
                openEyesLastState = eyesOpen = false;
            }
            else if (AngleDiff <= 5 && !openEyesLastState)
            {
                openEyesLastState = eyesOpen = true;
            }

            ToggleFilter();
        }
    }

    #region Split Screen


    public void ToggleFilterOutline(bool isFilterOutline)
    {
        this.isFilterOutline = isFilterOutline;
        foreach (FaceOutline faceOutline in faceOutlines)
        {
            faceOutline.enabled = isFilterOutline;
        }
        if (faceOutlineToggle != null && faceOutlineToggle.isOn != isFilterOutline)
        {
            faceOutlineToggle.isOn = isFilterOutline;
        }
    }

    public void ToggleSideBySideView(bool isSplitScreen)
    {
        this.isSplitScreen = isSplitScreen;
        if (isSplitScreen)
        {
            splitScreenIcon.color = Colors.Instance.grey;
            nonFilterBody.gameObject.SetActive(false);
            filterBody.anchoredPosition = new Vector2(0, 0);
            filterBody.sizeDelta = new Vector2(2436f, 1370f);
        }
        else
        {
            splitScreenIcon.color = Colors.Instance.yellow;
            nonFilterBody.gameObject.SetActive(true);
            filterBody.anchoredPosition = new Vector2(605f, 100f);
            //filterBody.sizeDelta = new Vector2(1200f, 670f);
            filterBody.sizeDelta = new Vector2(1200f, 1370f);
        }
        if (splitScreenToggle != null && splitScreenToggle.isOn != isSplitScreen)
        {
            splitScreenToggle.isOn = isSplitScreen;
        }
    }

    #endregion


    #region Light Intensity Detector

    public void DisableLightIntensityIndicator()
    {
        StopCoroutine(lowLightCoroutine);
        lowLightWarning.SetActive(false);
    }
    public void SetWebcamTexture(WebCamTexture _webcamTexture)
    {
        webCamTexture = _webcamTexture;
        ChooseFilterQuality(webCamTexture.width, webCamTexture.height);
        lowLightCoroutine = StartCoroutine(CalculateLightIntensityCoroutine());
    }

    Coroutine webCamCoroutine;
    internal void OnCameraChanged(int index)
    {
        if(ImageSourceProvider.ImageSource is WebCamSource)
        {
            ImageSource webCamSource = ImageSourceProvider.ImageSource;
            webCamSource.Stop();
            webCamSource.SelectSource(index);
            if(webCamCoroutine!=null)
                StopCoroutine(webCamCoroutine);
            webCamCoroutine = StartCoroutine(webCamSource.Play());
            mediaPipeSolution.GetComponent<IrisTrackingSolution>().Stop();
            filterImage.texture = null;
            Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource = ImageSourceType.WebCamera;
            ImageSourceProvider.ImageSource = mediaPipeSolution.GetComponent<Bootstrap>().GetImageSource(mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource);
            mediaPipeSolution.GetComponent<IrisTrackingSolution>().Play();
        }
    }

    private IEnumerator CalculateLightIntensityCoroutine()
    {
        int seconds = 0;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            seconds++;

            if (seconds < 5)
            {
                if (webCamTexture.isPlaying)
                {
                    CheckLightIntensity();
                }
            }
            else
            {
                DisableLightIntensityIndicator();
                DisableLowResolutionIndicator();
            }
        }
    }

    public void CheckLightIntensity()
    {
        Texture2D currentFrame = new Texture2D(webCamTexture.width, webCamTexture.height);
        currentFrame.SetPixels(webCamTexture.GetPixels());
        currentFrame.Apply();

        Color[] pixels = currentFrame.GetPixels();
        float totalBrightness = 0f;

        foreach (Color pixel in pixels)
        {
            float brightness = (pixel.r + pixel.g + pixel.b) / 3f;
            totalBrightness += brightness;
        }

        float averageBrightness = totalBrightness / pixels.Length;
        lightIntensity = averageBrightness;

        if (UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<Animator>().GetBool("Toggle") && lightIntensity < 0.5f)
        {
            lowLightWarning.SetActive(true);
        }
        else
        {
            lowLightWarning.SetActive(false);
        }
    }

    #endregion

    public IEnumerator SpawnLens()
    {
        mediaPipeSolution.SetActive(true);
        yield return new WaitWhile(() => leftIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList.Count != 5);
        yield return new WaitWhile(() => rightIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList.Count != 5);
        yield return new WaitWhile(() => faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList.Count != 468);

        //Spawn Mask
        for (int i = 0; i < leftEyeUpperAnnotationsPoints.Count; i++)
        {
            leftEyeUpperAnnotationsPoints[i].pointAnnotation = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[leftEyeUpperAnnotationsPoints[i].indexNumber];

            if (i != 0)
            {
                leftEyeUpperAnnotationsPoints[i].spawnedLayer = Instantiate(layerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                leftEyeUpperAnnotationsPoints[i].spawnedLayer.name = "Left Eye Upper layer " + i;
                leftEyeUpperAnnotationsPoints[i].spawnedLayer.SetUpEyeMask(leftEyeUpperAnnotationsPoints[i - 1].pointAnnotation.transform, leftEyeUpperAnnotationsPoints[i].pointAnnotation.transform, true);

                leftEyeUpperAnnotationsPoints[i].eyeLiner = Instantiate(eyeLinerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                leftEyeUpperAnnotationsPoints[i].eyeLiner.name = "Left Eye Upper Eye Liner " + i;
                leftEyeUpperAnnotationsPoints[i].eyeLiner.SetUpEyeLiner(leftEyeUpperAnnotationsPoints[i - 1].pointAnnotation.transform, leftEyeUpperAnnotationsPoints[i].pointAnnotation.transform, true);
            }
        }

        for (int i = 0; i < leftEyeLowerAnnotationsPoints.Count; i++)
        {
            leftEyeLowerAnnotationsPoints[i].pointAnnotation = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[leftEyeLowerAnnotationsPoints[i].indexNumber];

            if (i != 0)
            {
                leftEyeLowerAnnotationsPoints[i].spawnedLayer = Instantiate(layerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                leftEyeLowerAnnotationsPoints[i].spawnedLayer.name = "Left Eye Lower layer " + i;
                leftEyeLowerAnnotationsPoints[i].spawnedLayer.SetUpEyeMask(leftEyeLowerAnnotationsPoints[i - 1].pointAnnotation.transform, leftEyeLowerAnnotationsPoints[i].pointAnnotation.transform, false);

                leftEyeLowerAnnotationsPoints[i].eyeLiner = Instantiate(eyeLinerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                leftEyeLowerAnnotationsPoints[i].eyeLiner.name = "Left Eye Lower Eye Liner " + i;
                leftEyeLowerAnnotationsPoints[i].eyeLiner.SetUpEyeLiner(leftEyeLowerAnnotationsPoints[i - 1].pointAnnotation.transform, leftEyeLowerAnnotationsPoints[i].pointAnnotation.transform, false);
            }
        }

        for (int i = 0; i < rightEyeUpperAnnotationsPoints.Count; i++)
        {
            rightEyeUpperAnnotationsPoints[i].pointAnnotation = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[rightEyeUpperAnnotationsPoints[i].indexNumber];

            if (i != 0)
            {
                rightEyeUpperAnnotationsPoints[i].spawnedLayer = Instantiate(layerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                rightEyeUpperAnnotationsPoints[i].spawnedLayer.name = "Right Eye Upper layer " + i;
                rightEyeUpperAnnotationsPoints[i].spawnedLayer.SetUpEyeMask(rightEyeUpperAnnotationsPoints[i - 1].pointAnnotation.transform, rightEyeUpperAnnotationsPoints[i].pointAnnotation.transform, true);

                rightEyeUpperAnnotationsPoints[i].eyeLiner = Instantiate(eyeLinerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                rightEyeUpperAnnotationsPoints[i].eyeLiner.name = "Right Eye Upper Eye Liner " + i;
                rightEyeUpperAnnotationsPoints[i].eyeLiner.SetUpEyeLiner(rightEyeUpperAnnotationsPoints[i - 1].pointAnnotation.transform, rightEyeUpperAnnotationsPoints[i].pointAnnotation.transform, true);
            }
        }

        for (int i = 0; i < rightEyeLowerAnnotationsPoints.Count; i++)
        {
            rightEyeLowerAnnotationsPoints[i].pointAnnotation = faceLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[rightEyeLowerAnnotationsPoints[i].indexNumber];

            if (i != 0)
            {
                rightEyeLowerAnnotationsPoints[i].spawnedLayer = Instantiate(layerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                rightEyeLowerAnnotationsPoints[i].spawnedLayer.name = "Right Eye Lower layer " + i;
                rightEyeLowerAnnotationsPoints[i].spawnedLayer.SetUpEyeMask(rightEyeLowerAnnotationsPoints[i - 1].pointAnnotation.transform, rightEyeLowerAnnotationsPoints[i].pointAnnotation.transform, false);

                rightEyeLowerAnnotationsPoints[i].eyeLiner = Instantiate(eyeLinerPrefab, faceLandmarkListAnnotation._landmarkListAnnotation.transform);
                rightEyeLowerAnnotationsPoints[i].eyeLiner.name = "Right Eye Lower Eye Liner " + i;
                rightEyeLowerAnnotationsPoints[i].eyeLiner.SetUpEyeLiner(rightEyeLowerAnnotationsPoints[i - 1].pointAnnotation.transform, rightEyeLowerAnnotationsPoints[i].pointAnnotation.transform, false);
            }
        }

        //Spawn Iris
        leftEyeIris = Instantiate(irisPrefab, leftIrisLandmarkListAnnotation.transform);
        leftEyeIris.Setup("Left", leftIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[0].transform, leftEyeUpperAnnotationsPoints[8].pointAnnotation.transform,
            leftEyeUpperAnnotationsPoints[0].pointAnnotation.transform,
            leftIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[1].transform,
            leftIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[3].transform);

        rightEyeIris = Instantiate(irisPrefab, rightIrisLandmarkListAnnotation.transform);
        rightEyeIris.Setup("Right", rightIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[0].transform, rightEyeUpperAnnotationsPoints[0].pointAnnotation.transform,
            rightEyeUpperAnnotationsPoints[8].pointAnnotation.transform,
            rightIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[1].transform,
            rightIrisLandmarkListAnnotation._landmarkListAnnotation.pointAnnotationsList[3].transform);

        //Spawn Sclera
        leftEyeSclera = Instantiate(scleraPrefab, leftIrisLandmarkListAnnotation.transform);
        leftEyeSclera.Setup(leftEyeUpperAnnotationsPoints[0].pointAnnotation.transform, leftEyeUpperAnnotationsPoints[8].pointAnnotation.transform, leftEyeUpperAnnotationsPoints[4].pointAnnotation.transform, leftEyeLowerAnnotationsPoints[5].pointAnnotation.transform);
        rightEyeSclera = Instantiate(scleraPrefab, rightIrisLandmarkListAnnotation.transform);
        rightEyeSclera.Setup(rightEyeUpperAnnotationsPoints[0].pointAnnotation.transform, rightEyeUpperAnnotationsPoints[8].pointAnnotation.transform, rightEyeUpperAnnotationsPoints[4].pointAnnotation.transform, rightEyeLowerAnnotationsPoints[5].pointAnnotation.transform);

        //Spawn Glint
        //leftEyeGlint = Instantiate(glintPrefab, leftIrisLandmarkListAnnotation.transform);
        //leftEyeGlint.Setup("Left", leftEyeUpperAnnotationsPoints[0].pointAnnotation.transform, leftEyeUpperAnnotationsPoints[8].pointAnnotation.transform);
        //rightEyeGlint = Instantiate(glintPrefab, rightIrisLandmarkListAnnotation.transform);
        //rightEyeGlint.Setup("Right", rightEyeUpperAnnotationsPoints[0].pointAnnotation.transform, rightEyeUpperAnnotationsPoints[8].pointAnnotation.transform);

        UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().Setup();
        StartCoroutine(PupilDilation());

        CameraEyeContact(EyeContactStates.SemiEyeContact);
        layerGenerated = true;

        if (appVersion == ApplicationVersion.InAppPurchase && string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
        {
            countdownCouroutine = StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().CountdownCouroutine());
        }
        else
        {
            UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().countdown.SetActive(false);
            UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().cameraEyeContactButton.SetActive(true);
        }
    }

    public void DisableLowResolutionIndicator()
    {
        lowResolution.SetActive(false);
    }

    public void ChooseFilterQuality(int width, int height)
    {
        float megaPixels = width * height;
        megaPixels /= 1000000;
        Debug.Log("Webcam Resolution: " + width + "x" + height + "\n Mega Pixels: " + megaPixels);
        if (megaPixels > 0 && megaPixels <= filters[0].megaPixels)
        {
            currentFilter = "Low";
            lowResolution.SetActive(true);

        }
        else if (megaPixels > filters[0].megaPixels && megaPixels <= filters[1].megaPixels)
        {
            currentFilter = "Medium";
            lowResolution.SetActive(false);
        }
        else if (megaPixels > filters[1].megaPixels)
        {
            currentFilter = "High";
            lowResolution.SetActive(false);
        }
    }

    public void SetFilterQuality()
    {
        foreach (var filter in filters.Where(f => f.filterName == currentFilter))
        {
            leftEyeSclera.GetComponent<SpriteRenderer>().sprite = filter.scleraSprite;
            rightEyeSclera.GetComponent<SpriteRenderer>().sprite = filter.scleraSprite;
            leftEyeIris.GetComponent<SpriteRenderer>().sprite = filter.lensSprite;
            leftEyeIris.iris.GetComponent<SpriteRenderer>().sprite = filter.lensSprite;
            rightEyeIris.GetComponent<SpriteRenderer>().sprite = filter.lensSprite;
            rightEyeIris.iris.GetComponent<SpriteRenderer>().sprite = filter.lensSprite;
        }
    }

    private void ToggleFilter()
    {
        bool toggle = false;
        FadeType fadeType = FadeType.Immediately;
        if (straightFace && eyesOpen)
        {
            toggle = true;
            fadeType = lastFadeType;
        }
        else if (straightFace && !eyesOpen)
        {
            toggle = false;
            lastFadeType = fadeType = FadeType.Immediately;
        }
        else if (!straightFace && eyesOpen)
        {
            toggle = false;
            lastFadeType = fadeType = FadeType.Gradually;
        }

        leftEyeSclera.ToggleSclera(toggle, fadeType);
        rightEyeSclera.ToggleSclera(toggle, fadeType);

        leftEyeIris.ToggleIris(toggle, fadeType);
        rightEyeIris.ToggleIris(toggle, fadeType);

        //rightEyeGlint.ToggleGlint(toggle, fadeType);
        //leftEyeGlint.ToggleGlint(toggle, fadeType);

        foreach (AnnotationPoint annotationPoint in leftEyeUpperAnnotationsPoints)
        {
            if (annotationPoint.eyeLiner)
                annotationPoint.eyeLiner.ToggleEyeLiner(toggle, fadeType);
        }

        foreach (AnnotationPoint annotationPoint in leftEyeLowerAnnotationsPoints)
        {
            if (annotationPoint.eyeLiner)
                annotationPoint.eyeLiner.ToggleEyeLiner(toggle, fadeType);
        }

        foreach (AnnotationPoint annotationPoint in rightEyeUpperAnnotationsPoints)
        {
            if (annotationPoint.eyeLiner)
                annotationPoint.eyeLiner.ToggleEyeLiner(toggle, fadeType);
        }

        foreach (AnnotationPoint annotationPoint in rightEyeLowerAnnotationsPoints)
        {
            if (annotationPoint.eyeLiner)
                annotationPoint.eyeLiner.ToggleEyeLiner(toggle, fadeType);
        }
    }

    private IEnumerator PupilDilation()
    {
        value = "Increase";
        currentValue = 1;

        while (true)
        {
            if (value == "Increase")
            {
                if (currentValue <= maxValue)
                {
                    currentValue += dilationSpeed;
                    rightEyeIris.pupil.transform.localScale = new Vector3(currentValue, currentValue);
                    leftEyeIris.pupil.transform.localScale = new Vector3(currentValue, currentValue);
                }
                else
                {
                    value = "Decrease";
                    minValue = Random.Range(0.5f, 1f);
                }

                yield return new WaitForSeconds(0.1f);
            }
            else if (value == "Decrease")
            {
                if (currentValue >= minValue)
                {
                    currentValue -= dilationSpeed;
                    leftEyeIris.pupil.transform.localScale = new Vector3(currentValue, currentValue);
                    rightEyeIris.pupil.transform.localScale = new Vector3(currentValue, currentValue);
                }
                else
                {
                    value = "Increase";
                    maxValue = Random.Range(1, 1.2f);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void CameraEyeContact(EyeContactStates state)
    {
        currentEyeContactState = state;

        if (state == EyeContactStates.SemiEyeContact)
        {
            if(eyeContactTimingCoroutine!=null)
                StopCoroutine(eyeContactTimingCoroutine);
            eyeContactTimingCoroutine = StartCoroutine(EyeContactTiming());
        }
        else if (state == EyeContactStates.NoEyeContact)
        {
            if(eyeContactTimingCoroutine!=null)
                StopCoroutine(eyeContactTimingCoroutine);
            leftEyeIris.isFollowingIris = true;
            rightEyeIris.isFollowingIris = true;
        }
        else if (state == EyeContactStates.FullEyeContact)
        {
            if(eyeContactTimingCoroutine!=null)
                StopCoroutine(eyeContactTimingCoroutine);
            leftEyeIris.isFollowingIris = false;
            rightEyeIris.isFollowingIris = false;
        }
    }

    private IEnumerator EyeContactTiming()
    {
        int timer = Random.Range(5, 11);
        while (leftEyeIris == null || rightEyeIris == null)
        {
            yield return null;
        }

        while (true)
        {
            if (leftEyeIris.isFollowingIris)
            {
                //AR Iris is following actual iris
                if (timer == 0)
                {
                    leftEyeIris.isFollowingIris = false;
                    rightEyeIris.isFollowingIris = false;
                    timer = Random.Range(5, 11);
                }
                yield return new WaitForSeconds(1f);
                timer--;
            }
            else
            {
                //AR Iris is making camera eye contact
                if (timer == 0)
                {
                    leftEyeIris.isFollowingIris = true;
                    rightEyeIris.isFollowingIris = true;
                    timer = Random.Range(3, 6);
                }
                yield return new WaitForSeconds(1f);
                timer--;
            }
        }
    }

    #region Helper Methods

    private Color CalculateAverageColor(List<Color> colors)
    {
        float avgR = 0;
        float avgG = 0;
        float avgB = 0;
        float avgA = 0;

        foreach (Color color in colors)
        {
            avgR += color.r;
            avgG += color.g;
            avgB += color.b;
            avgA += color.a;
        }

        avgR /= colors.Count;
        avgG /= colors.Count;
        avgB /= colors.Count;
        avgA /= colors.Count;

        return new Color(avgR, avgG, avgB, avgA);
    }

    private Color AdjustBrightness(Color color, float factor)
    {
        // Convert color to HSB
        Color.RGBToHSV(color, out float h, out float s, out float b);

        // Adjust brightness
        b = Mathf.Clamp01(b * factor);

        // Convert back to RGB
        return Color.HSVToRGB(h, s, b);
    }

    private Color PickPixelsFromWebCamTexture2D(List<Vector3> _positions)
    {
        _texture2D = new Texture2D(webCamTexture.width, webCamTexture.height);
        _texture2D.SetPixels32(webCamTexture.GetPixels32());
        _texture2D.Apply();
        _texture2D = FlipTexture(_texture2D, false);
        _texture2D = Resize(_texture2D, UnityEngine.Screen.width, UnityEngine.Screen.height);

        List<Color> colors = new();
        List<Vector2> textureCoordinates = new();
        foreach (Vector3 pos in _positions)
        {
            textureCoordinates.Add(Camera.main.WorldToScreenPoint(pos));
            colors.Add(_texture2D.GetPixel((int)textureCoordinates.Last().x, (int)textureCoordinates.Last().y));
        }

        Color color = CalculateAverageColor(colors);

        //testing
        _image.rectTransform.sizeDelta = new Vector2(_texture2D.width, _texture2D.height);
        _image.texture = _texture2D;

        foreach (Vector2 pos in textureCoordinates)
        {
            _texture2D.SetPixel((int)pos.x, (int)pos.y, Color.red);
        }

        _texture2D.Apply();
        //testing

        return color;
    }

    private Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (upSideDown)
                {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                }
                else
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }

    private Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    private string ColorToHex(Color color)
    {
        // Convert each color component to hexadecimal and concatenate
        int r = (int)(color.r * 255);
        int g = (int)(color.g * 255);
        int b = (int)(color.b * 255);

        string hex = string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);

        return hex;
    }

    private Color HexToColor(string hex)
    {
        // Remove '#' if present
        hex = hex.Replace("#", "");

        // Parse hexadecimal values
        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int hexValue))
        {
            // Extract RGB components
            int r = (hexValue >> 16) & 0xFF;
            int g = (hexValue >> 8) & 0xFF;
            int b = hexValue & 0xFF;

            // Normalize and create Color
            return new Color(r / 255f, g / 255f, b / 255f);
        }
        else
        {
            Debug.LogError("Invalid hexadecimal color format: " + hex);
            return Color.black;
        }
    }
   

    #endregion
}


[System.Serializable]
public class AnnotationPoint
{
    public int indexNumber;
    public PointAnnotation pointAnnotation;
    public EyeMask spawnedLayer;
    public EyeLiner eyeLiner;
}

[System.Serializable]
public class Filter
{
    public string filterName;
    public int megaPixels;
    public Sprite scleraSprite;
    public Sprite lensSprite;
}

public enum FadeType
{
    Gradually,
    Immediately
}

public enum ApplicationVersion
{
    PayPal,
    InAppPurchase
}

public enum EyeContactStates
{
    SemiEyeContact,
    NoEyeContact,
    FullEyeContact
}