using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

public class Iris : MonoBehaviour
{
    public bool straightFace;
    public float xAxisOffset;
    public float yAxisOffset;
    public string eye;
    public bool isFollowingIris;
    public Transform initialPosition;
    public Transform finalPosition;
    public Transform actualIrisPosition;
    public Transform scalingInitialValue;
    public Transform scalingFinalValue;
    public float scalingFactor;
    public GameObject iris;
    public GameObject irisOuterLayer;
    public GameObject pupil;
    public GameObject glint;

    private bool toggleGlint = true;
    private bool toggle = false;
    private Coroutine coroutine;

    public void Setup(string _eye, Transform _actualIrisPosition, Transform _initialPosition, Transform _finalPosition, Transform _scalingInitialValue, Transform _scalingFinalValue)
    {
        isFollowingIris = false;
        eye = _eye;
        actualIrisPosition = _actualIrisPosition;
        initialPosition = _initialPosition;
        finalPosition = _finalPosition;
        scalingInitialValue = _scalingInitialValue;
        scalingFinalValue = _scalingFinalValue;

        //if (eye == "Right")
        //    glint.transform.localPosition = new Vector3(1.4f, 1.4f, 0);
        //else if (eye == "Left")
        //    glint.transform.localPosition = new Vector3(-1.4f, 1.4f, 0);
    }

    private void Update()
    {
        if (UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].activeInHierarchy)
        {
            iris.SetActive(true);
            irisOuterLayer.SetActive(true);
            pupil.SetActive(true);
            glint.SetActive(true);
        }
        else
        {
            iris.SetActive(false);
            irisOuterLayer.SetActive(false);
            pupil.SetActive(false);
            glint.SetActive(false);
        }

        if (scalingInitialValue && scalingFinalValue)
            ScaleAndPosition();

        float partHeight = UnityEngine.Screen.height / 3;
        float yPosition = Camera.main.WorldToScreenPoint(transform.position).y;

        if (yPosition < partHeight)                //Bottom Screen
        {
            //yAxisOffset = 15;
            yAxisOffset = 11;
        }
        else if (yPosition < partHeight * 2)       //Middle Screen
        {
            //yAxisOffset = 11.5f;
            yAxisOffset = 9f;
        }
        else                                       //Upper Screen
        {
            //yAxisOffset = 10;
            yAxisOffset = 8;
        }

        //Face in Bottom/Middle of the Screen and making camera eye contact
        if ((yPosition < partHeight || yPosition < partHeight * 2) && !isFollowingIris)
        {
            AICONTACTManager.Instance.expandEyes = true;
        }
        else
        {
            AICONTACTManager.Instance.expandEyes = false;
        }
    }

    public void ScaleAndPosition()
    {
        if (AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource == ImageSourceType.Video)
        {
            transform.position = (initialPosition.position + finalPosition.position) / 2f;

            if (eye == "Right")
                transform.localPosition += transform.right * -(xAxisOffset - 3);
            else if (eye == "Left")
                transform.localPosition += transform.right * (xAxisOffset - 3);

            transform.localPosition += transform.up * (yAxisOffset - 3);
        }
        else
        {
            if (isFollowingIris)
            {
                transform.position = actualIrisPosition.position;
            }
            else
            {
                transform.position = (initialPosition.position + finalPosition.position) / 2f;

                if (AICONTACTManager.Instance.angle > -10 && AICONTACTManager.Instance.angle < 10)
                {
                    straightFace = true;
                    //When looking straight don't move the iris on x-axis
                    transform.position = actualIrisPosition.position;
                }
                else
                {
                    straightFace = false;
                    if (eye == "Right")
                        transform.localPosition += transform.right * -xAxisOffset;
                    else if (eye == "Left")
                        transform.localPosition += transform.right * xAxisOffset;
                }

                transform.localPosition += transform.up * yAxisOffset;
            }
        }

        float distance = Vector3.Distance(scalingInitialValue.position, scalingFinalValue.position);
        distance += scalingFactor;
        transform.localScale = new Vector2(distance, distance);
    }

    public void ToggleIris(bool active, FadeType fadeType)
    {
        if (toggle != active)
        {
            toggle = active;
            if (fadeType == FadeType.Gradually)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                StartCoroutine(FadeFilter(active));
            }
            else 
            {
                int value = active ? 1 : 0;
                iris.GetComponent<SpriteRenderer>().color = new Color(iris.GetComponent<SpriteRenderer>().color.r, iris.GetComponent<SpriteRenderer>().color.g, iris.GetComponent<SpriteRenderer>().color.b, value);
                irisOuterLayer.GetComponent<SpriteRenderer>().color = new Color(irisOuterLayer.GetComponent<SpriteRenderer>().color.r, irisOuterLayer.GetComponent<SpriteRenderer>().color.g, irisOuterLayer.GetComponent<SpriteRenderer>().color.b, value);
                pupil.GetComponent<SpriteRenderer>().color = new Color(pupil.GetComponent<SpriteRenderer>().color.r, pupil.GetComponent<SpriteRenderer>().color.g, pupil.GetComponent<SpriteRenderer>().color.b, value);
                
                if(toggleGlint)
                    glint.GetComponent<SpriteRenderer>().color = new Color(glint.GetComponent<SpriteRenderer>().color.r, glint.GetComponent<SpriteRenderer>().color.g, glint.GetComponent<SpriteRenderer>().color.b, value);
            }
        }
    }

    private IEnumerator FadeFilter(bool _toggle)
    {
        float value;
        if (_toggle)
        {
            value = 0;
            while (value <= 1)
            {
                value += AICONTACTManager.Instance.fadeSpeed;
                iris.GetComponent<SpriteRenderer>().color = new Color(iris.GetComponent<SpriteRenderer>().color.r, iris.GetComponent<SpriteRenderer>().color.g, iris.GetComponent<SpriteRenderer>().color.b, value);
                irisOuterLayer.GetComponent<SpriteRenderer>().color = new Color(irisOuterLayer.GetComponent<SpriteRenderer>().color.r, irisOuterLayer.GetComponent<SpriteRenderer>().color.g, irisOuterLayer.GetComponent<SpriteRenderer>().color.b, value);
                pupil.GetComponent<SpriteRenderer>().color = new Color(pupil.GetComponent<SpriteRenderer>().color.r, pupil.GetComponent<SpriteRenderer>().color.g, pupil.GetComponent<SpriteRenderer>().color.b, value);

                if (toggleGlint)
                    glint.GetComponent<SpriteRenderer>().color = new Color(glint.GetComponent<SpriteRenderer>().color.r, glint.GetComponent<SpriteRenderer>().color.g, glint.GetComponent<SpriteRenderer>().color.b, value);
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            value = 1;
            while (value >= 0)
            {
                value -= AICONTACTManager.Instance.fadeSpeed;
                iris.GetComponent<SpriteRenderer>().color = new Color(iris.GetComponent<SpriteRenderer>().color.r, iris.GetComponent<SpriteRenderer>().color.g, iris.GetComponent<SpriteRenderer>().color.b, value);
                irisOuterLayer.GetComponent<SpriteRenderer>().color = new Color(irisOuterLayer.GetComponent<SpriteRenderer>().color.r, irisOuterLayer.GetComponent<SpriteRenderer>().color.g, irisOuterLayer.GetComponent<SpriteRenderer>().color.b, value);
                pupil.GetComponent<SpriteRenderer>().color = new Color(pupil.GetComponent<SpriteRenderer>().color.r, pupil.GetComponent<SpriteRenderer>().color.g, pupil.GetComponent<SpriteRenderer>().color.b, value);

                if (toggleGlint)
                    glint.GetComponent<SpriteRenderer>().color = new Color(glint.GetComponent<SpriteRenderer>().color.r, glint.GetComponent<SpriteRenderer>().color.g, glint.GetComponent<SpriteRenderer>().color.b, value);
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void SetIrisColor(Color color)
    {
        iris.GetComponent<SpriteRenderer>().color = color;
    }
    
    public void SetPupilBrightness(Color color)
    {
        pupil.GetComponent<SpriteRenderer>().color = color;
        irisOuterLayer.GetComponent<SpriteRenderer>().color = color;
    }

    public void ToggleGlint(bool _toggleGlint)
    {
        toggleGlint = _toggleGlint;
        glint.GetComponent<SpriteRenderer>().color = new Color(glint.GetComponent<SpriteRenderer>().color.r, glint.GetComponent<SpriteRenderer>().color.g, glint.GetComponent<SpriteRenderer>().color.b, _toggleGlint ? 1 : 0);
    }
}
