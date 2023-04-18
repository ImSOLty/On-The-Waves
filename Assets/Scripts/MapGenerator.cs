using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject straight, corner, checkpoint, finish, coin;
    public int checkpoints;
    public int multiplicator;
    public int coinsAmount;
    private List<GameObject> track = new List<GameObject>();

    public enum Type
    {
        STRAIGHT,
        CORNER
    }

    public class Tile
    {
        public Vector3 position;
        public Vector3 rotationEulers;
        public Vector3 scale;
        public Type type;
        public bool start;
        public bool checkpoint;
        public bool withCoin;

        public Tile(Type type, bool start, bool checkpoint, bool withCoin, Vector3 position, Vector3 rotationEulers,
            Vector3 scale)
        {
            this.type = type;
            this.start = start;
            this.checkpoint = checkpoint;
            this.withCoin = withCoin;
            this.position = position;
            this.rotationEulers = rotationEulers;
            this.scale = scale;
        }
    }

    public Transform GenerateAndReturnStart(int trackId, string custom, bool isServer)
    {
        Tile[] tiles;
        switch (trackId)
        {
            case 0:
                tiles = Track(TRACK1);
                break;
            case 1:
                tiles = Track(TRACK2);
                break;
            case 2:
                tiles = Track(TRACK3);
                break;
            case 3:
                tiles = Track(custom);
                break;
            default:
                tiles = Track(TRACK1);
                break;
        }

        Transform start = null;
        checkpoints = 0;
        foreach (Tile tile in tiles)
        {
            GameObject t = Instantiate(tile.type == Type.CORNER ? corner : straight, tile.position,
                Quaternion.Euler(tile.rotationEulers));
            t.transform.localScale = tile.scale;
            if (tile.checkpoint)
            {
                GameObject inst = checkpoint;
                if (tile.start)
                {
                    inst = finish;
                    start = t.transform.Find("Checkpoint1");
                }

                GameObject ch = Instantiate(inst, t.transform.position, Quaternion.identity, t.transform);
                ch.transform.forward = t.transform.forward;
                ch.transform.position += Vector3.up * 12;
                if (tile.start)
                {
                    ch.name = (-1).ToString();
                }
                else
                {
                    ch.name = (checkpoints++).ToString();
                }
            }
            
            if (tile.type != Type.CORNER && tile.withCoin)
            {
                coinsAmount++;
                if (isServer)
                {
                    GameObject c = Instantiate(coin, Vector3.zero, Quaternion.identity, t.transform);
                    c.transform.position = tile.position + new Vector3(Random.Range(-15, 15), 1, Random.Range(-25, 25));
                    c.GetComponent<NetworkObject>().Spawn(true);
                }
            }

            t.transform.parent = GameObject.Find("Track").transform;
            track.Add(t);
            multiplicator += 50;
        }

        return start;
    }

    public void ClearInfo()
    {
        checkpoints = 0;
        multiplicator = 0;
        coinsAmount = 0;
        foreach (var tile in track)
        {
            Destroy(tile);
        }

        track.Clear();
    }

    Tile[] Track(string code)
    {
        List<Tile> tiles = new List<Tile>();
        byte[] decodedBytes = Convert.FromBase64String(code);
        string decodedText = Encoding.UTF8.GetString(decodedBytes);
        foreach (string part in decodedText.Split("*"))
        {
            if (part.Equals("")) continue;
            string[] data = part.Split("~");
            tiles.Add(new Tile(
                data[0].Equals("Corner") ? Type.CORNER : Type.STRAIGHT,
                data[1].Equals("True"),
                data[2].Equals("True"),
                data[3].Equals("True"),
                new Vector3(int.Parse(data[4]), 0, int.Parse(data[5])),
                new Vector3(0, int.Parse(data[6]) + (data[0].Equals("Corner") ? 0 : 180), 0),
                new Vector3(data[7].Equals("False") ? 1 : -1, 1, 1)));
        }

        return tiles.ToArray();
    }

    private const string TRACK1 =
        "U3RyYWlnaHR+VHJ1ZX5UcnVlfkZhbHNlfi0xNzV+LTM1fjE4MH5GYWxzZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+LTE3NX4zNX4xODB+VHJ1ZSpTdHJhaWdodH5GYWxzZX5GYWxzZX5GYWxzZX4tMTA1fjM1fjI3MH5GYWxzZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+LTM1fjM1fjI3MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfi0zNX4xMDV+MTgwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMzV+MTc1fjE4MH5UcnVlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4zNX4xNzV+MjcwflRydWUqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfjM1fjEwNX4zNjB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfjEwNX4xMDV+MjcwflRydWUqU3RyYWlnaHR+RmFsc2V+RmFsc2V+RmFsc2V+MTA1fjM1fjM2MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfjEwNX4tMzV+MzYwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4xMDV+LTEwNX4zNjB+VHJ1ZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+MzV+LTEwNX40NTB+VHJ1ZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+MzV+LTM1fjE4MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfi0zNX4tMzV+OTB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfi0xMDV+LTM1fjkwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMTA1fi0xMDV+MH5UcnVlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMTc1fi0xMDV+OTB+VHJ1ZSo=";

    private const string TRACK2 =
        "U3RyYWlnaHR+VHJ1ZX5UcnVlfkZhbHNlfi0zNX4tMzV+MTgwfkZhbHNlKlN0cmFpZ2h0fkZhbHNlfkZhbHNlfkZhbHNlfi0zNX4zNX4xODB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfi0zNX4xMDV+MTgwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMTA1fjEwNX45MH5UcnVlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMTA1fjE3NX4xODB+VHJ1ZSpTdHJhaWdodH5GYWxzZX5GYWxzZX5GYWxzZX4tMzV+MTc1fjI3MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5GYWxzZX5GYWxzZX4zNX4xNzV+MjcwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4xMDV+MTc1fjI3MH5UcnVlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4xMDV+MTA1fjM2MH5UcnVlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4zNX4xMDV+NDUwfkZhbHNlKlN0cmFpZ2h0fkZhbHNlflRydWV+RmFsc2V+MzV+MzV+MzYwfkZhbHNlKlN0cmFpZ2h0fkZhbHNlfkZhbHNlfkZhbHNlfjM1fi0zNX4zNjB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfjM1fi0xMDV+MzYwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4xMDV+LTEwNX4yNzB+VHJ1ZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+MTA1fi0xNzV+MzYwflRydWUqU3RyYWlnaHR+RmFsc2V+RmFsc2V+RmFsc2V+MzV+LTE3NX40NTB+RmFsc2UqU3RyYWlnaHR+RmFsc2V+RmFsc2V+RmFsc2V+LTM1fi0xNzV+NDUwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4tMTA1fi0xNzV+NDUwflRydWUqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfi0xMDV+LTEwNX4xODB+VHJ1ZSpDb3JuZXJ+RmFsc2V+RmFsc2V+RmFsc2V+LTM1fi0xMDV+MjcwfkZhbHNlKg==";


    private const string TRACK3 =
        "U3RyYWlnaHR+VHJ1ZX5UcnVlfkZhbHNlfjEwNX4zNX4xODB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfjEwNX4xMDV+MTgwfkZhbHNlKlN0cmFpZ2h0fkZhbHNlfkZhbHNlfkZhbHNlfjM1fjEwNX45MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfi0zNX4xMDV+OTB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfi0xMDV+MTA1fjkwfkZhbHNlKlN0cmFpZ2h0fkZhbHNlfkZhbHNlfkZhbHNlfi0xMDV+MzV+MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfi0xMDV+LTM1fjB+RmFsc2UqQ29ybmVyfkZhbHNlfkZhbHNlfkZhbHNlfi0xMDV+LTEwNX4wfkZhbHNlKlN0cmFpZ2h0fkZhbHNlfkZhbHNlfkZhbHNlfi0zNX4tMTA1fi05MH5GYWxzZSpTdHJhaWdodH5GYWxzZX5UcnVlfkZhbHNlfjM1fi0xMDV+LTkwfkZhbHNlKkNvcm5lcn5GYWxzZX5GYWxzZX5GYWxzZX4xMDV+LTEwNX4tOTB+RmFsc2UqU3RyYWlnaHR+RmFsc2V+RmFsc2V+RmFsc2V+MTA1fi0zNX4xODB+RmFsc2Uq";
}