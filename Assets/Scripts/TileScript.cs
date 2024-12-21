using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    public int x, y; // position
    public int cost_to_end = 0, 
               total_Cost = 0, 
               cost_from_start = 0;

    public bool isTraversable = true; // default traversable true
    public TileScript parent; // used for path drawing

    // text components inside prefab
    public TextMeshProUGUI Total_txt;
    public TextMeshProUGUI From_txt;
    public TextMeshProUGUI To_txt;

    // Start is called before the first frame update
    void Start()
    {
        To_txt.text = cost_to_end.ToString();
        Total_txt.text = total_Cost.ToString();
    }

    public void SetCostFrom(int cost) // set texts with calculated numbers
    {
        cost_from_start = cost;
        From_txt.text = cost_from_start.ToString();

        total_Cost = cost_to_end + cost_from_start;
        Total_txt.text = total_Cost.ToString();
    }

    public void SetUnTraversable() // set color black and bool false
    {
        isTraversable = false;
        GetComponentInChildren<Image>().color = Color.black;
    }      
}
