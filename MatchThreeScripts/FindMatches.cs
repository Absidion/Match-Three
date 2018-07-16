using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour {

    private Board board;
    public List<GameObject> CurrentMatches = new List<GameObject>();

	// Use this for initialization
	void Start () {
        board = FindObjectOfType<Board>();		
	}

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }



    private IEnumerator FindAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                GameObject currentTileGO = board.allTiles[x, y];

                if (currentTileGO != null)
                {
                    Tile currentTile = currentTileGO.GetComponent<Tile>();
                    if (x > 0 && x < board.width - 1)
                    {
                        GameObject leftTileGO = board.allTiles[x - 1, y];
                        GameObject rightTileGO = board.allTiles[x + 1, y];

                        if (leftTileGO != null && rightTileGO != null)
                        {
                            Tile leftTile = leftTileGO.GetComponent<Tile>();
                            Tile rightTile = rightTileGO.GetComponent<Tile>();

                            if (leftTile.tag == currentTile.tag && rightTile.tag == currentTile.tag)
                            {
                                CurrentMatches.Union(isRowBomb(currentTile, leftTile, rightTile));

                                CurrentMatches.Union(isColumnBomb(currentTile, leftTile, rightTile));

                                GetNearbyTiles(currentTileGO, leftTileGO, rightTileGO);

                            }
                        }
                    }

                    if (y > 0 && y < board.height - 1)
                    {
                        GameObject upTileGO = board.allTiles[x, y-1];
                        GameObject downTileGO = board.allTiles[x, y+1];                       

                        if (upTileGO != null && downTileGO != null)
                        {
                            Tile upTile = upTileGO.GetComponent<Tile>();
                            Tile downTile = downTileGO.GetComponent<Tile>();

                            if (upTile.tag == currentTile.tag && downTile.tag == currentTile.tag)
                            {

                                CurrentMatches.Union(isColumnBomb(currentTile, upTile, downTile));
                                
                                CurrentMatches.Union(isRowBomb(currentTile, upTile, downTile));

                                GetNearbyTiles(currentTileGO, upTileGO, downTileGO);

                            }
                        }
                    }
                }
            }
        }
    }

    private void GetNearbyTiles(GameObject tile1, GameObject tile2, GameObject tile3)
    {
        AddToListAndMatch(tile1);
        AddToListAndMatch(tile2);
        AddToListAndMatch(tile3);
    }

    private void AddToListAndMatch(GameObject tile)
    {
        if (!CurrentMatches.Contains(tile))
        {
            CurrentMatches.Add(tile);
        }
        tile.GetComponent<Tile>().isMatched = true;
    }

    private List<GameObject> isRowBomb(Tile tile1, Tile tile2, Tile tile3)
    {
        List<GameObject> currentTiles = new List<GameObject>();
        if (tile1.isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(tile1.row));
        }
        if (tile2.isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(tile2.row));
        }
        if (tile3.isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(tile3.row));
        }
        return currentTiles;
    }

    private List<GameObject> isColumnBomb(Tile tile1, Tile tile2, Tile tile3)
    {
        List<GameObject> currentTiles = new List<GameObject>();
        if (tile1.isColumnBomb)
        {
            CurrentMatches.Union(GetColumnPieces(tile1.column));
        }
        if (tile2.isColumnBomb)
        {
            CurrentMatches.Union(GetColumnPieces(tile2.column));
        }
        if (tile3.isColumnBomb)
        {
            CurrentMatches.Union(GetColumnPieces(tile3.column));
        }
        return currentTiles;
    }

    public void MatchColoredTiles(string color)
    {
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                //Confirm that the piece exists
                if (board.allTiles[x, y] != null)
                {
                    //Compare the tag of the dot to the color we're looking for
                    if (board.allTiles[x, y].tag == color)
                    {
                        //Set the dot to be matched
                        board.allTiles[x, y].GetComponent<Tile>().isMatched = true;
                    }
                }

            }
        }
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i < board.height; i++)
        {
            if (board.allTiles[column, i] != null)
            {
                tiles.Add(board.allTiles[column, i]);
                board.allTiles[column, i].GetComponent<Tile>().isMatched = true;
            }
        }

        return tiles;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            if (board.allTiles[i, row] != null)
            {
                tiles.Add(board.allTiles[i, row]);
                board.allTiles[i, row].GetComponent<Tile>().isMatched = true;
            }
        }

        return tiles;
    }

    public void CheckBombs(BombType BT)
    {
        //Did the player move something?
        if (board.currentTile != null)
        {
            //is the place they moved matched?
            if (board.currentTile.isMatched)
            {
                //Make it unmatched
                //board.currentTile.isMatched = false;

                if ((board.currentTile.swipeAngle > -45 && board.currentTile.swipeAngle <= 45) ||
                    (board.currentTile.swipeAngle >= 135 || board.currentTile.swipeAngle < -135))
                {
                    //Right Swipe or Left Swipe
                    if (BT == BombType.DIRECTIONAL)
                        board.currentTile.otherTile.GetComponent<Tile>().ConvertToRowBomb();
                    else
                        board.currentTile.otherTile.GetComponent<Tile>().ConvertToColorBomb();
                }
                else if ((board.currentTile.swipeAngle > 45 && board.currentTile.swipeAngle <= 135) ||
                    (board.currentTile.swipeAngle < -45 && board.currentTile.swipeAngle >= -135))
                {
                    //Up swipe or down swipe
                    if (BT == BombType.DIRECTIONAL)
                        board.currentTile.otherTile.GetComponent<Tile>().ConvertToColumnBomb();
                    else
                        board.currentTile.otherTile.GetComponent<Tile>().ConvertToColorBomb();
                }

            }
            //Is the other piece matched?
            else if (board.currentTile.otherTile != null)
            {
                Tile otherTile = board.currentTile.otherTile.GetComponent<Tile>();
                //is the place they moved matched?
                if (otherTile.isMatched)
                {
                    //Make it unmatched
                    //board.currentTile.isMatched = false;

                    if ((board.currentTile.swipeAngle > -45 && board.currentTile.swipeAngle <= 45) ||
                    (board.currentTile.swipeAngle >= 135 || board.currentTile.swipeAngle < -135))
                    {
                        //Right Swipe or Left Swipe
                        board.currentTile.ConvertToRowBomb();
                    }
                    else if ((board.currentTile.swipeAngle > 45 && board.currentTile.swipeAngle <= 135) ||
                        (board.currentTile.swipeAngle < -45 && board.currentTile.swipeAngle >= -135))
                    {
                        //Up swipe or down swipe
                        board.currentTile.ConvertToColumnBomb();
                    }

                }
            }
        }
    }

}
