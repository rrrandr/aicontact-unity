using System.Collections;
using UnityEngine;

public class FaceOutline : MonoBehaviour
{
    public GameObject isFiltered;
    public CanvasGroup canvasGroup;
    public FacePosition facePosition;
    
    public float duration;

    private void OnEnable()
    {
        if (facePosition == FacePosition.NotCentered)
        {
            StartCoroutine(FadeToggleLoop());
        }
        else if(facePosition == FacePosition.Centerd)
        {
            StartCoroutine(FadeGreen());
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0;
    }
    void Update()
    {
        transform.localScale = new Vector3(1, 1, 1) * (isFiltered.activeInHierarchy?125:250);
    }

    private IEnumerator FadeToggleLoop()
    {
        while (true)
        {
            // Fade from 0 to 1
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                yield return null;
            }

            // Fade from 1 to 0
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                yield return null;
            }
        }
    }

    private IEnumerator FadeGreen()
    {
        float elapsedTime = 0f;
        float duration = 2f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }
    }

}

public enum FacePosition
{
    Centerd,
    NotCentered
}
