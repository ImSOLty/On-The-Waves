using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class NetworkControl : NetworkBehaviour
{
    [SerializeField] private Transform mlPrefab;
    [SerializeField] private GameObject objectCollider;
    private GameControl _gameControl;
    [HideInInspector] public Movement objectMovement;
    private PlayerProgress _progress;

    private MainMenu _mainMenu;

    public bool everyoneReady, imReady, imFinished;

    private Transform start;

    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _mainMenu = FindObjectOfType<MainMenu>();
        _gameControl = FindObjectOfType<GameControl>();
        objectMovement = GetComponentInChildren<Movement>();
        if (!IsOwner)
            return;
        StartCoroutine(Wait());
        if (IsOwnedByServer)
        {
            StartCoroutine(WaitForEveryone());
        }
        else
        {
            SetScene();
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitUntil(() => everyoneReady);
        Unfade();
        yield return new WaitForSeconds(1);
        if (IsOwnedByServer)
        {
            CountdownClientRpc(3);
            yield return new WaitForSeconds(1);
            CountdownClientRpc(2);
            yield return new WaitForSeconds(1);
            CountdownClientRpc(1);
            yield return new WaitForSeconds(1);
            GameStartedClientRpc();
            foreach (var c in FindObjectsOfType<BoatAgent>())
            {
                c.enabled = true;
                c.GetComponent<Movement>().acceleration = Random.Range(60, 100);
            }
        }
    }

    IEnumerator WaitForEveryone()
    {
        yield return new WaitUntil(() => NetworkManager.ConnectedClients.Count == _mainMenu.playersAmount);
        SetScene();
        yield return new WaitUntil(() => CountReady() == _mainMenu.playersAmount);
        ReadyClientRpc();
    }

    void Unfade()
    {
        _gameControl.UpdatePrintScores();
        _mainMenu.HideMenu();
        objectMovement.SetCamera();
        GameObject.Find("Fade").GetComponent<Animator>().SetBool("Fade", false);
    }

    void SetScene()
    {
        start = FindObjectOfType<MapGenerator>().GenerateAndReturnStart(_mainMenu.chosenTrack);

        OtherSettings();

        if (IsOwner && IsOwnedByServer)
        {
            float startOffset = 17.5f;
            float additionalOffset = -5;

            for (int j = NetworkManager.ConnectedClients.Count; j < 8; j++)
            {
                Transform ml = Instantiate(mlPrefab);
                ml.transform.position = new Vector3(start.position.x + (startOffset + additionalOffset * j), 0,
                    start.position.z) - 10 * start.forward;
                ml.transform.forward = start.forward;

                ml.GetComponent<NetworkObject>().Spawn(true);
                SetInitialDataClientRpc(Convert.ToUInt64(j), "Bot", 6);

                GameObject MLcollider = ml.Find("Collider").gameObject;
                MLcollider.SetActive(true);

                PlayerProgress mlprogress = MLcollider.gameObject.AddComponent<PlayerProgress>();
                mlprogress.SetInitial(Convert.ToUInt64(j), this, _gameControl, true);
            }
        }

        _progress = objectCollider.AddComponent<PlayerProgress>();
        _progress.SetInitial((NetworkObjectId-1), this, _gameControl, false);

        ImReadyServerRpc((NetworkObjectId-1), _mainMenu.playerName, _mainMenu.chosenColor);
    }

    void OtherSettings()
    {
        float startOffset = 17.5f;
        float additionalOffset = -5;
        transform.forward = start.forward;
        transform.position = new Vector3(start.position.x + (startOffset + additionalOffset * (NetworkObjectId-1)), 0,
            start.position.z) - 10 * start.forward;
        objectCollider.SetActive(true);
    }

    [ClientRpc]
    void ReadyClientRpc()
    {
        foreach (var c in FindObjectsOfType<NetworkControl>())
        {
            c.everyoneReady = true;
        }
    }

    [ClientRpc]
    void GameStartedClientRpc()
    {
        GameObject.Find("Countdown").GetComponent<Animator>().SetBool("On", false);
        foreach (var c in FindObjectsOfType<Movement>())
        {
            c.gameStarted = true;
        }
    }

    [ClientRpc]
    void CountdownClientRpc(int time)
    {
        GameObject.Find("Countdown").GetComponent<Text>().text = time.ToString();
        GameObject.Find("Countdown").GetComponent<Animator>().SetBool("Up", true);
    }

    [ServerRpc]
    void ImReadyServerRpc(ulong id, string username, int color)
    {
        imReady = true;
        SetInitialDataClientRpc(id, username, color);
    }

    [ClientRpc]
    void SetInitialDataClientRpc(ulong id, string username, int color)
    {
        _gameControl.PlayerDatas.Add(new GameControl.PlayerData(username, 0, _mainMenu.colors[color], id));
        if (id == (NetworkObjectId-1))
        {
            objectMovement.GetComponentInChildren<Text>().text = username;
            objectMovement.model.GetComponent<MeshRenderer>().materials[1].color = _mainMenu.colors[color];
        }
    }


    [ServerRpc]
    public void UpdateControlServerRpc(ulong id, int addScore, bool finished, string type)
    {
        UpdateControlClientRpc(id, addScore, finished, type);
    }

    [ClientRpc]
    void UpdateControlClientRpc(ulong id, int addScore, bool finished, string type)
    {
        if (finished && (NetworkObjectId-1) == id && _progress)
        {
            _progress.enabled = false;
            _gameControl.Ghost(objectMovement.model.GetComponent<MeshRenderer>());
        }

        if (_gameControl.UpdateScores(id, addScore, finished, type) && _progress)
        {
            ImFinishedServerRpc();
        }
    }
    
    [ServerRpc]
    public void ImFinishedServerRpc()
    {
        imFinished = true;
        int count = 0;
        foreach (var c in FindObjectsOfType<NetworkControl>())
        {
            count += c.imReady ? 1 : 0;
        }

        ulong[] ids = NetworkManager.ConnectedClients.Keys.ToArray();
        foreach (var client in ids)
        {
            NetworkManager.DisconnectClient(client);
        }
        NetworkManager.Shutdown();
    }

    int CountReady()
    {
        int count = 0;
        foreach (var c in FindObjectsOfType<NetworkControl>())
        {
            count += c.imReady ? 1 : 0;
        }

        return count;
    }
    
    public override void OnNetworkDespawn()
    {
        ClearEverything();
    }

    void ClearEverything()
    {
        FindObjectOfType<MapGenerator>().ClearInfo();
        FindObjectOfType<LobbyManager>().ClearInfo();
        
        _mainMenu.ClearInfoAndShowStats(_gameControl.PlayerDatas);
        
        _gameControl.ClearInfo();
    }
}