using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mazeControl : MonoBehaviour
{
    public Vector2Int mazeDimMN;
    // the logical maze: m x n
    string[,] maze;
    // the visual maze: m x n x 2, last dimension: 0->front of tile, 1-> back of tile
    GameObject[,,] visualMaze; 

    // tile Object
    public float tileSize; //  WARNING! NOT YET LINKED TO ACTUAL TILESIZE, BUT CALCULATIONS ONLY!
    public float tileInnerSize;
    public GameObject tileObject;

    // materials 
    public Material materialTileEdge;
    public Material materialSelect;
    public Material materialFree;
    public Material materialStart;
    public Material materialDest;
    public Material materialObst;


    public Camera gameCamera;
    string free = "free", start = "start", dest = "destination", obst = "obstacle";

    Vector2Int indexSelectedTile;

    // link string to material with dictionary


    void Start()
    {   
        // initialize array with right dimensions and all positions set to free
        maze = new string[mazeDimMN.x, mazeDimMN.y];
        visualMaze = new GameObject[mazeDimMN.x, mazeDimMN.y, 2];
        for (int i = 0; i < mazeDimMN.x; i++){
            for (int j = 0; j <  mazeDimMN.y; j++){
                // LOGICAL: assign to the logical maze array
                maze[i,j] = free;

                // VISUAL: 
                // spawn the tiles
                //Vector2 tilePositionOnMap = new Vector2(j*tileSize, i*tileSize);
                Vector2 tilePositionOnMap = indexToPos(i,j);

                // the background -> edge of a tile
                GameObject newTileBack = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
                newTileBack.GetComponent<Renderer>().material = materialTileEdge;
                newTileBack.transform.parent = transform;
                visualMaze[i,j,1] = newTileBack;

                // the tile filling itself
                GameObject newTile = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
                // scale it down to make the background visible as edge
                newTile.transform.localScale = new Vector3(tileInnerSize, tileInnerSize);
                // move it to the front so that it is rendered in front of the background "edge"
                newTile.transform.position = new Vector3(newTile.transform.position.x, newTile.transform.position.y, 0);
                newTile.GetComponent<Renderer>().material = materialFree;
                newTile.transform.parent = newTileBack.transform;
                visualMaze[i,j,0] = newTile;

            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        //print(gameCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        print(mousePosition);
        Vector2Int indices = posToIndex(mousePosition);

        
        // adjust selection color if the selected tile (hovered over by mouse) has changed
        if (indices != indexSelectedTile){
            if (indexSelectedTile.x != -1){
                // reset the selection color in the previously selected tile (if it was within array)
                visualMaze[indexSelectedTile.x, indexSelectedTile.y, 1].GetComponent<Renderer>().material = materialTileEdge;
            }
            
            // color the newly selected tile (only if within maze)
            if (indices.x != -1){
                visualMaze[indices.x, indices.y, 1].GetComponent<Renderer>().material = materialSelect;
            }
            //visualMaze[indices.x, indices.y, 1].GetComponent<Renderer>().material = materialSelect;
            // change the reference of selected tile to the currently selected one
            indexSelectedTile = indices;
        }

    

        print("index: [" + indices.x + "," + indices.y + "]");
        //visualMaze[indices.x, indices.y, 1].GetComponent<Renderer>().material = materialSelect;

 
        
    }

    // convert indices to position on map (x vertical, y horizontal)
    Vector2 indexToPos(int x, int y){
        if (x < 0 || x >= mazeDimMN.x || y < 0 || y >= mazeDimMN.y){
            print("Error in indexToPos: indices ["+x+","+y+"] outside of maze array: ["+mazeDimMN.x+","+mazeDimMN.y+"].");
            return Vector2.zero;
        }
        Vector2 position = new Vector2(y*tileSize, x*tileSize);
        return position;        
        

    }

    // convert position on map to indices in maze array
    Vector2Int posToIndex(Vector2 position){
        Vector2Int indices = Vector2Int.zero;
        Vector2 offset = new Vector2(tileSize/2, tileSize/2);
        position += offset;
        indices.x = Mathf.FloorToInt(position.y);
        indices.y = Mathf.FloorToInt(position.x);

        if (indices.x < 0 || indices.x >= mazeDimMN.x || indices.y < 0 || indices.y >= mazeDimMN.y){
            print("Index conversion warning: indices ["+indices.x+","+indices.y+"] outside of maze array: ["+mazeDimMN.x+","+mazeDimMN.y+"].");
            return new Vector2Int(-1, -1);
        }
        return indices;

        
    }
}
