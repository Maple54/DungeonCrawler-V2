using System.Collections;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    [SerializeField] private float sinkDepth = 4f;
    [SerializeField] private float openDuration = 1.2f;

    public void Open()
    {
        StartCoroutine(SlideDoor());
    }

    private IEnumerator SlideDoor()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * sinkDepth;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / openDuration);
            yield return null;
        }

        transform.position = endPos;
        GetComponent<Collider>().enabled = false;
    }
}