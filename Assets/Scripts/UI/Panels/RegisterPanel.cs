/// <summary>
/// About panel.
/// Provides functionalty for userregistration panel.
///
/// Created by Jorge L. Chavez Herrera.
/// </summary>
using UnityEngine;
using UnityEngine.UI;


public class RegisterPanel : Panel 
{
    #region Class members
    public InputField userField;
    public InputField emailField;
    public InputField passwordField;
    public InputField confirmPasswordField;
    public GameObject resultPanel;
    public Text resultLabel;
    #endregion

    #region MonoBehaviour overrides
    private void OnEnable()
    {
        HideAlert();
    }

    #endregion

    #region Panel overrides
    #endregion

    #region Class implementation public 
    public void RegisterUser()
    {
        resultPanel.gameObject.SetActive(false);
        MeridianApp.Instance.RegisterUser(MeridianApp.adminUser.Token, "admin", userField.text, emailField.text, passwordField.text, RegisterUser);
    }

    private void RegisterUser(MeridianData.RegisterUserResult registerUserResult)
    {
        if (registerUserResult != null)
        {
            resultLabel.text = registerUserResult.registerUserList[0].Msj;
            resultPanel.gameObject.SetActive(true);

            if (registerUserResult.registerUserList[0].Estatus == 1)
            {
                Invoke("ShowLogin", 2);
            }
        }
    }

    private void ShowLogin()
    {
        Menu.Instance.ShowPanel(0);
    }

    public void HideAlert()
    {
        resultPanel.gameObject.SetActive(false);
    }

    #endregion
}
