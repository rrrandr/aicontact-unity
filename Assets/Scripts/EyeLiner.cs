using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

public class EyeLiner : MonoBehaviour
{
    public Transform initialPosition;
    public Transform finalPosition;
    public bool upperLayer;
    public Sprite upperEyeLinerSprite;
    public Sprite lowerEyeLinerSprite;
    public Color upperEyeLinerColor;
    public Color lowerEyeLinerColor;
    public float offset;
    private float updatedOffset;

    private bool toggle = false;
    private Coroutine coroutine;

    public void SetUpEyeLiner(Transform _initialPosition, Transform _finalPosition, bool _upperLayer)
    {
        initialPosition = _initialPosition;
        finalPosition = _finalPosition;
        upperLayer = _upperLayer;

        if(upperLayer)
        {
            GetComponent<SpriteRenderer>().sprite = upperEyeLinerSprite;
            GetComponent<SpriteRenderer>().color = upperEyeLinerColor;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = lowerEyeLinerSprite;
            GetComponent<SpriteRenderer>().color = lowerEyeLinerColor;
        }
    }

    private void Update()
    {
        if (initialPosition && finalPosition)
            ScaleAndPosition();
    }

    public void ScaleAndPosition()
    {
        Vector3 centerPos = (initialPosition.position + finalPosition.position) / 2f;
        transform.position = centerPos;

        if (AICONTACTManager.Instance.expandEyes)
        {
            updatedOffset = offset + 2;
        }
        else
        {
            updatedOffset = offset;
        }

        if (AICONTACTManager.Instance.mediaPipeSolution.GetComponent<Bootstrap>()._defaultImageSource == ImageSourceType.WebCamera)
        {
            if (upperLayer)
                transform.localPosition += transform.up * updatedOffset;
            else
                transform.localPosition += transform.up * -updatedOffset;
        }
        else
        {
            if (upperLayer)
                transform.localPosition += transform.up * -updatedOffset;
            else
                transform.localPosition += transform.up * updatedOffset;
        }

        Vector3 direction = finalPosition.position - initialPosition.position;
        direction = Vector3.Normalize(direction);
        transform.right = direction;
        Vector3 scale = new(1, 1, 1);
        scale.x = Vector3.Distance(initialPosition.position, finalPosition.position);
        transform.localScale = scale;
    }

    public void ToggleEyeLiner(bool _toggle, FadeType _fadeType)
    {
        if (toggle != _toggle)
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
}
