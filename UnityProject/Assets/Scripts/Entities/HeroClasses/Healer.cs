using UnityEngine;
using System;
using UnityEngine.UI;

public class Healer : Character
{
    // ---------- Initialisation ---------- //
    protected override void Awake(){
        base.Awake();
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.All;
        this.gameObject.name = "Healer";
    }
    void Reset(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.All;
        this.gameObject.name = "Healer";
    }

    // ---------- Methods ---------- //

    override public void receiveDamage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the damage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack damage
        int damageReceived = attack;
        
        // character is weak to all attack types (except neutral ?), so damage worsens
        if (atkType != AttackType.Neutral){
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier); 
        }

        // if the attack is elemental, damage worsens
        if (elemental){
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier);
        }

        // applies damage (and tries dodging)
        receiveDamage(damageReceived);
    }

    override public void skillLvl1(GameObject target){
        ///<param> target : the target of the skill </param> 
        ///<summary> Restores a certain amount of HP to the target. For now, 10% of the target's maxHP will be restored </summary>
        
        // uses MP (10MP)
        useMP(mpCostSkillLvl1);

        // calcultates 10% of target's maxHP
        int healAmount = target.GetComponent<Character>().getMaxHP(); 
        healAmount = (int) ((float) healAmount * .1f);

        // heals target
        target.GetComponent<Character>().receiveHeal(healAmount);
    }

    override public void skillLvl2(GameObject [] target){
        ///<param> target : the targets of the attack </param> 
        ///<summary> strengthens the target </summary>
        if (target[0].GetComponent<Character>().getStatusList().Contains((Status.STRENGTHENED,1))){
            Debug.LogException(new Exception("[Classe Healer] The target is already strengthened"));
            buttonScript.setWarningText("Cible déjà renforcée");
            buttonScript.getCombatScript().setCurrentPhase(BattlePhase.SELECT_SKILL);
            buttonScript.ButtonAccess();
            return;
        }
        // uses MP (20MP)
        useMP(mpCostSkillLvl2);

        // applies skill
        target[0].GetComponent<Character>().getStatusList().Add((Status.STRENGTHENED,1));
        target[0].GetComponent<Character>().setDamageMultiplier(target[0].GetComponent<Character>().getDamageMultiplier()*1.5f); // multiplies applied damage by 2
    }

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the target of the skill </param> 
        ///<summary> Restores 50% of HP to the target and themselves </summary>
        
        // checks that only 1 taget is chosen (only 2 characters can fight in a battle, so 2 characters to heal : the ally and themselves)
        if (target.Length <1){
            Debug.LogException(new Exception("[Classe Healer] This skill has to target only 1 character"));
            return;
        }

        // uses MP (30MP)
        useMP(mpCostSkillLvl3);

        // heals themselves
        int healAmount = (int) ((float) this.maxHP * .5f);
        receiveHeal(healAmount);
        Character ally = target[0].GetComponent<Character>();
        // if there's an ally 
        if (ally != null){
            // calcultates 50% of ally's maxHP
            healAmount = target[0].GetComponent<Character>().getMaxHP(); 
            healAmount = (int) ((float) healAmount * .5f);

            // heals target
            target[0].GetComponent<Character>().receiveHeal(healAmount);
        }
        else {
            Debug.LogException(new Exception("[Classe Healer] The target of the skill should be an ally"));
        }
    }
}

