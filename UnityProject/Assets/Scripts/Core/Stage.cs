using UnityEngine;

/// <summary>
/// Represents enemy lineup and spawn positions for a battle stage.
/// </summary>
public class Stage
{
    

    private GameObject[] enemyList;
    private Vector3[] positionList;

    

    /// <summary>
    /// Builds a stage from enemy references and their spawn positions.
    /// </summary>
    public Stage(GameObject[] enemyList, Vector3[] positionList){
        this.enemyList = enemyList;
        this.positionList = positionList;
    }

    /// <summary>
    /// Builds an empty stage with no enemies and no positions.
    /// </summary>
    public Stage(){
        this.enemyList = new GameObject[0];
        this.positionList = new Vector3[0];
    }

    
    

    public void setEnemyList(GameObject[] list){ enemyList = list; }
    public void setPositionList(Vector3[] list){ positionList = list; }

    public GameObject[] getEnemyList(){ return enemyList; }
    public Vector3[] getPositionList(){ return positionList; }


    

    /// <summary>
    /// Resets and shows the stage entities at battle start.
    /// </summary>
    public void start(GameObject[] characters){
        reset(characters);
        show(characters);
    }

    /// <summary>
    /// Hides stage enemies and player characters.
    /// </summary>
    public void hide(GameObject[] characters){
        
        
        characters[0].gameObject.SetActive(false);
        characters[1].gameObject.SetActive(false);

        
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Restores enemies to their configured spawn positions.
    /// </summary>
    private void reset(GameObject[] characters){
        
        
        
        

        
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].transform.position = positionList[i];
        }
    }

    /// <summary>
    /// Shows stage enemies and player characters.
    /// </summary>
    private void show(GameObject[] characters){
        
        
        characters[0].gameObject.SetActive(true);
        characters[1].gameObject.SetActive(true);

        
        for (int i=0; i<enemyList.Length; i++){
            enemyList[i].gameObject.SetActive(true);
        }
    }
}