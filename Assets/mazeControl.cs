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

    public GameObject activeTypeTile;

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
    Dictionary<Material, string> materialToType = new Dictionary<Material, string>();


    public Camera gameCamera;

    /* REGARDING GLOBAL INDICES:
     there is (unnamed) global indices associated with each possible 
    position on the map via indexToPos(). 
    - Within the range of 0 to the dimensions specified for the maze in mazeDimMN, 
        they correspond to the indices of the visual maze 
    - with a fixed horizontal index of -selectorOffsetToLeftInTiles and the vertical indices
        of 0 to the number of field types, they vertical indices correspond to the 
        indices of arrayOfMaterial
    */
    Vector2Int indexSelectedTile;
    
    public int selectorOffsetToLeftInTiles;
    string activeType;


    void Start()
    {
        activeType = free;
        activeTypeTile.GetComponent<Renderer>().material = materialFree;
        arrayOfMaterial = new string[4] {free, start, dest, obst};
        initializeMaterialDict();
        initializeMaze();
        // generate example types for type selection
        initializeTypeSelectors(selectorOffsetToLeftInTiles);
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
            if (IsSelectorIndex(indices)){
                getFieldType(indices);
            } else if (IsInMaze(indices)){
                changeFieldType(indexSelectedTile.x, indexSelectedTile.y, activeType);

            }
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

        materialToType.Add(materialFree, free);
        materialToType.Add(materialStart, start);
        materialToType.Add(materialDest, dest);
        materialToType.Add(materialObst, obst);
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

        // first if: if not selector tile
        if (!IsSelectorIndex(indices)){
            // second if: and also not inside maze, then return -1 ,-1
            if (!IsInMaze(indices)){
            print("Index conversion warning: indices ["+indices.x+","+indices.y+"] outside of maze array: ["+mazeDimMN.x+","+mazeDimMN.y+"].");
            return new Vector2Int(-1, -1);
            }
        }
        // else return the global index        
        return indices;
    }

    bool IsSelectorIndex(Vector2Int indices){
        return (indices.y == -selectorOffsetToLeftInTiles && indices.x >= 0 && indices.x < arrayOfMaterial.Length);
    }

    bool IsInMaze(Vector2Int indices){
        return (indices.x >= 0 && indices.x < mazeDimMN.x && indices.y >= 0 && indices.y < mazeDimMN.y);
    }

        /* set the currently active field type according to users choice
    and color the demonstration tile accordingly */
    void getFieldType(Vector2Int indices){
        activeType = arrayOfMaterial[indices.x];
        activeTypeTile.GetComponent<Renderer>().material = typeToMaterial[activeType];
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
            print("hovered over tile changed.");
            // reset the selection color in the previously selected tile (if it was within array)
            GameObject previousHoverTileBack = getTileObject(indexSelectedTile, 1);
            if (previousHoverTileBack != null){
                previousHoverTileBack.GetComponent<Renderer>().material = materialTileEdge;
                }

            // color the newly selected tile (only if within maze/selector)
            GameObject hoverTileBack = getTileObject(indices, 1);
            if (hoverTileBack != null){
                print("we are actually hovering over a tile");
                hoverTileBack.GetComponent<Renderer>().material = materialSelect;                
            } else {
                print("not hovering over legit tile");
            }
            // change the reference of selected tile to the currently selected one
            indexSelectedTile = indices;
        }
    }

    /* use the global indices to return a tile of either the visual maze array or
    the visual selector array depending on which the indices correspond to
    returns tile object, front or back, return null for any other index */
    GameObject getTileObject(Vector2Int indices, int frontOrBack){
        if (indices.y == -selectorOffsetToLeftInTiles){
            if (indices.x >= 0 && indices.x < visualSelectorArray.Length){
                // return selector tile
                return visualSelectorArray[indices.x, frontOrBack];
            } else {
                return null;
            }
        } else if (indices.x >= 0 && indices.x < mazeDimMN.x && indices.y >= 0 && indices.y < mazeDimMN.y){
            // return visual maze tile
            return visualMaze[indices.x, indices.y, frontOrBack];
        } else {
            return null;
        }
        }
    }



