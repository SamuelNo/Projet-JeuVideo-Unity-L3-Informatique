using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

abstract public class Character : MonoBehaviour
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int lvl, maxHP, currentHP, maxMP, currentMP, baseAtk, teamID, mpCostSkillLvl1, mpCostSkillLvl2, mpCostSkillLvl3;
    [SerializeField] protected float dodgeProbability, damageMultiplier, weakenedMultiplier, strengthenedMultiplier;
    [SerializeField] protected AttackType attackType;
    [SerializeField] protected Weakness weakness;
    [SerializeField] private GameObject selectionCircle; 
    [SerializeField] private Color hoverColor = Color.yellow; 
    [SerializeField] private Color selectedColor = Color.orange;
    [SerializeField] protected List<(Status, int)> statusList = new List<(Status, int)>();
    [SerializeField] public StatBarHandler statBar;

    private SpriteRenderer circleRenderer; 
    private bool isSelected = false;

    private BattleUIController buttonScript;
    public GameObject textInfoPV;
    
    
    // ---------- Set and Get ---------- //

    public void setLvl(int n){ lvl = n; }
    public void setMaxHP(int n){ maxHP = n; }
    public void setCurrentHP(int n){ currentHP = n; }
    public void setMaxMP(int n){ maxMP = n; }
    public void setCurrentMP(int n){ currentMP = n; }
    public void setBaseAtk(int n){ baseAtk = n; }
    public void setDodgeProbability(float n){ dodgeProbability = n; } 
    public void setDamageMultiplier(float n){ damageMultiplier = n; } 
    public void setWeakenedMultiplier(float n){ weakenedMultiplier = n; } 
    public void setStrengthenedMultiplier(float n){ strengthenedMultiplier = n; } 
    public void setAttackType(AttackType t){ attackType = t; }
    public void setWeakness(Weakness w){ weakness = w; }
    public void setTeamID(int n){ teamID = n; }
    public void setMpCostSkillLvl1(int n){ mpCostSkillLvl1 = n; }
    public void setMpCostSkillLvl2(int n){ mpCostSkillLvl2 = n; }
    public void setMpCostSkillLvl3(int n){ mpCostSkillLvl3 = n; }

    public int getLvl(){ return lvl; }
    public int getMaxHP(){ return maxHP; }
    public int getCurrentHP(){ return currentHP; }
    public int getMaxMP(){ return maxMP; }
    public int getCurrentMP(){ return currentMP; }
    public int getBaseAtk(){ return baseAtk; }
    public int getTeamID(){ return teamID; }
    public int getMpCostSkillLvl1(){ return mpCostSkillLvl1; }
    public int getMpCostSkillLvl2(){ return mpCostSkillLvl2; }
    public int getMpCostSkillLvl3(){ return mpCostSkillLvl3; }
    public float getDodgeProbability(){ return dodgeProbability; }
    public float getDamageMultiplier(){ return damageMultiplier; }
    public float getWeakenedMultiplier(){ return weakenedMultiplier; }
    public float getStrengthenedMultiplier(){ return strengthenedMultiplier; }
    public AttackType getAttackType(){ return attackType; }
    public List<(Status, int)> getStatusList() {
    return statusList;
}


    // ---------- Methods ---------- //
    void Update()
    {
    if (textInfoPV != null)
    {
        Vector3 worldPos = transform.position + Vector3.up * 3.5f; 
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        textInfoPV.transform.position = screenPos;
    }
    }
    protected virtual void Awake() 
    {
        buttonScript = UnityEngine.Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);
        if (selectionCircle != null) {
            circleRenderer = selectionCircle.GetComponent<SpriteRenderer>();
            selectionCircle.SetActive(false); 
        }
    }
    protected void OnMouseEnter() {
    if (isSelected || circleRenderer == null || buttonScript.getCombatScript().getIsBattleOver()) return;
        
        selectionCircle.SetActive(true);
        circleRenderer.color = hoverColor;
    }

    protected void OnMouseExit() {
        if (isSelected || selectionCircle == null || buttonScript.getCombatScript().getIsBattleOver()) return;
        
        selectionCircle.SetActive(false);
    }
    public void OnMouseDown(){
        if(this.currentHP <= 0) {
            Debug.Log("Personnage mort, impossible de le sélectionner.");
            buttonScript.setWarningText("Personnage mort, impossible de le sélectionner.");
            return;
        }
        buttonScript = FindAnyObjectByType<BattleUIController>();
        if(buttonScript.getCombatScript().getIsBattleOver()) {
            Debug.Log("Le combat est terminé, impossible de sélectionner un personnage.");
            buttonScript.setWarningText("Le combat est terminé, impossible de sélectionner un personnage.");
            return;
        }
        if (buttonScript != null){
            isSelected = true;
            if (selectionCircle != null) {
                selectionCircle.SetActive(true);
                circleRenderer.color = selectedColor;
            }
            buttonScript.HandleSelection(this.gameObject);
        } else {
            Debug.LogException(new Exception("[Classe Character] BattleUIController not found in the scene.")); 
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
    
    public bool receiveDamage(int n){
        ///<param> n : amount of damage to be received by the character </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (UnityEngine.Random.value < dodgeProbability){
            Debug.Log("esquivé !");
            setTextInfoPV("Esquivé !");
            StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
            return false;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP <= 0){
            n += currentHP;
            Die();
        }
        if(currentHP > 0){
            Debug.Log( this.name+" a perdu "+n+"PV");
            setTextInfoPV("-"+n+"PV");
            StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
        }
        UpdateBars();
        return true;
    }
    

    public void receiveHeal(int n){
        ///<param> n : amount of HP to be received by the character </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
        Debug.Log( this.name+" a gagné "+n+"PV");
        setTextInfoPV("+"+n+"PV");
        StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV)); 
        UpdateBars();
    }

    public void useMP (int n){
        ///<param>  amount of MP used by the character </param> 
        ///<summary> Reduces n amount of MP </summary>

        if (currentMP >= n){
            currentMP -= n;
            UpdateBars();
        } else {
            Debug.LogException(new Exception("[Classe Character] There aren't enough magic points, this method should not be used")); 
        }
    }

    public void baseAttack(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculate the amount of damage and attacks the target </summary>

        // calculates damage (damage is increased if the healer's lvl2 skill is used)
        int damageReceived = (int) ((float) baseAtk * damageMultiplier); 

        // uses target's receiveDamage() to apply damage
        if (target.GetComponent<Character>() != null){ // if target is a character
            target.GetComponent<Character>().receiveDamage(damageReceived, attackType, false); 
        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            target.GetComponent<Enemy>().ReceiveDamage(damageReceived, attackType, false); 
        }
    }

    abstract public void receiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void skillLvl1(GameObject target);
    abstract public void skillLvl2(GameObject [] target);
    abstract public void skillLvl3(GameObject [] target);

}
