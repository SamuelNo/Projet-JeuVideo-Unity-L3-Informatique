using UnityEngine;
using System.IO;

public class Save : MonoBehaviour
{
    // ---------- Attributes  ---------- 
    private string filePath, tmp;
    private Principal principalScript;

    // saved data
    private string[] data;
    public int currentStage, unlockedStage;

    //  ---------- Initialisation  ---------- 
    void Awake(){
        filePath = Application.dataPath + "/Scripts/Data/Save/save.txt";
        principalScript = this.GetComponent<Principal>();
    }

    void Start(){
        // loads data from the file
        data = read().Split(" ");
        
        // current stage number
        currentStage = int.Parse(data[0]);
        
        // unlocked stage number
        unlockedStage = int.Parse(data[1]);
    }

    //  ----------  Methods  ---------- 

    public void loadFile(){
        ///<summary> loads the data saved in the file </summary>
        
        // loads data from the file
        data = read().Split(" ");
        
        // current stage number
        currentStage = int.Parse(data[0]);
        principalScript.setSelectedStage(currentStage);
        
        // unlocked stage number
        unlockedStage = int.Parse(data[1]);
        principalScript.setUnlockedStage(unlockedStage);
    }

    public void save(){
        ///<summary> saves data into the file </summary>
        
        // saves data
        writeOverwrite(currentStage.ToString() +" "+ unlockedStage.ToString());
    }

    public void reset(){
        ///<summary> resets all the data in the file and loads it </summary>
        
        // resets data
        writeOverwrite("0 0");

        // loads the new data
        loadFile();
    }

    //  ---------- read/write methods  ---------- 

    private void writeAppend(string text){
        ///<param> text : the text that will be added to the file </param>
        ///<summary> adds the text to the file (on a new line) </summary>
        
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(text);
        writer.Close();
    }
    
    private void writeOverwrite(string text){
        ///<param> text : the text that will be written to the file </param>
        ///<summary> puts the text to the file (replaces the old text) </summary>
        
        StreamWriter writer = new StreamWriter(filePath, false);
        writer.WriteLine(text);
        writer.Close();
    }
    
    private string read(){
        ///<summary> returns the text in the file </summary>
        
        StreamReader reader = new StreamReader(filePath);
        tmp = reader.ReadToEnd();
        reader.Close();

        return tmp;
    }
}
