//Author: Tamer Erdoğan

using System;
using UnityEngine;

namespace DB.Auth
{
    public abstract class AbstractAuthProvider : MonoBehaviour
    {
        public abstract void Init();

        public abstract string GetCurrentUserId();

        public abstract void Login(
            string email,
            string password,
            Action onLoginSuccess,
            Action<string> onLoginFailed
        );

        public abstract void Register(
            string email,
            string password,
            Action<string, string> onRegisterSuccess,
            Action<string> onRegisterFailed
        );
    }
}
