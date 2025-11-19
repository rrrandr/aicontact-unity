using RockVR.Video;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenRecordingManager : MonoBehaviour
{
    public GameObject windowsScreenRecording;
    public GameObject macOSScreenRecording;
    public bool isRecording;
    public string recordingTime;
    private int minutes;
    private int seconds;
    public TextMeshProUGUI recordTimeText;
    public Image recordingDotImage;
    
    private Coroutine recordingCoroutine;

    private void Start()
    {

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        windowsScreenRecording.SetActive(true);
        macOSScreenRecording.SetActive(false);

        recordingTime = "00:00";
        recordTimeText.text = "00 : 00";

#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      
        windowsScreenRecording.SetActive(false);
        macOSScreenRecording.SetActive(true);

#endif

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].activeInHierarchy && windowsScreenRecording.activeInHierarchy)
        {
            ScreenRecording();
        }
    }

    public void ScreenRecording()
    {
        if (AICONTACTManager.Instance.appVersion == ApplicationVersion.PayPal)
        {
            ToggleScreenRecording();
        }
        else
        {
            if (!string.IsNullOrEmpty(DatabaseManager.Instance.currentUser.email))
            {
                ToggleScreenRecording();
            }
            else
            {
                StartCoroutine(UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].GetComponent<MainScreenHandler>().DisplaySignInMessageCouroutine());
            }
        }
    }

    public void ToggleScreenRecording()
    {
        if (!isRecording)
        {
            Debug.Log("Recording Started");
            isRecording = true;
            recordTimeText.color = Colors._instance.yellow;
            recordingCoroutine = StartCoroutine(RecordingCoroutine());
            VideoCaptureCtrl.instance.StartCapture();
            recordingDotImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Recording Stoped");
            isRecording = false;
            StopCoroutine(recordingCoroutine);
            recordTimeText.color = Colors._instance.grey;
            recordingDotImage.gameObject.SetActive(false);
            minutes = 0;
            seconds = 0;
            recordingTime = "00:00";
            recordTimeText.text = "00 : 00";
            VideoCaptureCtrl.instance.StopCapture();
        }
    }

    private IEnumerator RecordingCoroutine()
    {
        string minutesString;
        string secondsString;

        while (minutes != 10)
        {
            yield return new WaitForSeconds(1f);
            seconds++;
            if (seconds == 60)
            {
                minutes++;
                seconds = 0;
            }
            
            if (minutes < 10)
                minutesString = "0" + minutes;
            else
                minutesString = minutes.ToString();

            if (seconds < 10)
                secondsString = "0" + seconds;
            else
                secondsString = seconds.ToString();

            recordingTime = minutesString + ":" + secondsString;
            recordTimeText.text = minutesString + " : " + secondsString;
            recordingDotImage.gameObject.SetActive(!recordingDotImage.gameObject.activeInHierarchy);
        }

        ToggleScreenRecording();
    }
}
