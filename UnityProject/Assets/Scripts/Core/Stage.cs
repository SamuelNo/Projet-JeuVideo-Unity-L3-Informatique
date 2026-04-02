using UnityEngine;

public class Stage
{
    // ---------- Attributes ---------- //

    private GameObject[] enemyList;
    private Vector3[] positionList;

    // ---------- Constructors ---------- //

    public Stage(GameObject[] enemyList, Vector3[] positionList){
        this.enemyList = enemyList;
        this.positionList = positionList;
    }

    public Stage(){
        this.enemyList = new GameObject[0];
        this.positionList = new Vector3[0];
    }

    
    // ---------- Set and Get ---------- //

    public void setEnemyList(GameObject[] list){ enemyList = list; }
    public void setPositionList(Vector3[] list){ positionList = list; }

    public GameObject[] getEnemyList(){ return enemyList; }
    public Vector3[] getPositionList(){ return positionList; }


    // ---------- Methods ---------- //

    public void start(GameObject[] characters){
        ///<param> character : the characters chosen by the player </param>
        ///<summary> puts all element of the stage in place and displays them </summary>
        reset(characters);
        show(characters);
    }

    public void hide(GameObject[] characters){
        ///<param> character : the characters chosen by the player </param>
        ///<summary> hide all elements of the stage </summary>
        
        // hides the player's characters
        characters[0].gameObject.SetActive(false);
        characters[1].gameObject.SetActive(false);

        // hides the enemies
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].gameObject.SetActive(false);
        }
    }

    private void reset(GameObject[] characters){
        ///<param> character : the characters chosen by the player </param>
        ///<summary> puts all elements of the stage in place </summary>
        
        // places the player's characters
        // characters[0].transform.position = 
        // characters[1].transform.position = 

        // places the enemies
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].transform.position = positionList[i];
        }
    }

    private void show(GameObject[] characters){
        ///<param> character : the characters chosen by the player </param>
        ///<summary> displays all elements of the stage </summary>
        
        // shows the player's characters
        characters[0].gameObject.SetActive(true);
        characters[1].gameObject.SetActive(true);

        // shows the enemies
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].gameObject.SetActive(true);
        }
    }
}