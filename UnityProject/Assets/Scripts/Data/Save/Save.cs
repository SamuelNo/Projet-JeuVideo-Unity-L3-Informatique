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
    void Awake()
    {
        filePath = Application.persistentDataPath + "/save.txt";
        principalScript = this.GetComponent<Principal>();

        if (principalScript == null)
        {
            principalScript = Object.FindAnyObjectByType<Principal>();
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "0 0");
            Debug.Log("Premier lancement : Fichier créé dans " + filePath);
        }

        loadFile();
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

    public void loadFile() {
        string rawData = read().Trim(); 
        if (string.IsNullOrEmpty(rawData)) return;

        data = rawData.Split(' ');
        Debug.Log("Données brutes lues : " + rawData);
        if (data.Length >= 2) {
            currentStage = int.Parse(data[0]);
            unlockedStage = int.Parse(data[1]);

            if (principalScript != null)
            {
                principalScript.setSelectedStage(currentStage);
                principalScript.setUnlockedStage(unlockedStage);
            }
            
            Debug.Log("Chargement réussi : Stage " + currentStage + " / Unlocked " + unlockedStage);
        }
    }

    public void save(){
        ///<summary> saves data into the file </summary>
        
        // saves data
        writeOverwrite(currentStage.ToString() +" "+ unlockedStage.ToString());
    }

    public void ResetSave(){
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
