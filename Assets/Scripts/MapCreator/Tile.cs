using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public enum Type
    {
        NONE,
        STRAIGHT,
        LEFTTURN,
        RIGHTTURN
    }

    [SerializeField] private Image sr;
    public Type type;
    public bool canbe = false;
    public int x = 0, y = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (type == Type.RIGHTTURN)
        {
            type = 0;
        }
        else
        {
            type++;
        }

        switch (type)
        {
            case Type.NONE:
                sr.color = Color.black;
                break;
            case Type.STRAIGHT:
                sr.color = Color.yellow;
                break;
            case Type.LEFTTURN:
                sr.color = Color.green;
                break;
            case Type.RIGHTTURN:
                sr.color = Color.blue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}