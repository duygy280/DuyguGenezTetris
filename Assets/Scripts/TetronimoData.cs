using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//my piece is F - it consists of a total of 6 blocks with 3 rows in descending order frome top to bottom
public enum Tetronimo { I, O, T, J, L, S, Z, F }

[Serializable]

public struct TetronimoData
{
    public Tetronimo tetronimo;
    public Vector2Int[] cells;
    public Tile tile;
}