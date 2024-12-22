using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class A_star : MonoBehaviour
{
    public GameObject prefab; // tile prefab

    // important tile refs
    public TileScript startTile;
    public TileScript endTile;
    public TileScript currTile;

    // tile lists
    Dictionary<(int, int), TileScript> kvp_All_Tiles = new(); // list that holds positions and tile refs
    private List<TileScript> openTiles = new(); // list that is used in traversing

    int cornerCost = 14, sideCost = 10; // for easier reading only

    // set start and end positions manually here
    int startX = 2, startY = 20;
    int endX = 19, endY = 2;

    // Start is called before the first frame update
    void Start()
    {
        openTiles = kvp_All_Tiles.Values.ToList();        
        currTile = startTile; // start traversing from start tile
        StartCoroutine(TraverseTroughTiles());
    }

    private void Awake()
    {
        // spawn tiles one by one, set grid position values and calculate "to_end" distance
        for (int y = 0; y < 25; y++)
        {
            for (int x = 0; x < 25; x++)
            {
                // spawn tiles under grid component
                // grid component handels actual positioning and order
                var obj = Instantiate(prefab, this.transform); 
                var tilescript = obj.GetComponent<TileScript>();

                // insert position coordinates inside grid to tile
                tilescript.x = x;
                tilescript.y = y;

                // start and end tile with noticable own colors and refs taken for later use
                if (x == startX && y == startY)
                {
                    startTile = tilescript;
                    startTile.GetComponentInChildren<Image>().color = Color.green;
                }
                else if (x == endX && y == endY)
                {
                    endTile = tilescript;
                    endTile.GetComponentInChildren<Image>().color = Color.red;
                }

                // set approx 30% of tiles to be untraversable
                int rand = Random.Range(0, 10);
                if (rand < 3 && tilescript != startTile && tilescript != endTile) 
                {
                    tilescript.SetUnTraversable(); // make tile black and untraversable
                    continue; // skip adding black tiles to alltiles (opentiles later on)
                }

                // calculate cost to end tile
                tilescript.cost_to_end = GetDistance(tilescript);
                // add position and ref to list
                kvp_All_Tiles.Add((tilescript.x, tilescript.y), tilescript);
            }
        }
    }

    // go from end to start with parent ref logick and color tiles to green
    void DrawFoundPath() 
    {
        Debug.Log("Drawing found path");
        TileScript curr = endTile;
        while(curr != startTile)
        {
            if(curr != endTile) // skip coloring end tile
            curr.GetComponentInChildren<Image>().color = Color.cyan;
            curr = curr.parent;
        }
    }

    public IEnumerator TraverseTroughTiles()
    {        
        while (currTile != endTile)
        {
            yield return new WaitForSeconds(0.1f); // slow down function so steps can be seen

            // set all traversed tilest to be yellow
            // skip START and END so they hold their original color
            if (currTile != startTile && currTile != endTile)
                currTile.GetComponentInChildren<Image>().color = Color.yellow;            

            // go trough neighbours 3x3 square
            for (int i = -1; i < 2; i++) // horizontal three step
            { 
                for (int j = -1; j < 2; j++) // vertical three step
                {
                    if (i == 0 && j == 0) continue; // skip current tile

                    int xpos = currTile.x + i;
                    int ypos = currTile.y + j;

                    if(!kvp_All_Tiles.Keys.Contains((xpos, ypos))) continue; // skip out of grid positions and blocked black tiles
                    // get position corresponding tile
                    TileScript neighbour = kvp_All_Tiles[(xpos,ypos)];                    

                    if (!neighbour.isTraversable || !openTiles.Contains(neighbour)) continue; // skip closed tiles

                    if (j == 0 || i == 0) // cost is 10;
                    {
                        // new traversed score is lower than before
                        if ((sideCost + currTile.cost_from_start) < neighbour.cost_from_start || neighbour.cost_from_start == 0)
                        {
                            neighbour.SetCostFrom(sideCost + currTile.cost_from_start);
                            neighbour.parent = currTile;

                            // open tile again for calculations with new values
                            if (!openTiles.Contains(neighbour))
                                openTiles.Add(neighbour); 
                        }
                    }
                    else // cost is 14
                    {
                        // new traversed score is lower than before
                        if ((cornerCost + currTile.cost_from_start) < neighbour.cost_from_start  || neighbour.cost_from_start == 0)
                        {
                            neighbour.SetCostFrom(cornerCost + currTile.cost_from_start);
                            neighbour.parent = currTile;

                            // open tile again for calculations with new values
                            if (!openTiles.Contains(neighbour))
                                openTiles.Add(neighbour); 
                        }
                    }

                    // path found -> break pathfinding logick and draw path
                    if (neighbour == endTile)
                    {
                        DrawFoundPath();
                        yield return null; 
                    }
                    // change all visited neighbours color to magenta
                    else neighbour.GetComponentInChildren<Image>().color = Color.magenta;                    
                }
            }

            // remove current tile from open list
            openTiles.Remove(currTile);

            // choose new current to be smallest total cost neighbour
            List<TileScript> smallest = openTiles.OrderBy(x => x.total_Cost).Where(x => x.total_Cost > 0).ToList();
            currTile = smallest.First();

            // check for lowest cost to end tile *if multiple total costs are equal*
            for (int i = 0; i < smallest.Count(); i++)
            {
                // current has lower value dont go further
                if (smallest[i].total_Cost > currTile.total_Cost) break; 

                // choose the one with lowest cost to end tile
                if (smallest[i].total_Cost == currTile.total_Cost && smallest[i].cost_to_end < currTile.cost_to_end)
                    currTile = smallest[i];
            }            
        }
        
        yield return null;
    }

    int GetDistance(TileScript curr)
    {
        // calculate x and y distances in grid -> use only positive values
        int distX = Mathf.Abs(curr.x - endX);
        int distY = Mathf.Abs(curr.y - endY);

        // to get lowest possible value...
        // calculate cornercost (14 *) with lower direction value
        // and then add remaing distance (higher - lower) with edge/sidecost (10 *) value

        if (distX > distY) return cornerCost * distY + sideCost * (distX - distY); // x dir higher dist

        else return cornerCost * distX + sideCost * (distY - distX); // y dir higher dist
    }
}