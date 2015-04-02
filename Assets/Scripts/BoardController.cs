using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class BoardController : MonoBehaviour
{
    public List<GameObject> Gems;
    public static BoardController Instance = null;

    public int Columns = 8;
    public int Rows = 8;

    private Transform boardHolder;
    private BoardObjectValue[,] gemsField;
    private BoardObjectValue previousPressed;
    private BoardObjectValue currentPressed;
    private List<int> GemIndexes;
    private bool inProcess;

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

        this.gemsField = new BoardObjectValue[this.Columns, this.Rows];
        this.GemIndexes = new List<int>();
        for (int i = 0; i < this.Gems.Count; i++)
        {
            this.GemIndexes.Add(i);
        }

        this.BoardSetup();
    }

    public void GemPressed(GameObject gem)
    {
        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                var obj = this.gemsField[i,j];
                if(gem == obj.Object)
                {
                    this.previousPressed = this.currentPressed;
                    this.currentPressed = obj;
                    this.inProcess = true;

                    break;
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (this.inProcess)
        {
            this.inProcess = false;
            if (this.previousPressed == null || this.currentPressed.Value == previousPressed.Value || this.GetDistance(this.currentPressed.Position, previousPressed.Position) > 1)
            {
                this.previousPressed = null;
            }
            else
            {
                StartCoroutine(this.ProcessGemsSwapping());
            }
        }
    }

    private IEnumerator ProcessGemsSwapping()
    {
        this.SwapPosition(this.currentPressed, this.previousPressed);

            yield return new WaitForSeconds(0.1f);

            var previousNeighbours = this.GetAllSameColorNeighbours(this.previousPressed);
            var currentNeighbours = this.GetAllSameColorNeighbours(this.currentPressed);

            var previousNeighboursCount = previousNeighbours.Count();
            var pnarray = previousNeighbours.ToArray();
            for (int i = 0; i < previousNeighboursCount; i++)
            {
                var removingGem = pnarray[i];
                this.gemsField[removingGem.Position.X, removingGem.Position.Y] = null;
                Destroy(removingGem.Object);
            }

            var currentNeighboursCount = currentNeighbours.Count();
            var cnarray = currentNeighbours.ToArray();
            for (int i = 0; i < currentNeighboursCount; i++)
            {
                var removingGem = cnarray[i];
                this.gemsField[removingGem.Position.X, removingGem.Position.Y] = null;
                Destroy(removingGem.Object);
            }

            if (pnarray.Any() || cnarray.Any())
            {
                yield return this.UpdateBoard();
            }
            else
            {
                SwapPosition(this.currentPressed, this.previousPressed);

                yield return new WaitForSeconds(0.1f);
            }

        yield return new WaitForSeconds(0.1f);
    }
    

    private IEnumerator UpdateBoard()
    {
        yield return this.MoveGemsToEmptySpaces();
        yield return this.CreateNewGemsForEmptySpaces();
    }

    private IEnumerator MoveGemsToEmptySpaces()
    {
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
                        nearestGem.Position = point;
                        nearestGem.Object.transform.position = new Vector3(point.X, point.Y, 0);

                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }
    }

    private IEnumerator CreateNewGemsForEmptySpaces()
    {
        yield return new WaitForSeconds(0.5f);
    }

    private void SwapPosition(BoardObjectValue a, BoardObjectValue b)
    {
        var aPosition = a.Position;
        var bPosition = b.Position;
        this.gemsField[aPosition.X, aPosition.Y] = b;
        b.Position = aPosition;
        this.gemsField[bPosition.X, bPosition.Y] = a;
        a.Position = bPosition;

        var tempPosition = b.Object.transform.position;
        b.Object.transform.position = a.Object.transform.position;
        a.Object.transform.position = tempPosition;
    }

    private BoardObjectValue FindNearestGemInColumns(Point position)
    {
        if(position.X == this.Rows - 1)
        {
            return null;
        }

        for (int i = position.Y + 1; i < this.Rows; i++)
        {
            var currentGem = this.gemsField[position.X, i];
            if(currentGem != null)
            {
                return currentGem;
            }
        }

        return null;
    }

    private void BoardSetup()
    {
        this.boardHolder = new GameObject("Board").transform;

        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                this.gemsField[i, j] = new BoardObjectValue { Position = new Point { X = i, Y = j } };
                var field = this.gemsField[i,j];
                var indexes = this.GemIndexes.ToList();
                GameObject instance = null;
                while (instance == null)
                {
                    var index = Random.Range(0, indexes.Count);
                    var value = indexes[index];
                    field.Value = value;
                    if (!this.GetAllSameColorNeighbours(field).Any())
                    {
                        instance = Instantiate(this.Gems[value], new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(this.boardHolder);
                        field.Object = instance;

                        //yield return new WaitForSeconds(1);
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

    private IEnumerable<BoardObjectValue> GetAllSameColorNeighbours(BoardObjectValue target)
    {
        var horizontalResult = new List<BoardObjectValue>();

        for (int i = target.Position.X - 1; i >= 0 ; i--)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp == null || temp.Value != target.Value)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        for (int i = target.Position.X + 1; i < this.Columns; i++)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp == null || temp.Value != target.Value)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        var verticalResult = new List<BoardObjectValue>();

        for (int i = target.Position.Y + 1; i < this.Rows; i++)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp == null || temp.Value != target.Value)
            {
                break;
            }

            verticalResult.Add(temp);
        }

        for (int i = target.Position.Y - 1; i >= 0 ; i--)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp == null || temp.Value != target.Value)
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
        List<BoardObjectValue> result = horizontalResult;
        if (result.Any())
        {
            result.Add(target);
        }

        return result;
    }
}
