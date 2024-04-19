//Author: Tamer Erdoğan

using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public abstract class AbstractDatabaseProvider : MonoBehaviour
{
    public abstract void Init();

    public abstract void CreateUser(
        string id,
        string email,
        Action onSuccess = null,
        Action onFailure = null
    );

    public abstract void GetCurrentUser(
        Action<DocumentSnapshot> onSuccess = null,
        Action onFailure = null
    );

    public abstract void UpdateUserFields(
        int level,
        int score,
        Action onSuccess = null,
        Action onFailure = null
    );

    public abstract void GetLeaderboarData(
        Action<IEnumerable<DocumentSnapshot>> onSuccess = null,
        Action onFailure = null
    );
}
