/// <summary>
/// About panel.
/// Provides functionalty for showing the about panel.
///
/// Created by Jorge L. Chavez Herrera.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class LoginPanel : Panel 
{
    #region Class members
    public InputField userField;
    public InputField passwordField;
    public GameObject resultPanel;
    public Text resultLabel;
    public Button continueButton;
    #endregion

    #region MonoBehaviour overrides 
    private void Awake()
    {
        MeridianApp.adminUserReadyDelegate += AdminUserReadyDelegate;
    }

    private void OnEnable()
    {
        HideAlert();
    }
    #endregion

    #region Panel overrides
    #endregion

    #region Class implementation
    public void UserLogin()
    {
        MeridianApp.Instance.UserLogin(userField.text, passwordField.text, UserLogin);
    }

    private void UserLogin(MeridianData.UserLoginResult loginResult)
    {
        if (loginResult != null)
        {
            if (loginResult.userList[0].Estatus == 1)
            {
                EnterDecorator();
            }
            else
            {
                resultLabel.text = "Error: Nombre de usuario o contraseña invalida";
                resultPanel.gameObject.SetActive(true);
            }
        }
        else
        {
            resultLabel.text = "Error: Nombre de usuario o contraseña invalida";
            resultPanel.gameObject.SetActive(true);
        }
    }

    private void AdminUserReadyDelegate()
    {
    }

    public void HideAlert()
    {
        resultPanel.gameObject.SetActive(false);
        continueButton.interactable = (string.IsNullOrEmpty(userField.text) == false && string.IsNullOrEmpty(passwordField.text) == false);
    }

    public void EnterDecorator()
    {
        Menu.Instance.ShowPanel(2);
    }
    #endregion
}
