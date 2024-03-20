using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
  [SerializeField]
  private int _width;

  [SerializeField]
  private int _height;

  [SerializeField]
  private GameObject _tilePrefab;

  private GridLayoutGroup _gridLayoutGroup;

  private BackgroundTile[,] _backgroundTiles;

  private void Start()
  {
    _gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
    
    _backgroundTiles = new BackgroundTile[_width, _height];

    _gridLayoutGroup.constraintCount = _width;

    Setup();
  }

  private void Setup()
  {
    for (int i = 0; i < _width; i++)
    {
      for (int j = 0; j < _height; j++)
      {
        Vector2 position = new(i, j);
        GameObject tile = Instantiate(_tilePrefab, position, quaternion.identity, transform);
        tile.name = $"({i}, {j})";
      }
    }
  }
}