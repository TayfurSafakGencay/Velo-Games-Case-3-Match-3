//Author: Tamer Erdoğan

using System;
using UnityEngine;

[RequireComponent(typeof(AbstractAuthProvider))]
public class AuthManager : MonoBehaviour
{
    #region Singleton
    public static AuthManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _authProvider = GetComponent<AbstractAuthProvider>();
        _authProvider.Init();
    }
    #endregion Singleton

    private AbstractAuthProvider _authProvider;

    public string GetCurrentUserId()
    {
        return _authProvider.GetCurrentUserId();
    }

    public void Login(
        string email,
        string password,
        Action onLoginSuccess = null,
        Action<string> onLoginFailed = null
    )
    {
        _authProvider.Login(email, password, onLoginSuccess, onLoginFailed);
    }

    public void Register(
        string email,
        string password,
        Action<string> onRegisterSuccess = null,
        Action<string> onRegisterFailed = null
    )
    {
        _authProvider.Register(email, password, onRegisterSuccess, onRegisterFailed);
    }
}
