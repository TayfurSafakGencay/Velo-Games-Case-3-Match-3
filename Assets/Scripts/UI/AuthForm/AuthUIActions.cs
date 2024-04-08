//Author: Tamer Erdoğan

using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AuthUIActions : MonoBehaviour
{
    [SerializeField]
    protected TMP_InputField emailInput;

    [SerializeField]
    protected TMP_InputField passwordInput;

    [SerializeField]
    protected TMP_Text errorMessageText;

    public void ClearAction()
    {
        emailInput.text = "";
        passwordInput.text = "";
        CloseError();
    }

    protected void SetError(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
    }

    protected void CloseError()
    {
        errorMessageText.text = "";
        errorMessageText.gameObject.SetActive(false);
    }
}
