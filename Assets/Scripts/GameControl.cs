using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public GameObject notificationPrefab;
    public Color notificationGood;
    public Color notificationBad;

    public class PlayerData
    {
        public string name;
        public int score;
        public Color color;
        public ulong id;
        public bool finished;
        public int skillScore;
        public int speedScore;

        public PlayerData(string name, int score, Color color, ulong id)
        {
            this.name = name;
            this.score = score;
            this.color = color;
            this.id = id;
            finished = false;
        }
    }

    public List<PlayerData> PlayerDatas = new List<PlayerData>();
    [SerializeField] private Text[] namesField;
    [SerializeField] private Text[] scoresField;
    [SerializeField] private Text lapsText;
    [SerializeField] private Text checkpointsText;
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private Material ghostMaterial;


    public bool UpdateScores(ulong id, int addScore, bool finished, string type)
    {
        PlayerData player = PlayerDatas.Find(pd => pd.id == id);
        player.score += addScore;
        if (type == "skill")
        {
            player.skillScore += addScore;
        }

        if (finished)
        {
            player.speedScore += addScore;
            player.finished = true;
        }

        UpdatePrintScores();

        return PlayerDatas.TrueForAll(pd => pd.finished);
    }

    public void UpdateForMe(int laps, int checkpoints, int totalLaps, int totalCheckpoints)
    {
        lapsText.text = "Laps: " + laps + "/" + totalLaps;
        checkpointsText.text = "Checkpoints: " + checkpoints + "/" + totalCheckpoints;
    }

    public void UpdatePrintScores()
    {
        PlayerDatas.Sort((x, y) =>
            -1 * x.score.CompareTo(y.score));
        int i = 0;
        foreach (var player in PlayerDatas)
        {
            namesField[i].text = player.name;
            namesField[i].color = player.color;
            scoresField[i].text = player.score.ToString();
            i++;
        }
    }

    public void Ghost(MeshRenderer meshRenderer)
    {
        ghostObject.SetActive(true);
        meshRenderer.materials[1] = ghostMaterial;
    }

    public void ClearInfo()
    {
        ghostObject.SetActive(false);
        PlayerDatas.Clear();
    }
}