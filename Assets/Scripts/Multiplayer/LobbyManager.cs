using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public class PlayerData
    {
        public string name;
        public bool ready;
        public string color;
        public string id;

        public PlayerData(string name, bool ready, string color, string id)
        {
            this.name = name;
            this.ready = ready;
            this.color = color;
            this.id = id;
        }
    }

    private Lobby curLobby;
    private MainMenu _mainMenu;

    private float _timer;

    private void Start()
    {
        _mainMenu = GetComponent<MainMenu>();
    }

    public async void Authenticate(string playerName)
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            AuthenticationService.Instance.SwitchProfile(playerName);
            return;
        }

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyPollForUpdates();
    }

    public bool isHost()
    {
        return curLobby != null && AuthenticationService.Instance.PlayerId == curLobby.HostId;
    }

    void UpdatePlayersList()
    {
        PlayerData[] list = new PlayerData[8];
        int i = 0;
        foreach (var player in curLobby.Players)
        {
            list[i] = new PlayerData(player.Data["PlayerName"].Value, player.Data["PlayerReady"].Value == "true",
                player.Data["PlayerColor"].Value, player.Id);
            i++;
        }

        for (; i < 8; i++)
        {
            list[i] = new PlayerData("", false, "", "");
        }

        _mainMenu.UpdatePlayersList(list);
    }

    public async void CreateLobby(string name, int numberOfPlayers, int numberOfLaps, int track, string customTrack)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>()
                {
                    {"StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0")},
                    {"Track", new DataObject(DataObject.VisibilityOptions.Member, track.ToString())},
                    {"CustomTrack", new DataObject(DataObject.VisibilityOptions.Member, customTrack)},
                    {"Laps", new DataObject(DataObject.VisibilityOptions.Member, numberOfLaps.ToString())},
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(name, numberOfPlayers, createLobbyOptions);
            curLobby = lobby;
            _mainMenu.SetLobby(curLobby.LobbyCode, curLobby.Name, curLobby.MaxPlayers,
                int.Parse(curLobby.Data["Laps"].Value), int.Parse(curLobby.Data["Track"].Value), curLobby.Data["CustomTrack"].Value);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string code)
    {
        if (code == "")
        {
            return;
        }

        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            Debug.Log("Joined lobby by code " + code);
            curLobby = lobby;
            _mainMenu.SetLobby(curLobby.LobbyCode, curLobby.Name, curLobby.MaxPlayers,
                int.Parse(curLobby.Data["Laps"].Value), int.Parse(curLobby.Data["Track"].Value), curLobby.Data["CustomTrack"].Value);
            _mainMenu.JoinedSuccess();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(curLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        curLobby = null;
    }

    public async void Kick(string id)
    {
        if (id == curLobby.HostId)
        {
            return;
        }

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(curLobby.Id, id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerReady()
    {
        await LobbyService.Instance.UpdatePlayerAsync(curLobby.Id, AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerReady",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
                            _mainMenu.ready ? "true" : "false")
                    }
                }
            });
    }

    async void HandleLobbyPollForUpdates()
    {
        if (curLobby != null)
        {
            _timer -= Time.deltaTime;
            if (_timer < 0f)
            {
                try
                {
                    _timer = 1.1f;
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(curLobby.Id);
                    curLobby = lobby;
                    UpdatePlayersList();
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                    _mainMenu.OnLeaveClick();
                    curLobby = null;
                }

                if (curLobby.Data["StartGame"].Value != "0")
                {
                    if (!isHost())
                    {
                        GetComponent<RelayManager>().JoinRelay(curLobby.Data["StartGame"].Value);
                    }

                    curLobby = null;
                }
            }
        }
    }


    Player GetPlayer()
    {
        return new()
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _mainMenu.playerName)},
                {
                    "PlayerReady",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _mainMenu.ready ? "true" : "false")
                },
                {"PlayerColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _mainMenu.playerColor)},
            }
        };
    }

    public async void StartGame()
    {
        if (isHost())
        {
            try
            {
                string relayCode = await GetComponent<RelayManager>().CreateRelay(curLobby.MaxPlayers);

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(curLobby.Id, new UpdateLobbyOptions()
                {
                    Data = new Dictionary<string, DataObject>()
                    {
                        {"StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });
                curLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public void ClearInfo()
    {
        curLobby = null;
        _timer = 0;
    }
}