using Mediapipe.Unity;
using UnityEngine;

public class EyeMask : MonoBehaviour
{
    public Transform initialPosition;
    public Transform finalPosition;
    public bool upperLayer;
    public float offset;
    private float updatedOffset;

    public void SetUpEyeMask (Transform _initialPosition, Transform _finalPosition, bool _upperLayer)
    {
        initialPosition = _initialPosition;
        finalPosition = _finalPosition;
        upperLayer = _upperLayer;
    }

    private void Update()
    {
        if(initialPosition && finalPosition)
            ScaleAndPosition();
    }

    public void ScaleAndPosition()
    {
        Vector3 centerPos = (initialPosition.position + finalPosition.position) / 2f;
        transform.position = centerPos;

        if(AICONTACTManager.Instance.expandEyes)
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
        Vector3 scale = new (1, 1, 1);
        scale.x = Vector3.Distance(initialPosition.position, finalPosition.position);
        transform.localScale = scale;
    }
}
