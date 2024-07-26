using System.Collections;
using UnityEngine;

public class UISelectedFrameScript : MonoBehaviour
{
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.localPosition = newPosition;
    }

    public void ReturnToOriginalPosition()
    {
        StartCoroutine(MoveToPosition(originalPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float time = 0.5f; // duration of the animation
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
