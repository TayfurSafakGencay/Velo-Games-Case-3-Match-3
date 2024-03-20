using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BackgroundTile : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _objects;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        int randomObjectIndex = Random.Range(0, _objects.Count);
        Transform tileTransform = transform;
        GameObject instantiate = Instantiate(_objects[randomObjectIndex], tileTransform.position, quaternion.identity, tileTransform);
        instantiate.name = "Object: " + gameObject.name;
    }
}
