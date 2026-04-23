using UnityEngine;
using System.IO;

public class Save : MonoBehaviour
{
    // ---------- Attributes  ---------- 
    private string filePath, tmp;
    private Principal principalScript;

    // saved data
    private string[] data;
    private int unlockedStage;

    //  ---------- Initialisation  ---------- 
    void Start(){
        filePath = Application.dataPath + "/Scripts/Data/Save/save.txt";
        principalScript = this.GetComponent<Principal>();

        loadFile();
        reset();
    }

    //  ----------  Methods  ---------- 

    public void loadFile(){
        ///<summary> loads the data saved in the file </summary>
        
        // loads data from the file
        data = read().Split(" ");
        
        // unlocked stage number
        unlockedStage = int.Parse(data[0]);
        principalScript.setUnlockedStage(unlockedStage);

        // other data
        // ...
    }

    public void save(){
        ///<summary> saves data into the file </summary>
        
        // unlocked stage number
        unlockedStage = principalScript.getUnlockedStage();
        writeOverwrite(unlockedStage.ToString());

        // other data
        // ...
    }

    public void reset(){
        ///<summary> resets all the data in the file and loads it </summary>
        
        // unlocked stage number
        writeOverwrite("0");

        // other data
        // ...


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
