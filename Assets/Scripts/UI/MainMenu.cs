using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainMenu : MonoBehaviour
{
    private LobbyManager _lobbyManager;

    [HideInInspector] public LobbyManager.PlayerData[] AllPlayerData;

    [HideInInspector] public int playersAmount = 2;
    [HideInInspector] public int lapsAmount = 1;

    [HideInInspector] public string playerName = "";
    [HideInInspector] public string playerColor = "0";
    [HideInInspector] public bool ready = false;
    [HideInInspector] public int chosenColor = 0;
    public int chosenTrack = 0;
    public string customTrack = "";

    [SerializeField] private GameObject joystickObject;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Image trackImage, lobbyTrackImage;

    [SerializeField] private Button[] nonInteractableButtons;

    [SerializeField] private GameObject mainPanel,
        createLobbyPanel,
        inLobbyPanel,
        customizePanel,
        menuCamera,
        baseCamera,
        inGamePanel,
        statsPanel;

    [SerializeField] private InputField lobbyName, codeName, playerName_field;

    [SerializeField]
    private Text code, playersAmountText, lapsAmountText, lobbyTextPlayers, lobbyTextLaps, lobbyTextName;

    [SerializeField] private GameObject[] playersText;

    [SerializeField] public Color[] colors;
    [SerializeField] private Sprite[] tracks;

    [SerializeField] private Sprite readySprite, notReadySprite;
    [SerializeField] private Button playButton, createLobbyButton;

    [SerializeField] private Text skilledText, unskilledText, fastestText, slowestText, winnerName, winnerPoints;

    private AudioManager _audioManager;
    [SerializeField] private Image soundButton;
    [SerializeField] private Sprite mute, unmute;

    void Start()
    {
        _audioManager = FindObjectOfType<AudioManager>();
        _audioManager.Play("Theme");
        _audioManager.Play("Waves");
        if (!PlayerPrefs.HasKey("PreferredName") && !PlayerPrefs.HasKey("PreferredColor"))
        {
            foreach (Button b in nonInteractableButtons)
            {
                b.interactable = false;
            }
        }

        _lobbyManager = GetComponent<LobbyManager>();
    }

    public void OnSoundClick()
    {
        foreach (var s in FindObjectsOfType<AudioSource>())
        {
            s.mute = !s.mute;
        }

        soundButton.sprite = soundButton.sprite.Equals(mute) ? unmute : mute;
    }

    public void OnCreateClick()
    {
        _audioManager.Play("Button");
        playersAmount = 2;
        mainPanel.SetActive(false);
        createLobbyPanel.SetActive(true);
    }

    public void OnJoinClick()
    {
        _audioManager.Play("Button");
        _lobbyManager.JoinLobby(codeName.text);
    }

    public void JoinedSuccess()
    {
        mainPanel.SetActive(false);
        inLobbyPanel.SetActive(true);
        playButton.gameObject.SetActive(false);
    }

    public void OnCustomizeClick()
    {
        _audioManager.Play("Button");
        mainPanel.SetActive(false);
        meshRenderer.gameObject.SetActive(true);
        customizePanel.SetActive(true);
    }

    public void OnLeaveClick()
    {
        _audioManager.Play("Button");
        ready = false;
        inLobbyPanel.SetActive(false);
        mainPanel.SetActive(true);
        _lobbyManager.LeaveLobby();
    }

    public void OnReadyClick()
    {
        _audioManager.Play("Button");
        ready = !ready;
        _lobbyManager.UpdatePlayerReady();
    }

    public void OnCreateLobbyClick()
    {
        _audioManager.Play("Button");
        if (lobbyName.text == "")
        {
            FindObjectOfType<ErrorMessage>().Error("Lobby name is empty");
            return;
        }

        createLobbyPanel.SetActive(false);
        inLobbyPanel.SetActive(true);
        playButton.gameObject.SetActive(true);
        _lobbyManager.CreateLobby(lobbyName.text, playersAmount, lapsAmount, chosenTrack, customTrack);
    }

    public void OnSaveClick()
    {
        _audioManager.Play("Button");
        playerName = playerName_field.text;
        if (playerName.Length == 0)
        {
            playerName = "";
            FindObjectOfType<ErrorMessage>().Error("Player name should be entered");
            return;
        }

        playerColor = chosenColor.ToString();

        foreach (Button b in nonInteractableButtons)
        {
            b.interactable = true;
        }

        meshRenderer.gameObject.SetActive(false);
        customizePanel.SetActive(false);
        mainPanel.SetActive(true);
        _lobbyManager.Authenticate(playerName);
    }

    public void OnBack(string panel)
    {
        _audioManager.Play("Button");
        switch (panel)
        {
            case "customize":
                customizePanel.SetActive(false);
                meshRenderer.gameObject.SetActive(false);
                break;
            case "create":
                createLobbyPanel.SetActive(false);
                break;
        }

        mainPanel.SetActive(true);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    public void OnModel(bool left)
    {
        _audioManager.Play("Spray");
        if (left)
        {
            chosenColor = chosenColor == 0 ? 7 : chosenColor - 1;
            meshRenderer.materials[1].color = colors[chosenColor];
        }
        else
        {
            chosenColor = chosenColor == 7 ? 0 : chosenColor + 1;
            meshRenderer.materials[1].color = colors[chosenColor];
        }
    }

    public void OnMap(bool left)
    {
        _audioManager.Play("Spray");
        if (left)
        {
            chosenTrack = chosenTrack == 0 ? 3 : chosenTrack - 1;
            trackImage.sprite = tracks[chosenTrack];
        }
        else
        {
            chosenTrack = chosenTrack == 3 ? 0 : chosenTrack + 1;
            trackImage.sprite = tracks[chosenTrack];
        }

        if (chosenTrack == 3)
        {
            try
            {
                string clipBoard = GUIUtility.systemCopyBuffer;
                byte[] decodedBytes = Convert.FromBase64String(clipBoard);
                string decodedText = Encoding.UTF8.GetString(decodedBytes);
                if (!Regex.IsMatch(decodedText,
                    "((Straight|Corner)~(False|True)~(False|True)~(False|True)~(-?\\d+)~(-?\\d+)~(-?\\d+)~(False|True)\\*)+"))
                {
                    FindObjectOfType<ErrorMessage>().Error("No map code in clipboard found");
                    createLobbyButton.interactable = false;
                    customTrack = "";
                }
                else
                {
                    customTrack = clipBoard;
                }
            }
            catch (FormatException)
            {
                FindObjectOfType<ErrorMessage>().Error("No map code in clipboard found");
                createLobbyButton.interactable = false;
                customTrack = "";
            }
        }
        else
        {
            createLobbyButton.interactable = true;
        }
    }

    public void OnPlayersAmount(bool up)
    {
        _audioManager.Play("Button");
        playersAmount = Mathf.Clamp(playersAmount + (up ? 1 : -1), 1, 8);
        playersAmountText.text = playersAmount + " players";
    }

    public void OnLapsAmount(bool up)
    {
        _audioManager.Play("Button");
        lapsAmount = Mathf.Clamp(lapsAmount + (up ? 1 : -1), 1, 6);
        lapsAmountText.text = lapsAmount + " laps";
    }

    public void OnPlayClick()
    {
        _audioManager.Play("Button");
        _lobbyManager.StartGame();
    }

    public void OnMenuClick()
    {
        _audioManager.Play("Button");
        statsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void UpdatePlayersList(LobbyManager.PlayerData[] players)
    {
        AllPlayerData = players;
        int amountReady = 0;
        int i = 0;
        foreach (var player in AllPlayerData)
        {
            if (player.name.Equals(""))
            {
                playersText[i].SetActive(false);
                i++;
                continue;
            }

            playersText[i].SetActive(true);

            playersText[i].GetComponentInChildren<Text>().text = player.name;
            Image[] images = playersText[i].GetComponentsInChildren<Image>();

            images[1].color = colors[int.Parse(player.color)];

            if (player.ready)
            {
                images[2].sprite = readySprite;
                amountReady++;
            }
            else
            {
                images[2].sprite = notReadySprite;
            }

            Button b = playersText[i].GetComponentInChildren<Button>();
            if (_lobbyManager.isHost())
            {
                b.interactable = true;
                b.onClick.AddListener(delegate { _lobbyManager.Kick(player.id); });
            }
            else
            {
                b.interactable = false;
            }

            i++;
        }

        if (amountReady == playersAmount)
        {
            playButton.interactable = true;
        }
        else
        {
            playButton.interactable = false;
        }
    }

    public void SetLobby(string codeStr, string name, int maxPlayers, int laps, int track, string custom)
    {
        code.text = "CODE: " + codeStr;
        lobbyTextPlayers.text = maxPlayers + " players";
        lobbyTextLaps.text = laps + " laps";
        lobbyTextName.text = name;
        chosenTrack = track;
        customTrack = custom;
        lapsAmount = laps;
        lobbyTrackImage.sprite = tracks[track];
    }

    public void HideMenu()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            joystickObject.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        foreach (GameObject panel in new[]
            {mainPanel, createLobbyPanel, inLobbyPanel, customizePanel, menuCamera, meshRenderer.gameObject})
        {
            panel.SetActive(false);
        }

        baseCamera.SetActive(true);
        inGamePanel.SetActive(true);
    }

    public void ClearInfoAndShowStats(List<GameControl.PlayerData> list)
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            joystickObject.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (list.Count <= 0)
        {
            FindObjectOfType<ErrorMessage>().Error("No player data found");
            return;
        }

        playersAmount = 2;
        lapsAmount = 1;
        ready = false;
        chosenTrack = 0;
        lobbyName.text = "";
        codeName.text = "";
        playersAmountText.text = playersAmount + " players";
        lapsAmountText.text = lapsAmount + " laps";
        trackImage.sprite = tracks[chosenTrack];
        lobbyTrackImage.sprite = tracks[chosenTrack];

        foreach (var p in playersText)
        {
            p.SetActive(false);
        }

        baseCamera.SetActive(false);
        inGamePanel.SetActive(false);

        menuCamera.SetActive(true);
        statsPanel.SetActive(true);

        winnerName.text = list[0].name;
        winnerPoints.text = list[0].score + " points";
        list.Sort((x, y) =>
            x.skillScore.CompareTo(y.skillScore));
        skilledText.text = list[list.Count - 1].name + " as the most skilled driver earned " +
                           list[list.Count - 1].score + " points.";
        unskilledText.text = list[0].name + " as the least skilled driver earned " + list[0].score + " points.";
        ;
        list.Sort((x, y) =>
            x.speedScore.CompareTo(y.speedScore));
        fastestText.text = list[list.Count - 1].name + " as the fastest driver earned " + list[list.Count - 1].score +
                           " points.";
        ;
        slowestText.text = list[0].name + " as the slowest driver earned " + list[0].score + " points.";
        ;
    }
}