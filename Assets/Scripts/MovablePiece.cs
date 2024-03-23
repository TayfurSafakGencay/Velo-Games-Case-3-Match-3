using System.Collections;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private GamePiece _piece;

    private IEnumerator moveCoroutine;

    private void Awake()
    {
        _piece = gameObject.GetComponent<GamePiece>();
    }

    public void Move(int newX, int newY, float time)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        _piece.X = newX;
        _piece.Y = newY;
        
        Vector2 startPos = transform.position;
        Vector2 endPos = _piece.BoardRef.GetWorldPosition(newX, newY);

        for (float t = 0; t <= time; t += Time.deltaTime)
        {
            _piece.transform.position = Vector2.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        _piece.transform.position = endPos;
    }
}
