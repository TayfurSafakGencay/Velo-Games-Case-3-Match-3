//Author: Tamer Erdoğan

using System;
using System.Collections.Generic;
using DB.Database;
using DB.FirestoreData;
using Firebase.Firestore;
using UnityEngine;

namespace DB.Managers
{
    [RequireComponent(typeof(AbstractDatabaseProvider))]
    public class DatabaseManager : MonoBehaviour
    {
        #region Singleton
        public static DatabaseManager Instance;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            _databaseProvider = GetComponent<AbstractDatabaseProvider>();
            _databaseProvider.Init();
        }
        #endregion Singleton

        private AbstractDatabaseProvider _databaseProvider;

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

        public void CreateUser(
            string id,
            string email,
            Action onSuccess = null,
            Action onFailure = null
        )
        {
            _databaseProvider.CreateUser(id, email, onSuccess, onFailure);
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
            _databaseProvider.GetLeaderboardData(onSuccess, onFailure);
        }

        public void GetCurrentUser(Action<UserFD> onSuccess = null, Action onFailure = null)
        {
            _databaseProvider.GetCurrentUser(
                (DocumentSnapshot userDoc) => onSuccess?.Invoke(userDoc.ConvertTo<UserFD>()),
                onFailure
            );
        }
    }
}
