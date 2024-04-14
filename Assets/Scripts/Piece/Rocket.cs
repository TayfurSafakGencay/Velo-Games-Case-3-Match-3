using System.Collections;
using BoardMain;
using UnityEngine;

namespace Piece
{
  public class Rocket : MonoBehaviour
  {
    public int MoveSpeed;

    private Vector3 _moveDirection;

    private Rigidbody2D rb;

    private Board _board;
    
    private void Awake()
    {
      rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void SetBoard(Board board)
    {
      _board = board;
    }

    private void Update()
    {
      rb.velocity = _moveDirection * MoveSpeed;
    }

    private void OnEnable()
    {
      StartCoroutine(WaitDisable());

      _moveDirection = gameObject.name switch
      {
        RocketType.Up => new Vector3(0, -1, 0),
        RocketType.Down => new Vector3(0, 1, 0),
        RocketType.Right => new Vector3(1, 0, 0),
        RocketType.Left => new Vector3(-1, 0, 0),
        _ => _moveDirection
      };
    }

    private const float _disableTime = 1.5f;
    private IEnumerator WaitDisable()
    {
      yield return new WaitForSeconds(_disableTime);
      
      gameObject.SetActive(false);
    }
  }

  public abstract class RocketType
  {
    public const string Up = "Up Rocket";
    public const string Down = "Down Rocket";
    public const string Right = "Right Rocket";
    public const string Left = "Left Rocket";
  }
}