//Author: Tamer Erdoğan

using UnityEngine;

public class MenuUIActions : MonoBehaviour
{
    [SerializeField]
    private GameObject _loginForm;

    [SerializeField]
    private GameObject _registerForm;

    [SerializeField]
    private GameObject _menuContainer;

    public void OpenLoginForm()
    {
        CloseAllForms();
        _loginForm.SetActive(true);
    }

    public void OpenRegisterForm()
    {
        CloseAllForms();
        _registerForm.SetActive(true);
    }

    public void BackToMenu()
    {
        CloseAllForms();
        _menuContainer.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void CloseAllForms()
    {
        _loginForm.SetActive(false);
        _loginForm.GetComponent<AuthUIActions>()?.ClearAction();
        _registerForm.SetActive(false);
        _registerForm.GetComponent<AuthUIActions>()?.ClearAction();
        _menuContainer.SetActive(false);
    }
}
