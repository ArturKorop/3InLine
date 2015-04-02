using UnityEngine;
using System.Collections;

public class GemController : MonoBehaviour
{
    public void OnMouseDown()
    {
        BoardController.Instance.GemPressed(this.gameObject);
    }
}
