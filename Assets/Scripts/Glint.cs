using System.Collections;
using UnityEngine;

public class Glint : MonoBehaviour
{
    public float xAxisOffset;
    public float yAxisOffset;
    public string eye;
    public Transform xInitialPosition;
    public Transform xFinalPosition;

    private bool toggleGlint = true;
    private bool toggle = false;
    private Coroutine coroutine;

    public void Setup(string _eye, Transform _xInitialPosition, Transform _xFinalPosition)
    {
        eye = _eye;
        xInitialPosition = _xInitialPosition;
        xFinalPosition = _xFinalPosition;

        if (eye == "Right")
            transform.eulerAngles = new Vector3(0, 180, 0);
        else if (eye == "Left")
            transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        if (UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].activeInHierarchy)
            GetComponent<SpriteRenderer>().enabled = true;
        else
            GetComponent<SpriteRenderer>().enabled = false;

        if (xInitialPosition && xFinalPosition)
            ScaleAndPosition();
    }

    public void ScaleAndPosition()
    {
        gameObject.transform.position = (xInitialPosition.position + xFinalPosition.position) / 2f;
        transform.localPosition += transform.up * yAxisOffset;
        transform.localPosition += transform.right * xAxisOffset;
    }

    public void ToggleGlint(bool _toggle, FadeType _fadeType)
    {
        if (toggleGlint && toggle != _toggle)
        {
            toggle = _toggle;

            if (_fadeType == FadeType.Gradually)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                StartCoroutine(FadeFilter(_toggle));
            }
            else
            {
                int value = _toggle ? 1 : 0;
                GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, value);
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
                GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, value);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            value = 1;
            while (value >= 0)
            {
                value -= AICONTACTManager.Instance.fadeSpeed;
                GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, value);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void ToggleGlint(bool _toggleGlint)
    {
        toggleGlint = _toggleGlint;
        GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, _toggleGlint ? 1 : 0);
    }
}
