using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Arhythmia;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE { get; set; }

    public bool test;
    public bool multiMonitor;
    public bool firstLevelLoaded = false;
    private bool resetting = false;

    public List<GameObject> players = new List<GameObject>();

    private MusicController musicController;
    private CameraManager cameraManager;
    private UIManager uiManager;

    public bool fadeEnded = true;
    public bool musicPlaying = false;

    private bool paused = false;
    private IState lastState;

    public IState currentGameState;
    public IState PLAYINGSTATE = new PlayingState();
    public IState PAUSESTATE = new PauseState();
    public IState MENUSTATE = new MenuState();

    public delegate void BattleBegan();
    public static event BattleBegan OnBattleBegin;
    public delegate void BattleEnded();
    public static event BattleEnded OnBattleEnd;
    

    void Awake()
    {
        if (INSTANCE != null)
        {
            Destroy(gameObject);
        }
        else
        {
            INSTANCE = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += LevelLoaded;
        }

        cameraManager = GetComponent<CameraManager>();
        uiManager = GetComponent<UIManager>();

        if (!test)
            currentGameState = MENUSTATE;
        else
            currentGameState = PLAYINGSTATE;

        AudioListener.pause = false;
        MusicController.OnAudioStart += MusicController_OnAudioStart;
        Shader.WarmupAllShaders();
    }

    private void MusicController_OnAudioStart()
    {
        musicPlaying = true;
    }

    public void ToBattle()
    {
        if (fadeEnded && musicPlaying)
        {
            fadeEnded = false;
            StartCoroutine(musicController.PowerCrossFade());
        }
        else if (!musicPlaying)
            musicController.currentAudioPlaying = MusicController.CurrentAudio.BATTLE;

        if(OnBattleBegin != null)
            OnBattleBegin();
    }
    public void ToOverworld()
    {
        if (fadeEnded && musicPlaying)
        {
            fadeEnded = false;
            StartCoroutine(musicController.PowerCrossFade());
        }
        else if (!musicPlaying && currentGameState == PLAYINGSTATE)
            musicController.currentAudioPlaying = MusicController.CurrentAudio.OVERWORLD;

        if (OnBattleEnd != null)
            OnBattleEnd();
    }

    public void Pause()
    {
        paused = !paused;

        if (!paused)
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            currentGameState = lastState;
        }
        else
        {
            Time.timeScale = 0;
            AudioListener.pause = true;
            lastState = currentGameState;
            currentGameState = PAUSESTATE;
        }

        musicController.Pause();
        uiManager.TooglePauseMenu();
    }

    public void Dead()
    {
        //mc.Dead();
    }

    public void StartGame(int[,] selectedCharacters, int levelSelected)
    {
        players.AddRange(InitializePlayers(selectedCharacters));
        BattleManager.INSTANCE.SetNumberOfLists(players.Count);
        SceneManager.LoadScene(levelSelected, LoadSceneMode.Single);
    }
    public void RestartLevel()
    {
        resetting = true;
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentLevel, LoadSceneMode.Single);
    }
    public void ReturnToMainMenu()
    {
        ResetValuesOfManagers();
        SceneManager.LoadScene(0);
    }
    private void LevelLoaded(Scene level, LoadSceneMode loadMode)
    {
        if (level.buildIndex != 0)
        {
            if (!firstLevelLoaded)
            {
                InitializeHUD();
                firstLevelLoaded = true;
            }

            LevelLoader load = FindObjectOfType<LevelLoader>();
            load.EnemySpawn(level.buildIndex);
            load.PlayerSpawn(players);

            musicController = FindObjectOfType<MusicController>();
            cameraManager.SetCameras(resetting);
            uiManager.SetMenuButtons(resetting);

            foreach (PatternSynch ps in FindObjectsOfType<PatternSynch>())
                ps.SetObjectsReferences();

            FindObjectOfType<BeatSynch>().SetupBeatTime();
            musicController.SetCharactersBeatDependencies();

            if (resetting)
            {
                foreach (PatternSynch p in FindObjectsOfType<PatternSynch>())
                    p.Reset();
                Pause();
                resetting = false;
            }
            else
            {
                currentGameState = PLAYINGSTATE;
            }
        }
        else
            foreach (CanvasConfig cc in FindObjectsOfType<CanvasConfig>())
                Destroy(cc.gameObject);
    }

    private GameObject[] InitializePlayers(int[,] sc)
    {
        List<GameObject> objs = new List<GameObject>();
        Player p = null;
        for (int i = 0; i < sc.GetLength(0); i++)
        {
            switch (sc[i, 0])
            {
                case -1:
                    break;
                case 0:
                    objs.Add((GameObject)Instantiate(Resources.Load("Characters/Players/Warrior")));
                    goto case 10;
                case 1:
                    objs.Add((GameObject)Instantiate(Resources.Load("Characters/Players/Pierrette")));
                    goto case 10;
                case 2:
                    objs.Add((GameObject)Instantiate(Resources.Load("Characters/Players/JPop")));
                    goto case 10;
                case 10:
                    objs[i].name = objs[i].name.Replace("(Clone)", "");
                    p = objs[i].GetComponent<Player>();
                    p.SetupController(i + 1);
                    InitializeSynchronizers(p, sc[i, 1]);
                    break;
            }
        }

        return objs.ToArray();
    }

    private void InitializeSynchronizers(Player p, int ps)
    {
        PatternSynch patSyn = null;
        GameObject o = null;
        switch (ps)
        {
            case 0:
                o = (GameObject)Instantiate(Resources.Load("General/Synchronizers/MelodySynch"));
                break;
            case 1:
                o = (GameObject)Instantiate(Resources.Load("General/Synchronizers/ArmonizerSynch"));
                break;
            case 2:
                o = (GameObject)Instantiate(Resources.Load("General/Synchronizers/BassSynch"));
                break;
            case 3:
                o = (GameObject)Instantiate(Resources.Load("General/Synchronizers/PercussionSynch"));
                break;
        }
        o.name = o.name.Replace("(Clone)", "");
        patSyn = o.GetComponent<PatternSynch>();
        patSyn.LineSync = (PatternSynch.LineSynch)ps;
        p.PatternSynch = patSyn;
    }

    private void InitializeHUD()
    {
        GameObject canv = (GameObject)Instantiate(Resources.Load("General/Canvas/Canvas"));
        canv.name = canv.name.Replace("(Clone)", "");
        canv.GetComponent<CanvasConfig>().DivideCanvas();

        for (int i = 0; i < players.Count; i++)
        {
            //Get player joystick number
            int p = players[i].GetComponent<Player>().PlayerNumber;

            //Instantiate HUD Holder
            GameObject hud = new GameObject("HUDPlayer" + p, typeof(RectTransform));
            hud.transform.SetParent(canv.transform);
            hud.transform.localScale = Vector3.one;

            //HP, SHIELD and HIT image
            GameObject hp           = (GameObject)Instantiate(Resources.Load("General/Canvas/HP"), hud.transform, false); hp.name = hp.name.Replace("(Clone)", "");
            GameObject shield       = (GameObject)Instantiate(Resources.Load("General/Canvas/Shield"), hud.transform, false); shield.name = shield.name.Replace("(Clone)", "");
            GameObject hit          = (GameObject)Instantiate(Resources.Load("General/Canvas/hitImage"), hud.transform, false); hit.name = hit.name.Replace("(Clone)", "");

            //Config Canvas
            canv.GetComponent<CanvasConfig>().SetupCanvasSize(hud.GetComponent<RectTransform>(), p);

            GameObject go = new GameObject(("SkillzP" + p), typeof(RectTransform));
            go.transform.SetParent(GameObject.Find("HUDPlayer" + p).transform, false);
            go.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0); go.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            go.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

            switch (players[i].gameObject.name)
            {
                case "Pierrette":
                    Instantiate(Resources.Load("General/Canvas/PierretteSkillHolder"), GameObject.Find("SkillzP" + p).transform, false);
                    break;
                case "Warrior":
                    Instantiate(Resources.Load("General/Canvas/MagicianSkillHolder"), GameObject.Find("SkillzP" + p).transform, false);
                    break;
                case "JPop":
                    Instantiate(Resources.Load("General/Canvas/JPopSkillHolder"), GameObject.Find("SkillzP" + p).transform, false);
                    break;
            }

            //Instantiate camera object to target and set it as child of the player
            Instantiate(Resources.Load("Characters/Players/Cameras/CamTarget/CamP" + p), players[i].transform.position + new Vector3(1f, 1.3f, 0), Quaternion.identity, players[i].transform);
            //Instantiate camera
            GameObject camH = (GameObject)Instantiate(Resources.Load("Characters/Players/Cameras/CamHolderP" + p));
            camH.name = camH.name.Replace("(Clone)", "");
            camH.GetComponent<CameraMovement>().SetHUDObjects(hud.transform, p);

            System.GC.Collect();
        }
    }

    private void ResetValuesOfManagers()
    {
        foreach (GameObject obj in players)
            Destroy(obj);
        players.Clear();
        firstLevelLoaded = false;
        musicPlaying = false;
        musicController = null;

        //Soul Pooler Reset
        Destroy(GameObject.Find("SoulHolder"));
        //Managers Reset
        uiManager.ToMainMenu();
        cameraManager.ToMainMenu();
        BattleManager.INSTANCE.ToMainMenu();
        //Synchronizers Reset
        foreach (PatternSynch p in FindObjectsOfType<PatternSynch>())
            Destroy(p.gameObject);
        ////Canvas Reset


        paused = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
        currentGameState = MENUSTATE;
        lastState = null;
    }
}
