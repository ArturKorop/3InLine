using UnityEngine;
using System.Collections;

public class BoardController : MonoBehaviour
{
    public GameObject Background;

    public int Columns = 8;
    public int Rows = 8;

    private Transform boardHolder;

    public void Awake()
    {
        this.BoardSetup();
    }

    private void BoardSetup()
    {
        this.boardHolder = new GameObject("Board").transform;

        for (int i = 0; i < this.Columns; i++)
        {
            for (int j = 0; j < this.Rows; j++)
            {
                var instance = Instantiate(this.Background, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                instance.transform.SetParent(this.boardHolder);
            }
        }
    }
}
