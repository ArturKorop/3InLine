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
            Destroy(target.Object);
            Destroy(this.currentPressed.Object);
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
}
