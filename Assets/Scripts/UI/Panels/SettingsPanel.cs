﻿/// <summary>
/// About panel.
/// Provides functionalty for adjusting app settings.
///
/// Created by Jorge L. Chavez Herrera.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class SettingsPanel : Panel 
{
    #region Class members
    public Text nameLabel;
    public Text emailLabel;
    #endregion

    #region MonoBehaviour overrides
    private void OnEnable()
    {
        if (MeridianApp.currentUser != null)
        {
            nameLabel.text = MeridianApp.currentUser.UserName;
            emailLabel.text = Menu.Instance.panels[0].GetComponent<LoginPanel>().userField.text;

        }
    }
    #endregion

    #region Panel overrides
    #endregion

    #region Class implementation
    public void Logout()
    {
        MeridianApp.SetCurrentUser(null);
        nameLabel.text = "";
        emailLabel.text = Menu.Instance.panels[0].GetComponent<LoginPanel>().userField.text = "";
    }

	#endregion
}
