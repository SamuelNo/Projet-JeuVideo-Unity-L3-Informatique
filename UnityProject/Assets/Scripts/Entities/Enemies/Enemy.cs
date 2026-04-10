using UnityEngine;

abstract public class Enemy : MonoBehaviour 
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int maxHP, currentHP, maxMP, currentMP, teamId;
    [SerializeField] protected float dodgeProbability;
    [SerializeField] protected AttackType resistance, attackTypeUsed;
    [SerializeField] protected bool elementalAttack;

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



    // ---------- Methods ---------- //

    public void OnMouseDown()
    {
         if(this.currentHP <= 0) {
            Debug.Log("Ennemi mort, impossible de le sélectionner.");
            return;
        }
        BattleUIController controller = FindAnyObjectByType<BattleUIController>();
        if (controller != null)
        {
            controller.HandleSelection(this.gameObject);
        }
        else
        {
            Debug.LogError("BattleUIController not found in the scene.");
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

    public void ReceiveDamage(int n){
        ///<param> n : amount of damage to be received by the enemy </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (Random.value < dodgeProbability){
            Debug.Log("Esquivé !");
            return;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP < 0){
            n += currentHP;
            Die();
        }
        Debug.Log("L'adversaire a perdu "+n+"PV");
    }

    public void ReceiveHeal(int n){
        ///<param> n : amount of HP to be received by the enemy </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
    }
    
    abstract public void ReceiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void TargetedAttack(GameObject target);
    abstract public void AoeAttack(GameObject [] target);

}