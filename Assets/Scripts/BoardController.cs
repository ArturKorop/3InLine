using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public List<GameObject> Gems;
    public Text ScoreText;
    public static BoardController Instance = null;

    public int Columns = 8;
    public int Rows = 8;
    public int GemPrice;

    private Transform boardHolder;
    private BoardGem[,] gemsField;
    private BoardGem previousPressed;
    private BoardGem currentPressed;
    private List<int> GemIndexes;
    private bool canProcess;
    private int score;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        this.gemsField = new BoardGem[this.Columns, this.Rows];
        this.GemIndexes = new List<int>();
        for (int i = 0; i < this.Gems.Count; i++)
        {
            this.GemIndexes.Add(i);
        }

        this.canProcess = true;
        this.score = 0;
        this.BoardSetup();
    }

    public void GemPressed(GameObject gem)
    {
        if (this.canProcess)
        {
            for (int i = 0; i < this.Columns; i++)
            {
                for (int j = 0; j < this.Rows; j++)
                {
                    var obj = this.gemsField[i, j];
                    if (gem == obj.GameObject)
                    {
                        this.previousPressed = this.currentPressed;
                        this.currentPressed = obj;

                        break;
                    }
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (this.canProcess)
        {
            this.canProcess = false;
            if (this.previousPressed == null || this.currentPressed.Color == previousPressed.Color || this.GetDistance(this.currentPressed.Position, previousPressed.Position) > 1)
            {
                this.previousPressed = null;
                this.canProcess = true; 
            }
            else
            {
                StartCoroutine(this.ProcessGemsSwapping());
            }
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    private bool CheckGemStatus(BoardGem gem)
    {
        var neighbours = this.GetAllSameColorNeighbours(gem).ToArray(); ;
        var neighboursCount = neighbours.Count();
        for (int i = 0; i < neighboursCount; i++)
        {
            var removingGem = neighbours[i];
            this.gemsField[removingGem.Position.X, removingGem.Position.Y] = null;
            Destroy(removingGem.GameObject);
        }

        this.UpdateScore(neighboursCount * this.GemPrice * (int)(neighboursCount/2));

        return neighboursCount > 0;
    }

    private IEnumerator ProcessGemsSwapping()
    {
            this.SwapPosition(this.currentPressed, this.previousPressed);

            yield return new WaitForSeconds(0.2f);

            if (this.CheckGemStatus(this.currentPressed) | this.CheckGemStatus(this.previousPressed))
            {
                yield return StartCoroutine(this.ProcessUpdateBoard());
            }
            else
            {
                SwapPosition(this.currentPressed, this.previousPressed);
            }

        yield return new WaitForSeconds(0.2f);
        this.previousPressed = null;
        this.currentPressed = null;
        this.canProcess = true;
    }

    private IEnumerator ProcessUpdateBoard()
    {
        yield return StartCoroutine(this.MoveGemsToEmptySpaces());

        yield return StartCoroutine(this.CreateNewGemsForEmptySpaces());
    }

    private IEnumerator MoveGemsToEmptySpaces()
    {
        var movedGems = new List<BoardGem>();
        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                var currentGem = this.gemsField[i, j];
                if(currentGem == null)
                {
                    var point = new Point { X = i, Y = j };
                    var nearestGem = this.FindNearestGemInColumns(point);
                    if(nearestGem != null)
                    {
                        this.gemsField[nearestGem.Position.X, nearestGem.Position.Y] = null;
                        nearestGem.Position = point;
                        nearestGem.GameObject.transform.position = new Vector3(point.X, point.Y, 0);
                        this.gemsField[i, j] = nearestGem;
                        movedGems.Add(nearestGem);

                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }

        var needMoveAnotherGems = false;
        if(movedGems.Any())
        {
            foreach (var gem in movedGems)
            {
                if(this.CheckGemStatus(gem))
                {
                    needMoveAnotherGems = true;
                }
            }
        }

        if(needMoveAnotherGems)
        {
            yield return StartCoroutine(this.MoveGemsToEmptySpaces());
        }
    }

    private IEnumerator CreateNewGemsForEmptySpaces()
    {
        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                if (this.gemsField[i, j] != null)
                {
                    continue;
                }

                this.gemsField[i, j] = new BoardGem { Position = new Point { X = i, Y = j } };
                var field = this.gemsField[i, j];
                var indexes = this.GemIndexes.ToList();
                GameObject instance = null;
                while (instance == null)
                {
                    var index = Random.Range(0, indexes.Count);
                    var value = indexes[index];
                    field.Color = value;
                    if (!this.GetAllSameColorNeighbours(field).Any())
                    {
                        instance = Instantiate(this.Gems[value], new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(this.boardHolder);
                        field.GameObject = instance;

                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        indexes.Remove(value);
                    }
                }
            }
        }

        yield return new WaitForEndOfFrame();
    }

    private void BoardSetup()
    {
        this.boardHolder = new GameObject("Board").transform;

        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                if(this.gemsField[i,j] != null)
                {
                    continue;
                }

                this.gemsField[i, j] = new BoardGem { Position = new Point { X = i, Y = j } };
                var field = this.gemsField[i,j];
                var indexes = this.GemIndexes.ToList();
                GameObject instance = null;
                while (instance == null)
                {
                    var index = Random.Range(0, indexes.Count);
                    var value = indexes[index];
                    field.Color = value;
                    if (!this.GetAllSameColorNeighbours(field).Any())
                    {
                        instance = Instantiate(this.Gems[value], new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(this.boardHolder);
                        field.GameObject = instance;
                    }
                    else
                    {
                        indexes.Remove(value);
                    }
                }
            }
        }
    }

    private float GetDistance(Point a, Point b)
    {
        return Mathf.Sqrt(Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y));
    }

    private IEnumerable<BoardGem> GetAllSameColorNeighbours(BoardGem target)
    {
        var horizontalResult = new List<BoardGem>();

        for (int i = target.Position.X - 1; i >= 0 ; i--)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp == null || temp.Color != target.Color)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        for (int i = target.Position.X + 1; i < this.Columns; i++)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp == null || temp.Color != target.Color)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        var verticalResult = new List<BoardGem>();

        for (int i = target.Position.Y + 1; i < this.Rows; i++)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp == null || temp.Color != target.Color)
            {
                break;
            }

            verticalResult.Add(temp);
        }

        for (int i = target.Position.Y - 1; i >= 0 ; i--)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp == null || temp.Color != target.Color)
            {
                break;
            }

            verticalResult.Add(temp);
        }

        if (horizontalResult.Count < 2)
        {
            horizontalResult.Clear();
        }

        if (verticalResult.Count < 2)
        {
            verticalResult.Clear();
        }

        horizontalResult.AddRange(verticalResult);
        List<BoardGem> result = horizontalResult;
        if (result.Any())
        {
            result.Add(target);
        }

        return result;
    }

    private BoardGem FindNearestGemInColumns(Point position)
    {
        for (int i = position.Y + 1; i < this.Rows; i++)
        {
            var currentGem = this.gemsField[position.X, i];
            if (currentGem != null)
            {
                return currentGem;
            }
        }

        return null;
    }

    private void SwapPosition(BoardGem a, BoardGem b)
    {
        var aPosition = a.Position;
        var bPosition = b.Position;
        this.gemsField[aPosition.X, aPosition.Y] = b;
        b.Position = aPosition;
        this.gemsField[bPosition.X, bPosition.Y] = a;
        a.Position = bPosition;

        var tempPosition = b.GameObject.transform.position;
        b.GameObject.transform.position = a.GameObject.transform.position;
        a.GameObject.transform.position = tempPosition;
    }

    private void UpdateScore(int addValue)
    {
        this.score += addValue;
        this.ScoreText.text = "Score: " + this.score;
    }
}
