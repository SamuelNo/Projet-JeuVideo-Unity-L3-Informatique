using System.Collections;
using UnityEngine;
using System.Collections.Generic;

abstract public class Enemy : MonoBehaviour 
{
    // ---------- Attributes ---------- //

    [SerializeField] protected Rank rank;
    [SerializeField] protected int maxHP, currentHP, maxMP, currentMP, teamId, basePower;
    [SerializeField] protected float dodgeProbability;
    [SerializeField] protected AttackType resistance, attackTypeUsed;
    [SerializeField] protected bool elementalAttack;
    [SerializeField] private GameObject selectionCircle; 
    [SerializeField] private Color hoverColor = Color.yellow; 
    [SerializeField] private Color selectedColor = Color.orange;
    [SerializeField] public StatBarHandler statBar;
    [SerializeField] private string nameText;
    [SerializeField] protected EnemyType type;


    private SpriteRenderer circleRenderer; 
    private bool isSelected = false;

    private BattleUIController buttonScript;
    public GameObject textInfoPV;
    public Vector3 offset = new Vector3(0, 3.5f, 0); 

    protected List<(Status,int)> statusList;

    // AI
    protected EnemyAI ai;
    protected int costAOE, costSpecial, costHeal, costBoost, costProtection;
    [SerializeField] protected bool isPhase2 = false;
    [SerializeField] protected float buffAttack = 1.0f;
    [SerializeField] protected bool isProtected = false;
    [SerializeField] protected bool isBuffed = false;

    // Number of turns remaining before the temporary effect is removed
    [SerializeField] protected int buffTurnsRemaining = 0;
    [SerializeField] protected int protectionTurnsRemaining = 0;

    // ---------- Set and Get ---------- //

    public Rank Rank { get => rank; set => rank = value; }
    public int BasePower { get => basePower; set => basePower = value; }
    public EnemyType Type { get => type; set => type = value; }

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
    public StatBarHandler getStatBar() { return statBar; }
    public GameObject getTextInfoPV() { return textInfoPV; }
    public EnemyAI AI { get => ai; set => ai = value; }
    public bool IsPhase2 { get => isPhase2; set => isPhase2 = value; }
    public float BuffAttack { get => buffAttack; set => buffAttack = value; }
    public bool IsProtected { get => isProtected; set => isProtected = value; }
    public bool IsBuffed { get => isBuffed; set => isBuffed = value; }

    public int CostAOE { get => costAOE; set => costAOE = value; }
    public int CostSpecial { get => costSpecial; set => costSpecial = value; }
    public int CostHeal { get => costHeal; set => costHeal = value; }
    public int CostBoost { get => costBoost; set => costBoost = value; }
    public int CostProtection { get => costProtection; set => costProtection = value; }
    // ---------- Methods ---------- //
    void Update()
    {
    if (textInfoPV != null){    
        Vector3 worldPos = transform.position + Vector3.up * offset.y; 
        
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
        statusList = new List<(Status,int)>();
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
        }/*
        setTextInfoPV("Mort !");
        StartCoroutine(ClearTextAfterDelay(2.0f, textInfoPV));*/
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
    public float GetRankMultiplier()
    {
        ///<summary> A multiplier that will be applied to the base attack depending on the enemy's rank </summary>
        switch (rank)
        {
            case Rank.C:
                return 0.9f;
            case Rank.B:
                return 1.2f;
            case Rank.A:
                return 1.5f;
            case Rank.S:
                return 2.0f;
            default:
                return 1.0f;
        }
    }

    public int GetMPCost(int baseCost)
    {
        float multiplier = 1.0f;

        if (IsPhase2)
        {
            multiplier = 0.5f;
        }
        return (int)(baseCost * GetRankMultiplier() * multiplier);
    }
    public void ReceiveDamage(int n){
        ///<param> n : amount of damage to be received by the enemy </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        int damage = n;

        // tries to dodge the attack 
        if (Random.value < dodgeProbability){
            Debug.Log("Esquivé !");
            setTextInfoPV("Esquivé !");
            StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
            return;
        }

        if (isProtected)
        {
            damage = (int)(n - 0.20f * n * GetRankMultiplier());
            Debug.Log(gameObject.name + " est protégé.");
            Debug.Log(gameObject.name + " a réduit les dégâts à " + damage);
        }

        // if dodge fails, damage is taken
        currentHP -= damage;
        if (currentHP < 0){
            damage += currentHP;
            Die();
        }
        if(currentHP > 0){
            Debug.Log("L'adversaire a perdu "+damage+"PV");
            setTextInfoPV("-"+damage+"PV");
            StartCoroutine(ClearTextAfterDelay(3.0f, textInfoPV));
        }
        UpdateBars();
        return;
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
        UpdateBars();
    }

    public void Heal(GameObject target)
    {
        ///<param> target : the target of the skill </param> 
        ///<summary> Restores a certain amount of HP to the target : 10% of the enemy's maxHP will be restored </summary>

        UseMp(GetMPCost(costHeal));

        int healAmount = this.MaxHP;
        healAmount = (int)((float)healAmount * 0.1f * GetRankMultiplier());

        // heals target
        target.GetComponent<Enemy>().ReceiveHeal(healAmount);
    }

    public void BoostAttack(GameObject target)
    {
        ///<param> target : the target of the skill </param>
        ///<summary> Boosts the target's attack for the next turn</summary>
        UseMp(GetMPCost(costBoost));

        Enemy enemyTarget = target.GetComponent<Enemy>();
        enemyTarget.IsBuffed = true;

        float boostAmount = 1.0f;
        boostAmount += 0.1f * GetRankMultiplier();
        target.GetComponent<Enemy>().BuffAttack = boostAmount;
        enemyTarget.buffTurnsRemaining = 2;
    }

    public void Protection(GameObject target)
    {
        ///<param> target : the target of the skill </param> 
        ///<summary> Protects the target for the next turn : 20% of the damage taken will be reduced for the next turn </summary>

        UseMp(GetMPCost(costBoost));
        // protects target
        target.GetComponent<Enemy>().IsProtected = true;

        target.GetComponent<Enemy>().protectionTurnsRemaining = 2;

        Debug.Log(gameObject.name + " a protégé " + target.name);
    }

    public void EndTurnConsumeTemporaryEffects()
    {
        if (buffTurnsRemaining > 0)
        {
            buffTurnsRemaining--;
            if (buffTurnsRemaining == 0)
            {
                IsBuffed = false;
                // If it's a boss in phase 2 : buffAttack = 1.5
                BuffAttack = (IsPhase2) ? 1.5f : 1f;
            }
        }

        if (protectionTurnsRemaining > 0)
        {
            protectionTurnsRemaining--;
            if (protectionTurnsRemaining == 0)
            {
                IsProtected = false;
            }
        }
    }

    public void UseMp(int amount)
    {
        if (CurrentMP < amount)
        {
            Debug.Log(gameObject.name + " : pas assez de MP.");
            return;
        }
        CurrentMP -= amount;
        UpdateBars();
    }

    abstract public void ReceiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void TargetedAttack(GameObject target);
    abstract public void AoeAttack(GameObject [] target);

}