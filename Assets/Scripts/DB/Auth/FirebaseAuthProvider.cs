//Author: Tamer Erdoğan

using System;
using System.Collections;
using Firebase.Auth;
using UnityEngine;

namespace DB.Auth
{
    public class FirebaseAuthProvider : AbstractAuthProvider
    {
        private FirebaseAuth _auth;

        public override void Init()
        {
            _auth = FirebaseAuth.DefaultInstance;
        }

        public override string GetCurrentUserId()
        {
            return _auth.CurrentUser.UserId;
        }

        public override void Login(
            string email,
            string password,
            Action onLoginSuccess = null,
            Action<string> onLoginFailed = null
        )
        {
            StartCoroutine(LoginRequest(email, password, onLoginSuccess, onLoginFailed));
        }

        IEnumerator LoginRequest(
            string email,
            string password,
            Action onLoginSuccess = null,
            Action<string> onLoginFailed = null
        )
        {
            var loginRequestTask = _auth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => loginRequestTask.IsCompleted);

            if (loginRequestTask.Exception == null)
                onLoginSuccess?.Invoke();
            else
                onLoginFailed?.Invoke("An error occurred while login.");
        }

        public override void Register(
            string email,
            string password,
            Action<string, string> onRegisterSuccess = null,
            Action<string> onRegisterFailed = null
        )
        {
            StartCoroutine(RegisterRequest(email, password, onRegisterSuccess, onRegisterFailed));
        }

        IEnumerator RegisterRequest(
            string email,
            string password,
            Action<string, string> onRegisterSuccess = null,
            Action<string> onRegisterFailed = null
        )
        {
            var registerRequestTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerRequestTask.IsCompleted);

            if (registerRequestTask.Exception == null)
                onRegisterSuccess?.Invoke(
                    registerRequestTask.Result.User.UserId,
                    registerRequestTask.Result.User.Email
                );
            else
                onRegisterFailed?.Invoke(registerRequestTask.Exception.InnerException.Message);
        }
    }
}
