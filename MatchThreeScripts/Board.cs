using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WAIT,
    MOVE
}
public enum BombType
{
    COLOR,
    DIRECTIONAL
}

public class Board : MonoBehaviour {

    public GameState currentState = GameState.MOVE;
    public int width;
    public int height;

    public int dropDownOffset;

    public GameObject tilePrefab;
    //private BackgroundTile[,] tileBoard;
    public GameObject destroyEffect;
    public GameObject[] tiles;

    public GameObject[,] allTiles;
    public Tile currentTile;

    private FindMatches findMatches;

    // Use this for initialization
    void Start ()
    {
        findMatches = FindObjectOfType<FindMatches>();
        //tileBoard = new BackgroundTile[width, height];
        allTiles = new GameObject[width, height];
        Setup();
	}

    void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 PositionPlacement = new Vector2(x, y);
                GameObject BGTile = Instantiate(tilePrefab, PositionPlacement, Quaternion.identity) as GameObject;
                BGTile.transform.parent = this.transform;
                BGTile.name = "( " + x + ", " + y + ")";
              
                int dotToUse = Random.Range(0, tiles.Length);
                int maxIterations = 0;
                while (MatchesAt(x, y, tiles[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, tiles.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                Vector2 DotPosPlacement = new Vector2(x, y + dropDownOffset);

                GameObject dot = Instantiate(tiles[dotToUse], DotPosPlacement, Quaternion.identity);
                dot.GetComponent<Tile>().row = y;
                dot.GetComponent<Tile>().column = x;

                dot.transform.parent = this.transform;
                dot.name = BGTile.name;

                allTiles[x, y] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allTiles[column - 1, row].tag == piece.tag &&
                allTiles[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (allTiles[column, row - 1].tag == piece.tag &&
                allTiles[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allTiles[column, row - 1].tag == piece.tag &&
                allTiles[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allTiles[column - 1, row].tag == piece.tag &&
                allTiles[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }


        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allTiles[column, row].GetComponent<Tile>().isMatched)
        {
            //ow many elements are in the matched pieces from findmatches?
            if (findMatches.CurrentMatches.Count == 4 || findMatches.CurrentMatches.Count == 7)
            {
                findMatches.CheckBombs(BombType.DIRECTIONAL);
            }
            if (findMatches.CurrentMatches.Count == 5)
            {
                //Setup Color Bombs();
                findMatches.CheckBombs(BombType.COLOR);
            }

            GameObject particle = Instantiate(destroyEffect, allTiles[column, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.5f);
            Destroy(allTiles[column, row]);
            allTiles[column, row] = null;
        }
    }
    public void DestroyMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] != null)
                {
                    DestroyMatchesAt(x, y);
                }
            }
        }
        findMatches.CurrentMatches.Clear();
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allTiles[x, y].GetComponent<Tile>().row -= nullCount;
                    allTiles[x, y] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] == null)
                {
                    Vector2 tempPosition = new Vector2(x, y + dropDownOffset);
                    int dotToUse = Random.Range(0, tiles.Length);
                    GameObject piece = Instantiate(tiles[dotToUse], tempPosition, Quaternion.identity);
                    allTiles[x, y] = piece;
                    piece.GetComponent<Tile>().row = y;
                    piece.GetComponent<Tile>().column = x;

                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] != null)
                {
                    if (allTiles[x, y].GetComponent<Tile>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.2f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.2f);
            DestroyMatches();
        }
        findMatches.CurrentMatches.Clear();
        yield return new WaitForSeconds(0.2f);
        currentState = GameState.MOVE;
    }
}


