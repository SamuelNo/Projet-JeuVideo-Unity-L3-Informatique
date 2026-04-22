using System.Collections;
using UnityEngine;
using System.Collections.Generic;

abstract public class Enemy : MonoBehaviour 
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int maxHP, currentHP, maxMP, currentMP, teamId;
    [SerializeField] protected float dodgeProbability;
    [SerializeField] protected AttackType resistance, attackTypeUsed;
    [SerializeField] protected bool elementalAttack;
    [SerializeField] private GameObject selectionCircle; 
    [SerializeField] private Color hoverColor = Color.yellow; 
    [SerializeField] private Color selectedColor = Color.orange;
    [SerializeField] public StatBarHandler statBar;
    [SerializeField] private string nameText;
    

    private SpriteRenderer circleRenderer; 
    private bool isSelected = false;

    private BattleUIController buttonScript;
    public GameObject textInfoPV;

    protected List<(Status,int)> statusList;

    // ---------- Set and Get ---------- //

    public int MaxHP { get => maxHP;  set => maxHP = value; }
    public int CurrentHP { get => currentHP;  set => currentHP = value; }
    public int MaxMP { get => maxMP;  set => maxMP = value; }
    public int CurrentMP { get => currentMP;  set => currentMP = value; }

    public int TeamId { get => teamId;  set => teamId = value; }

    public float DodgeProbability { get => dodgeProbability;  set => dodgeProbability = value; } 
    public AttackType Resistance{ get => resistance;  set => resistance = value; }
    public AttackType AttackTypeUsed { get => attackTypeUsed;  set => attackTypeUsed = value; }
    public bool ElementalAttack { get => elementalAttack;  set => elementalAttack = value; }

    public void setStatusList(List<(Status,int)> list){ statusList = list; }
    public List<(Status,int)> getStatusList(){ return statusList; }

    // ---------- Methods ---------- //
    void Update()
    {
    if (textInfoPV != null){    
        Vector3 worldPos = transform.position + Vector3.up * 3.5f; 
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
    
        textInfoPV.transform.position = screenPos;
    }
    }
    protected virtual void Awake() 
    {   this.gameObject.name = nameText; // default name, can be changed in inspector
        buttonScript = UnityEngine.Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);
        if (selectionCircle != null) {
            circleRenderer = selectionCircle.GetComponent<SpriteRenderer>();
            selectionCircle.SetActive(false);
        }
    }
    protected void OnMouseEnter() {
    if (isSelected || circleRenderer == null) return;
        
        selectionCircle.SetActive(true);
        circleRenderer.color = hoverColor;
    }

    protected void OnMouseExit() {
        if (isSelected || selectionCircle == null) return;
        
        selectionCircle.SetActive(false);
    }
    public void OnMouseDown()
    {
         if(this.currentHP <= 0) {
            Debug.Log("Ennemi mort, impossible de le sélectionner.");
            buttonScript.setWarningText("Ennemi mort, impossible de le sélectionner.");
            return;
        }
        BattleUIController controller = FindAnyObjectByType<BattleUIController>();
        if (controller != null)
        {
            isSelected = true;
            if (selectionCircle != null) {
                selectionCircle.SetActive(true);
                circleRenderer.color = selectedColor;
            }
            controller.HandleSelection(this.gameObject);
        }
        else
        {
            Debug.LogError("BattleUIController not found in the scene.");
        }
    }
    public void Deselect() {
        isSelected = false;
        if (selectionCircle != null) {
            selectionCircle.SetActive(false);
        }
    }

    private void Die()
    {
        currentHP = 0;

        if (GetComponent<Collider2D>() != null) 
        {
            GetComponent<Collider2D>().enabled = false;
        }
        setTextInfoPV("Mort !");
        StartCoroutine(ClearTextAfterDelay(2.0f, textInfoPV));
        Destroy(gameObject, 0.1f); 
        
        Debug.Log(gameObject.name + " a été supprimé de la scène.");
    }

   public void setTextInfoPV(string text) 
    {

        textInfoPV.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
    }
    private IEnumerator ClearTextAfterDelay(float delay, GameObject textObject) 
    {
        yield return new WaitForSeconds(delay);
        setTextInfoPV(""); // Clear the text after the delay 
    }
    public void UpdateBars() {
    if (statBar != null) {
        // Remplace par tes vrais noms de variables (ex: pv, pvMax...)
        statBar.SetValues(currentHP, maxHP, currentMP, maxMP);
    }
}

    public void ReceiveDamage(int n){
        ///<param> n : amount of damage to be received by the enemy </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (Random.value < dodgeProbability){
            Debug.Log("Esquivé !");
            setTextInfoPV("Esquivé !");
            StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
            return;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP < 0){
            n += currentHP;
            Die();
        }
        UpdateBars();
        Debug.Log("L'adversaire a perdu "+n+"PV");
        setTextInfoPV("-"+n+"PV");
        StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
    }

    public void ReceiveHeal(int n){
        ///<param> n : amount of HP to be received by the enemy </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
        setTextInfoPV("+"+n+"PV");
        StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
    }
    
    abstract public void ReceiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void TargetedAttack(GameObject target);
    abstract public void AoeAttack(GameObject [] target);

}