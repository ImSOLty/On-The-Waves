using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    
    [SerializeField] GameObject straight, corner, checkpoint, finish;
    public int checkpoints;
    public int multiplicator;
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

        public Tile(Type type, bool start, bool checkpoint, Vector3 position, Vector3 rotationEulers, Vector3 scale)
        {
            this.type = type;
            this.start = start;
            this.checkpoint = checkpoint;
            this.position = position;
            this.rotationEulers = rotationEulers;
            this.scale = scale;
        }
    }

    public Transform GenerateAndReturnStart(int track)
    {
        Tile[] tiles;
        switch (track)
        {
            case 0:
                tiles = Track1();
                break;
            case 1:
                tiles = Track2();
                break;
            case 2:
                tiles = Track3();
                break;
            default:
                tiles = Track1();
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
                ch.transform.position+= Vector3.up*12;
                if (tile.start)
                {
                    ch.GetComponent<Checkpoint>().number = -1;
                }
                else
                {
                    ch.GetComponent<Checkpoint>().number = checkpoints++;
                }
            }

            t.transform.parent = GameObject.Find("Track").transform;
            this.track.Add(t);
            multiplicator += 50;
        }
        
        return start;
    }

    public void ClearInfo()
    {
        checkpoints = 0;
        multiplicator = 0;
        foreach (var tile in track)
        {
            Destroy(tile);
        }

        track.Clear();
    }

    Tile[] Track1() 
    {
        return new Tile[]
        {
            new Tile(Type.STRAIGHT, true, true, new Vector3(140, 0, 0), new Vector3(0, 180, 0), new Vector3(1, 1, 1)),
            new Tile(Type.CORNER, false, false, new Vector3(140,0,-70), new Vector3(0,0,0), new Vector3(-1,1,1)),
            new Tile(Type.STRAIGHT, false, false, new Vector3(70,0,-70), new Vector3(0,-90,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(0,0,-70), new Vector3(0,90,0), new Vector3(1,1,1)),
            new Tile(Type.STRAIGHT, false, true, new Vector3(0,0,-140), new Vector3(0,180,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(0,0,-210), new Vector3(0,0,0), new Vector3(-1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,-210), new Vector3(0,90,0), new Vector3(-1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,-140), new Vector3(0,-180,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-140,0,-140), new Vector3(0,90,0), new Vector3(-1,1,1)),
            new Tile(Type.STRAIGHT, false, true, new Vector3(-140,0,-70), new Vector3(0,0,0), new Vector3(1,1,1)),
            new Tile(Type.STRAIGHT, false, false, new Vector3(-140,0,0), new Vector3(0,0,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-140,0,70), new Vector3(0,180,0), new Vector3(-1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,70), new Vector3(0,-90,0), new Vector3(-1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,0), new Vector3(0,0,0), new Vector3(1,1,1)),
            new Tile(Type.STRAIGHT, false, true, new Vector3(0,0,0), new Vector3(0,90,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(70,0,0), new Vector3(0,-90,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(70,0,70), new Vector3(0,180,0), new Vector3(-1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(140,0,70), new Vector3(0,-90,0), new Vector3(-1,1,1)),
        };
    }
    
    Tile[] Track2()
    {
        return new Tile[]
        {
            new Tile(Type.STRAIGHT, true, true, new Vector3(-35,0,0), new Vector3(0,180,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-35,0,-70), new Vector3(0,0,0), new Vector3(-1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(-105,0,-70), new Vector3(0,90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(-105,0,-140), new Vector3(0,0,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, false, new Vector3(-35,0,-140), new Vector3(0,90,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(35,0,-140), new Vector3(0,90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(105,0,-140), new Vector3(0,-90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(105,0,-70), new Vector3(0,180,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(35,0,-70), new Vector3(0,90,0), new Vector3(-1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(35,0,0), new Vector3(0,0,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(35,0,70), new Vector3(0,180,0), new Vector3(-1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(105,0,70), new Vector3(0,-90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(105,0,140), new Vector3(0,180,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, false, new Vector3(35,0,140), new Vector3(0,-90,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(-35,0,140), new Vector3(0,-90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(-105,0,140), new Vector3(0,90,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(-105,0,70), new Vector3(0,0,0), new Vector3(1,1,1) ),
            new Tile(Type.CORNER, false, false, new Vector3(-35,0,70), new Vector3(0,-90,0), new Vector3(-1,1,1) ),
        };
    }
    Tile[] Track3()
    {
        return new Tile[]
        {
            new Tile(Type.STRAIGHT, true, true, new Vector3(-70,0,0), new Vector3(0,180,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,-70), new Vector3(0,0,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(0,0,-70), new Vector3(0,90,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(70,0,-70), new Vector3(0,-90,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(70,0,0), new Vector3(0,0,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(70,0,70), new Vector3(0,-180,0), new Vector3(1,1,1) ),
            new Tile(Type.STRAIGHT, false, true, new Vector3(0,0,70), new Vector3(0,-90,0), new Vector3(1,1,1)),
            new Tile(Type.CORNER, false, false, new Vector3(-70,0,70), new Vector3(0,90,0), new Vector3(1,1,1) ),
        };
    }
}