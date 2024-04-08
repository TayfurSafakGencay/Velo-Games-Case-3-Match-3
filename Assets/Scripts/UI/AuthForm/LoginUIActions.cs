//Author: Tamer Erdoğan

using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUIActions : AuthUIActions
{
    [SerializeField]
    private string _levelSelectSceneName = "Level Select";

    public void LoginAction()
    {
        CloseError();

        string email = emailInput.text.Trim();
        if (!ValidationHelper.IsValidLength(email))
        {
            SetError("E-posta 2 ile 255 karakter arasında olmalıdır.");
            return;
        }

        if (!ValidationHelper.IsValidEmail(email))
        {
            SetError("E-posta formatı yanlış.");
            return;
        }

        string password = passwordInput.text.Trim();
        if (!ValidationHelper.IsValidLength(password))
        {
            SetError("Şifre 2 ile 255 karakter arasında olmalıdır.");
            return;
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;

        AuthManager.Instance.Login(
            email,
            password,
            () =>
            {
                SceneManager.LoadScene(_levelSelectSceneName);
                canvasGroup.interactable = true;
            },
            (errorMessage) =>
            {
                SetError(errorMessage);
                canvasGroup.interactable = true;
            }
        );
    }
}
