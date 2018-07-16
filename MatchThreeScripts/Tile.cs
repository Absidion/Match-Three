using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    //Current Position on the board
    public int column;
    public int row;

    //Previous Position on the board after a swap
    public int previousColumn;
    public int previousRow;

    //Target position on the board before a swap
    public int targetX;
    public int targetY;

    public bool isMatched = false;

    public GameObject otherTile;

    private Board board;
    private FindMatches findMatches;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Variables")]
    public float swipeAngle = 0;
    public float swipeResist = 1f; //So you have to swipe in one direction for it to count as a swipe

    [Header("Power Up Variables")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public GameObject rowBomb;
    public GameObject columnBomb;
    public GameObject colorBomb;

    // Use this for initialization
    void Start()
    {

        isColumnBomb = false;
        isRowBomb = false;

        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();

    }

    //This is for testing and Debug only.
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isColorBomb = true;
            GameObject bomb = Instantiate(colorBomb, transform.position, Quaternion.identity);
            bomb.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (board.currentState == GameState.WAIT)
        {
            findMatches.FindAllMatches();
        }

        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            //Move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if (board.allTiles[column, row] != this.gameObject)
            {
                board.allTiles[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            //Move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if (board.allTiles[column, row] != this.gameObject)
            {
                board.allTiles[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCoroutine()
    {
        if (isColorBomb)
        {
            //This piece is a color bomb, and the other piece is the color to destroy
            findMatches.MatchColoredTiles(otherTile.tag);
            isMatched = true;
        }
        else if (otherTile.GetComponent<Tile>().isColorBomb)
        {
            //The other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchColoredTiles(tag);
            isMatched = true;

        }
        yield return new WaitForSeconds(0.3f);
        if (otherTile != null)
        {
            if (!isMatched && !otherTile.GetComponent<Tile>().isMatched)
            {
                otherTile.GetComponent<Tile>().row = row;
                otherTile.GetComponent<Tile>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(0.5f);
                board.currentTile = null;
                board.currentState = GameState.MOVE;
            }
            else
            {
                board.DestroyMatches();
            
            }
            otherTile = null;
        }

    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.MOVE)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstTouchPosition);
        }
    }
    private void OnMouseUp()
    {
        if (board.currentState == GameState.MOVE)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            Debug.Log(swipeAngle);
            CalculateSwap();
            board.currentState = GameState.WAIT;
            board.currentTile = this;

        }
        else
        {
            board.currentState = GameState.MOVE;
        }
    }

    void CalculateSwap()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width-1)
        {
            //Right Swipe
            otherTile = board.allTiles[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherTile.GetComponent<Tile>().column -= 1;
            column += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            otherTile = board.allTiles[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherTile.GetComponent<Tile>().column += 1;
            column -= 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height-1)
        {
            //Up
            otherTile = board.allTiles[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherTile.GetComponent<Tile>().row -= 1;
            row += 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down
            otherTile = board.allTiles[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherTile.GetComponent<Tile>().row += 1;
            row -= 1;
        }

        StartCoroutine(CheckMoveCoroutine());
    }
    //An individual shape will look to it's left and right to find if it's neighbours having matching tags.
    //If the neighbours do have matching tags, they are considered to be a match of 3.
    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftTile = board.allTiles[column - 1, row];
            GameObject rightTile = board.allTiles[column + 1, row];
            if (leftTile != null && rightTile != null)
            {
                if (leftTile.tag == this.gameObject.tag && rightTile.tag == this.gameObject.tag)
                {
                    leftTile.GetComponent<Tile>().isMatched = true;
                    rightTile.GetComponent<Tile>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upTile = board.allTiles[column, row+1];
            GameObject downTile = board.allTiles[column, row-1];
            if (upTile != null && downTile != null)
            {
                if (upTile.tag == this.gameObject.tag && downTile.tag == this.gameObject.tag)
                {
                    upTile.GetComponent<Tile>().isMatched = true;
                    downTile.GetComponent<Tile>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }
    public void ConvertToRowBomb()
    {
        isRowBomb = true;
        GameObject bomb = Instantiate(rowBomb, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }

    public void ConvertToColumnBomb()
    {
        isColumnBomb = true;
        GameObject bomb = Instantiate(columnBomb, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }
    public void ConvertToColorBomb()
    {
        isColorBomb = true;
        GameObject bomb = Instantiate(colorBomb, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }
}
