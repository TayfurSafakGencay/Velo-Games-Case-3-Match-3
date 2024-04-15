//Author: Tamer Erdoğan

using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[RequireComponent(typeof(AbstractDatabaseProvider))]
public class DatabaseManager : MonoBehaviour
{
    #region Singleton
    public static DatabaseManager Instance;

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion Singleton

    private AbstractDatabaseProvider _databaseProvider;

    void Start()
    {
        _databaseProvider = GetComponent<AbstractDatabaseProvider>();
        _databaseProvider.Init();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateUserFields(12, 13245, () => Debug.Log("Update fields OK!"));
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetLeaderboardData(
                (documents) =>
                {
                    foreach (var item in documents)
                    {
                        Debug.Log(item.GetValue<int>("score"));
                    }
                }
            );
        }
    }
#endif

    public void CreateUser(string id, Action onSuccess = null, Action onFailure = null)
    {
        _databaseProvider.CreateUser(id, onSuccess, onFailure);
    }

    public void UpdateUserFields(
        int level,
        int score,
        Action onSuccess = null,
        Action onFailure = null
    )
    {
        _databaseProvider.UpdateUserFields(level, score, onSuccess, onFailure);
    }

    public void GetLeaderboardData(
        Action<IEnumerable<DocumentSnapshot>> onSuccess = null,
        Action onFailure = null
    )
    {
        _databaseProvider.GetLeaderboarData(onSuccess, onFailure);
    }
}
