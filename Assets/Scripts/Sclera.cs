using System.Collections;
using UnityEngine;

public class Sclera : MonoBehaviour
{
    public float yAxisOffset;
    public Transform xInitialPosition;
    public Transform xFinalPosition;
    public Transform yInitialPosition;
    public Transform yFinalPosition;

    private bool toggle = false;
    private Coroutine coroutine;

    public void Setup(Transform _xInitialPosition, Transform _xFinalPosition, Transform _yInitialPosition, Transform _yFinalPosition)
    {
        xInitialPosition = _xInitialPosition;
        xFinalPosition = _xFinalPosition;
        yInitialPosition = _yInitialPosition;
        yFinalPosition = _yFinalPosition;
    }

    private void Update()
    {
        if (UIManager.Instance.UIScreensReferences[ScreenName.MainScreen].activeInHierarchy)
            GetComponent<SpriteRenderer>().enabled = true;
        else
            GetComponent<SpriteRenderer>().enabled = false;

        if (xInitialPosition && xFinalPosition && yInitialPosition && yFinalPosition)
            ScaleAndPosition();
    }

    public void ScaleAndPosition()
    {
        //Positioning
        Vector3 centerPos = (xInitialPosition.position + xFinalPosition.position) / 2f;
        transform.position = centerPos;
        Vector3 direction = xFinalPosition.position - xInitialPosition.position;
        direction = Vector3.Normalize(direction);
        transform.right = direction;
        transform.localPosition += transform.up * yAxisOffset;

        //Scaling
        Vector2 scale = new(1, 1);
        scale.x = Vector2.Distance(xInitialPosition.position, xFinalPosition.position);
        scale.y = Vector2.Distance(yInitialPosition.position, yFinalPosition.position);

        if (AICONTACTManager.Instance.expandEyes)
        {
            scale.x += 0.25f;
            scale.y += 0.40f;
        }
        
        gameObject.transform.localScale = scale;
    }

    public void ToggleSclera(bool _toggle, FadeType _fadeType)
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
                GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, _toggle ? 1 : 0);
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

    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}


