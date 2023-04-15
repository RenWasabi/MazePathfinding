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
    GameObject[,] visualSelectorArray; // for choosing field types

    // tile Object
    public float tileSize; //  WARNING! NOT YET LINKED TO ACTUAL TILESIZE, BUT CALCULATIONS ONLY!
    public float tileInnerSize;
    public GameObject tileObject;

    // types and materials
    string free = "free", start = "start", dest = "destination", obst = "obstacle";
    public Material materialTileEdge;
    public Material materialSelect;
    public Material materialFree;
    public Material materialStart;
    public Material materialDest;
    public Material materialObst;
    string[] arrayOfMaterial;
    Dictionary<string, Material> typeToMaterial = new Dictionary<string, Material>();


    public Camera gameCamera;

    Vector2Int indexSelectedTile;



    void Start()
    {
        arrayOfMaterial = new string[4] {free, start, dest, obst};
        initializeMaterialDict();
        initializeMaze();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        print(mousePosition);
        Vector2Int indices = posToIndex(mousePosition);

        /* control flow according to indices 
        - insize of maze
            - on click
        - outsize of maze
        - type selection
            - on click
        */

        // color the edge of the tile currently hovered over with selection material
        colorHoveredOverTile(indices);

        

        if (Input.GetMouseButtonDown(0)){ // 0 should be primary button -> left click
            changeFieldType(indexSelectedTile.x, indexSelectedTile.y, obst);
        }



        print("index: [" + indices.x + "," + indices.y + "]");   
    }


    // initialize array with right dimensions and all positions set to free
    /* initialize both the logical array and its visual represantation */
    void initializeMaze(){
        maze = new string[mazeDimMN.x, mazeDimMN.y];
        visualMaze = new GameObject[mazeDimMN.x, mazeDimMN.y, 2];
        for (int i = 0; i < mazeDimMN.x; i++){
            for (int j = 0; j <  mazeDimMN.y; j++){
                // LOGICAL: assign to the logical maze array
                maze[i,j] = free;
                // VISUAL
                spawnFullTile(visualMaze, new Vector2Int(i,j), free);
            }

            // generate example types for type selection
            initializeTypeSelectors(2);

        }
    }
    

    /* handling the visual tile representation
    input: 
    - 3D array representing the maze visually
    - 2D index indicating the position in the overall world and array
    - string type -> which material / field type shall be spawned
       does:
    - spawns a tile at the corresponding position, i.e.
        - the back / tile edge (layer 0)
        - the smaller innner tile with the material (layer 1)
    */
    // for 2D (like the maze)
    void spawnFullTile(GameObject[,,] visualArray, Vector2Int indices, string type){
        Vector2 tilePositionOnMap = indexToPos(indices.x, indices.y);

        // the background -> edge of a tile
        GameObject newTileBack = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
        newTileBack.GetComponent<Renderer>().material = materialTileEdge;
        newTileBack.transform.parent = transform;
        visualArray[indices.x,indices.y,1] = newTileBack;

        // the tile filling itself
        GameObject newTile = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
        // scale it down to make the background visible as edge
        newTile.transform.localScale = new Vector3(tileInnerSize, tileInnerSize);
        // move it to the front so that it is rendered in front of the background "edge"
        newTile.transform.position = new Vector3(newTile.transform.position.x, newTile.transform.position.y, 0);
        newTile.GetComponent<Renderer>().material = typeToMaterial[type];
        newTile.transform.parent = newTileBack.transform;
        visualArray[indices.x,indices.y,0] = newTile;
    }
    // for 1D (like the selectors)
    // here the horizontal index is fixed, only moving vertical
    void spawnFullTile(GameObject[,] visualArray, int indexHorizontalFixed, int indexVertical, string type){
        Vector2 tilePositionOnMap = indexToPos(indexVertical, indexHorizontalFixed);

        // the background -> edge of a tile
        GameObject newTileBack = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
        newTileBack.GetComponent<Renderer>().material = materialTileEdge;
        newTileBack.transform.parent = transform;
        visualArray[indexVertical,1] = newTileBack;

        // the tile filling itself
        GameObject newTile = (GameObject) Instantiate(tileObject, tilePositionOnMap, Quaternion.identity);
        // scale it down to make the background visible as edge
        newTile.transform.localScale = new Vector3(tileInnerSize, tileInnerSize);
        // move it to the front so that it is rendered in front of the background "edge"
        newTile.transform.position = new Vector3(newTile.transform.position.x, newTile.transform.position.y, 0);
        newTile.GetComponent<Renderer>().material = typeToMaterial[type];
        newTile.transform.parent = newTileBack.transform;
        visualArray[indexVertical,0] = newTile;
        
    }

    void initializeTypeSelectors(int offsetToLeftInTiles){

        visualSelectorArray = new GameObject[arrayOfMaterial.Length, 2];
        int indexVertical = 0;
        foreach (string material in arrayOfMaterial){
            spawnFullTile(visualSelectorArray, -offsetToLeftInTiles, indexVertical, material);
            indexVertical++;
        }

    }



    // initialize field type string to material dictionary
    void initializeMaterialDict(){
        typeToMaterial.Add(free, materialFree);
        typeToMaterial.Add(start, materialStart);
        typeToMaterial.Add(dest, materialDest);
        typeToMaterial.Add(obst, materialObst);
    }


    // convert indices to position on map (x vertical, y horizontal)
    Vector2 indexToPos(int x, int y){

        /*
        if (x < 0 || x >= mazeDimMN.x || y < 0 || y >= mazeDimMN.y){
            print("Error in indexToPos: indices ["+x+","+y+"] outside of maze array: ["+mazeDimMN.x+","+mazeDimMN.y+"].");
            return Vector2.zero;
        }
        */
        Vector2 position = new Vector2(y*tileSize, x*tileSize);
        return position;        
        

    }

    /* convert position on map to indices in maze array
     return value [-1,-1] represents any invalid/meaningless position 
     -> neither in maze, nor type selector field */
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


    // for given indices, change the logical field type as well as the tile material
    // to the given type
    void changeFieldType(int x, int y, string type){
        maze[x,y] = type;
        visualMaze[x, y, 0].GetComponent<Renderer>().material = typeToMaterial[type];
    }

    // color the edge of the indexed tile with selection material
    void colorHoveredOverTile(Vector2Int indices){
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
            // change the reference of selected tile to the currently selected one
            indexSelectedTile = indices;
        }
    }


}
