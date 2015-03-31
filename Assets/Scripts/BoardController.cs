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
    private GameObjectValue[,] gemsField;
    private GameObjectValue currentPressed;

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

        this.gemsField = new GameObjectValue[this.Columns, this.Rows];
        this.BoardSetup();
    }

    public void GemPressed(GameObject gem)
    {
        GameObjectValue target = null;
        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                var obj = this.gemsField[i,j];
                if(gem == obj.Object)
                {
                    target = obj;
                    break;
                }
            }
        }
        
        if(this.currentPressed == null || this.currentPressed.Value != target.Value || this.GetDistance(this.currentPressed.Position, target.Position) > 1)
        {
            this.currentPressed = target;
        }
        else if(this.currentPressed.Object == target.Object)
        {

        }
        else
        {
            var targetNeighbours = this.GetAllSameColorNeighbours(target);
            var currentPressedNeighbours = this.GetAllSameColorNeighbours(this.currentPressed);
            var targetNeighboursCount = targetNeighbours.Count();
            var tnarray = targetNeighbours.ToArray();
            for (int i = 0; i < targetNeighboursCount; i++)
            {
                Destroy(tnarray[i].Object);
            }

            var pressedNeighboursCount = currentPressedNeighbours.Count();
            var pnarray = currentPressedNeighbours.ToArray();
            for (int i = 0; i < pressedNeighboursCount; i++)
            {
                Destroy(pnarray[i].Object);
            }
        }
    }

    private void BoardSetup()
    {
        this.boardHolder = new GameObject("Board").transform;

        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                var index = Random.Range(0, this.Gems.Count);
                var instance = Instantiate(this.Gems[index], new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                instance.transform.SetParent(this.boardHolder);
                this.gemsField[i, j] = new GameObjectValue { Value = index, Object = instance, Position = new Point { X = i, Y = j } };
            }
        }
    }

    private float GetDistance(Point a, Point b)
    {
        return Mathf.Sqrt(Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y));
    }

    private IEnumerable<GameObjectValue> GetAllSameColorNeighbours(GameObjectValue target)
    {
        var horizontalResult = new List<GameObjectValue>();

        for (int i = 0; i < target.Position.X; i++)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp.Value != target.Value)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        for (int i = target.Position.X + 1; i < this.Columns; i++)
        {
            var temp = this.gemsField[i, target.Position.Y];
            if (temp.Value != target.Value)
            {
                break;
            }

            horizontalResult.Add(temp);
        }

        var verticalResult = new List<GameObjectValue>();

        for (int i = target.Position.Y + 1; i < this.Rows; i++)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp.Value != target.Value)
            {
                break;
            }

            verticalResult.Add(temp);
        }

        for (int i = 0; i < target.Position.Y; i++)
        {
            var temp = this.gemsField[target.Position.X, i];
            if (temp.Value != target.Value)
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
        List<GameObjectValue> result = horizontalResult;
        if (result.Any())
        {
            result.Add(target);
        }

        return result;
    }
}
