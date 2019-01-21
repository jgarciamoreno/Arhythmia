using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    public static UIManager INSTANCE;

    private int menuIndex = 0;
    public List<Button> menuButtons = new List<Button>();

    public GameObject pausePanel;

    GameManager gameManager;
    //MusicController musicController;

    private void Awake()
    {
        if (INSTANCE != null)
            Destroy(this);
        else
            INSTANCE = this;

        gameManager = GetComponent<GameManager>();
    }

    public void Submit(Character c)
    {
        switch (menuIndex)
        {
            case 0:
                OnResumePressed();
                break;
            case 1:
                OnRestartPressed();
                break;
            case 2:
                OnOptionsPressed();
                break;
            case 3:
                OnExitPressed();
                break;
        }
    }
    public void Cancel(Character c)
    {

    }
    public void NavigateOptions(Character c)
    {
        menuButtons[menuIndex].interactable = false;
        
        menuIndex += c.inputHandler.controller.MenuLeftJoystick();
        menuIndex = menuIndex < 0 ? 3 : menuIndex;
        menuIndex = menuIndex > 3 ? 0 : menuIndex;

        menuButtons[menuIndex].interactable = true;
    }

    public void TooglePauseMenu()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    private void OnResumePressed()
    {
        gameManager.Pause();
    }
    private void OnRestartPressed()
    {
        gameManager.RestartLevel();
    }
    private void OnOptionsPressed()
    {

    }
    private void OnExitPressed()
    {
        gameManager.ReturnToMainMenu();
    }

    public void SetMenuButtons(bool resetting)
    {
        if (resetting)
            menuButtons.Clear();

        menuButtons.Add(GameObject.Find("Resume_Button").GetComponent<Button>());
        menuButtons.Add(GameObject.Find("Restart_Button").GetComponent<Button>());
        menuButtons.Add(GameObject.Find("Options_Button").GetComponent<Button>());
        menuButtons.Add(GameObject.Find("Exit_Button").GetComponent<Button>());
        pausePanel = menuButtons[0].transform.parent.gameObject;
        pausePanel.transform.SetAsLastSibling();
        pausePanel.SetActive(resetting);
    }

    public void ToMainMenu()
    {
        pausePanel = null;
        menuButtons.Clear();
        menuIndex = 0;
    }
}
