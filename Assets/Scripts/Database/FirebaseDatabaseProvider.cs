//Author: Tamer Erdoğan

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseDatabaseProvider : AbstractDatabaseProvider
{
    private FirebaseFirestore _firestore;

    public override void Init()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
    }

    public override void CreateUser(
        string id,
        string email,
        Action onSuccess = null,
        Action onFailure = null
    )
    {
        UserFD userFD = new UserFD { id = id, email = email };
        StartCoroutine(CreateUserRequest(userFD, onSuccess, onFailure));
    }

    IEnumerator CreateUserRequest(UserFD userFD, Action onSuccess = null, Action onFailure = null)
    {
        var createUserRequestTask = _firestore.Collection("/users").AddAsync(userFD);
        yield return new WaitUntil(() => createUserRequestTask.IsCompleted);

        if (createUserRequestTask.Exception == null)
            onSuccess?.Invoke();
        else
            onFailure?.Invoke();
    }

    public override void GetCurrentUser(
        Action<DocumentSnapshot> onSuccess = null,
        Action onFailure = null
    )
    {
        string userId = AuthManager.Instance.GetCurrentUserId();
        StartCoroutine(GetUserRequest(userId, onSuccess, onFailure));
    }

    IEnumerator GetUserRequest(
        string userId,
        Action<DocumentSnapshot> onSuccess = null,
        Action onFailure = null
    )
    {
        var getUserRequestTask = _firestore
            .Collection("/users")
            .WhereEqualTo("id", userId)
            .GetSnapshotAsync();

        yield return new WaitUntil(() => getUserRequestTask.IsCompleted);

        if (getUserRequestTask.Exception == null)
            onSuccess?.Invoke(getUserRequestTask.Result.Documents.First());
        else
            onFailure?.Invoke();
    }

    public override void UpdateUserFields(
        int level,
        int score,
        Action onSuccess = null,
        Action onFailure = null
    )
    {
        IDictionary<string, object> fields = new Dictionary<string, object>
        {
            { "score", score },
            { "level", level }
        };

        GetCurrentUser(
            (userDoc) =>
                StartCoroutine(UpdateUserFieldsRequest(userDoc, fields, onSuccess, onFailure))
        );
    }

    IEnumerator UpdateUserFieldsRequest(
        DocumentSnapshot userDoc,
        IDictionary<string, object> fields,
        Action onSuccess = null,
        Action onFailure = null
    )
    {
        var updateUserFieldsRequestTask = userDoc.Reference.UpdateAsync(fields);

        yield return new WaitUntil(() => updateUserFieldsRequestTask.IsCompleted);

        if (updateUserFieldsRequestTask.Exception == null)
            onSuccess?.Invoke();
        else
            onFailure?.Invoke();
    }

    public override void GetLeaderboarData(
        Action<IEnumerable<DocumentSnapshot>> onSuccess = null,
        Action onFailure = null
    )
    {
        StartCoroutine(GetLeaderboarDataRequest(onSuccess, onFailure));
    }

    IEnumerator GetLeaderboarDataRequest(
        Action<IEnumerable<DocumentSnapshot>> onSuccess = null,
        Action onFailure = null
    )
    {
        var getLeaderboarDataRequestTask = _firestore
            .Collection("/users")
            .OrderByDescending("score")
            .Limit(10)
            .GetSnapshotAsync();

        yield return new WaitUntil(() => getLeaderboarDataRequestTask.IsCompleted);

        if (getLeaderboarDataRequestTask.Exception == null)
            onSuccess?.Invoke(getLeaderboarDataRequestTask.Result.Documents);
        else
            onFailure?.Invoke();
    }
}
