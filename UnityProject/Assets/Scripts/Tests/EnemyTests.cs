using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyTests
{
    GameObject monsterGameObject, bossGameObject;
    GameObject fighterGameObject, healerGameObject, protectorGameObject, mageGameObject;
    Enemy monster, boss;
    Character fighter, healer, protector, mage;

    [SetUp]
    public void SetUp()
    {
        // Create enemies
        monsterGameObject = new GameObject();
        bossGameObject = new GameObject();
        monster = monsterGameObject.AddComponent<Monster>();
        boss = bossGameObject.AddComponent<Boss>();

        // Create characters
        fighterGameObject = new GameObject();
        healerGameObject = new GameObject();
        protectorGameObject = new GameObject();
        mageGameObject = new GameObject();
        fighter = fighterGameObject.AddComponent<Fighter>();
        healer = healerGameObject.AddComponent<Healer>();
        protector = protectorGameObject.AddComponent<Protector>();
        mage = mageGameObject.AddComponent<Mage>();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean all created objects after each test

        // Enemies
        UnityEngine.Object.DestroyImmediate(monsterGameObject);
        UnityEngine.Object.DestroyImmediate(bossGameObject);

        // Characters
        UnityEngine.Object.DestroyImmediate(fighterGameObject);
        UnityEngine.Object.DestroyImmediate(healerGameObject);
        UnityEngine.Object.DestroyImmediate(protectorGameObject);
        UnityEngine.Object.DestroyImmediate(mageGameObject);
    }


    // -------- Monster Tests --------

    [Test]
    public void MonsterReceiveDamage()
    {
        //Monster takes normal damage

        // Declaration
        monster.MaxHP = 20;
        monster.CurrentHP = 20;
        monster.DodgeProbability = 0;

        // Test
        monster.ReceiveDamage(10);
        Assert.AreEqual(10, monster.CurrentHP);

    }

    [Test]
    public void MonsterReceiveDamageRes()
    {
        // Declaration
        monster.MaxHP = 20;
        monster.CurrentHP = 20;
        monster.DodgeProbability = 0;
        monster.Resistance = AttackType.Ranged;

        // Test
        // The monster has resistance -> damage should be reduced
        // (40% reduction -> 6 damage taken)
        monster.ReceiveDamage(10, AttackType.Ranged, false);
        Assert.AreEqual(14, monster.CurrentHP);
    }

    [Test]
    public void MonsterReceiveHeal()
    {
        // Declaration
        monster.MaxHP = 20;
        monster.CurrentHP = 10;

        // Test
        monster.ReceiveHeal(10);
        Assert.AreEqual(20, monster.CurrentHP);

        // Heal should not exceed MaxHP
        monster.ReceiveHeal(10);
        Assert.AreEqual(20, monster.CurrentHP);
    }

    [Test]
    public void MonsterTargetedAttack()
    {
        // Setup targets
        // target 1 : fighter
        fighter.setMaxHP(20);
        fighter.setCurrentHP(20);
        fighter.setDodgeProbability(0);
        fighter.setWeakenedMultiplier(2);

        // target 2 : healer
        healer.setMaxHP(20);
        healer.setCurrentHP(20);
        healer.setDodgeProbability(0);
        healer.setWeakenedMultiplier(2);

        // Setup monster
        monster.MaxHP = 20;
        monster.AttackTypeUsed = AttackType.Ranged;
        monster.ElementalAttack = false;

        // Test

        // -- Ranged attack --
        // fighter weak -> increased damage : monster.MaxHP * 0.1 * fighter.Weakness = 4HP should be removed  
        monster.TargetedAttack(fighterGameObject);
        Assert.AreEqual(16, fighter.getCurrentHP());

        // healer weak to ranged attack : monster.MaxHP * 0.1 * healer.Weakness = 4HP should be removed
        monster.TargetedAttack(healerGameObject);
        Assert.AreEqual(16, healer.getCurrentHP());



        // attack type of the monster changed -> melee attack type
        monster.AttackTypeUsed = AttackType.Melee;

        // -- Melee attack --
        // fighter not weak -> normal damage : monster.MaxHP * 0.1 = 2HP should be removed
        monster.TargetedAttack(fighterGameObject);
        Assert.AreEqual(14, fighter.getCurrentHP());

        // healer weak : monster.MaxHP * 0.1 * healer.Weakness = 4HP should be removed
        monster.TargetedAttack(healerGameObject);
        Assert.AreEqual(12, healer.getCurrentHP());

    }

    [Test]
    public void MonsterAoeAttack()
    {
        // List of targets
        GameObject[] target = new GameObject[2] { protectorGameObject, mageGameObject };

        // Protector
        protector.setMaxHP(30);
        protector.setCurrentHP(30);
        protector.setDodgeProbability(0);
        protector.setWeakenedMultiplier(2);

        // Mage
        mage.setMaxHP(30);
        mage.setCurrentHP(30);
        mage.setDodgeProbability(0);
        mage.setWeakenedMultiplier(2);

        // Monster setup
        monster.MaxHP = 100;
        monster.AttackTypeUsed = AttackType.Melee;
        monster.ElementalAttack = false;

        // Test
        // attack the protector and the mage
        // The protector isn't weak to melee attack : monster.MaxHP * 0.1 = 10HP should be removed
        // The mage's weak to melee attack : monster.MaxHP * 0.1 * mage.WeaknessMultiplier = 20HP should be removed
        monster.AoeAttack(target);
        Assert.AreEqual(20, protector.getCurrentHP());
        Assert.AreEqual(10, mage.getCurrentHP());
    }

    [Test]
    public void MonsterDodge()
    {
        // 100% dodge -> should take no damage
        monster.DodgeProbability = 100;
        monster.CurrentHP = 20;
        monster.ReceiveDamage(10);
        Assert.AreEqual(20, monster.CurrentHP); 
    }










    // -------- Boss Tests --------
    [Test]
    public void BossReceiveDamage()
    {
        // Declaration
        boss.MaxHP = 20;
        boss.CurrentHP = 20;
        boss.DodgeProbability = 0;

        // Test
        // Receive normal damage
        boss.ReceiveDamage(10);
        Assert.AreEqual(10, boss.CurrentHP);
    }

    [Test]
    public void BossReceiveDamageRes()
    {
        // Declaration
        boss.MaxHP = 20;
        boss.CurrentHP = 20;
        boss.DodgeProbability = 0;
        boss.Resistance = AttackType.Melee;

        // Test
        // Resistance reduce damage
        boss.ReceiveDamage(10, AttackType.Melee, false);
        Assert.AreEqual(16, boss.CurrentHP);
    }

    [Test]
    public void BossReceiveHeal()
    {
        // Declaration
        boss.MaxHP = 20;
        boss.CurrentHP = 10;

        // Test
        boss.ReceiveHeal(10);
        Assert.AreEqual(20, boss.CurrentHP);

        // Overheal check
        boss.ReceiveHeal(10);
        Assert.AreEqual(20, boss.CurrentHP);
    }

    [Test]
    public void BossDodge()
    {
        boss.DodgeProbability = 100;
        boss.CurrentHP = 20;
        boss.ReceiveDamage(10);
        Assert.AreEqual(20, boss.CurrentHP); // no damage should be taken
    }

    [Test]
    public void BossTargetedAttack()
    {
        // Setup targets
        // target 1 : protector
        protector.setMaxHP(30);
        protector.setCurrentHP(30);
        protector.setDodgeProbability(0);
        protector.setWeakenedMultiplier(1.5f);

        // target 2 : mage
        mage.setMaxHP(30);
        mage.setCurrentHP(30);
        mage.setDodgeProbability(0);
        mage.setWeakenedMultiplier(1.5f);

        // Setup boss
        boss.MaxHP = 70;
        boss.CurrentHP = 50;
        boss.AttackTypeUsed = AttackType.Melee;
        boss.ElementalAttack = false;

        // Tests

        // -- Melee attack --
        // protector not weak -> normal damage : (int)(boss.MaxHP * 0.15) = 10HP should be removed
        // mage weak -> increased damage : (int)(boss.MaxHP * 0.15 * mage.WeakenedMultiplier) = 15HP
        boss.TargetedAttack(protectorGameObject);
        Assert.AreEqual(20, protector.getCurrentHP());

        boss.TargetedAttack(mageGameObject);
        Assert.AreEqual(15, mage.getCurrentHP());



        // Elemental ranged attack
        // protector's weak to elemental attack
        // mage isn't
        boss.AttackTypeUsed = AttackType.Ranged;
        boss.ElementalAttack = true;

        boss.TargetedAttack(protectorGameObject);
        Assert.AreEqual(5, protector.getCurrentHP());

        boss.TargetedAttack(mageGameObject);
        Assert.AreEqual(5, mage.getCurrentHP());
    }

    [Test]
    public void BossAoeAttack()
    {
        // List of targets
        GameObject[] target = new GameObject[2] { fighterGameObject, healerGameObject };

        // Fighter
        fighter.setMaxHP(30);
        fighter.setCurrentHP(30);
        fighter.setDodgeProbability(0);
        fighter.setWeakenedMultiplier(2);


        // Healer
        healer.setMaxHP(30);
        healer.setCurrentHP(30);
        healer.setDodgeProbability(0);
        healer.setWeakenedMultiplier(2);

        // Boss setup
        boss.MaxHP = 80;
        boss.AttackTypeUsed = AttackType.Melee;
        boss.ElementalAttack = false;

        // Test
        // attack the fighter and the healer
        // The fighter isn't weak to melee attack : boss.MaxHP * 0.15 = 12HP should be removed
        // The mage's weak to melee attack : boss.MaxHP * 0.15 * healer.WeaknessMultiplier = 24HP should be removed
        boss.AoeAttack(target);
        Assert.AreEqual(18, fighter.getCurrentHP());
        Assert.AreEqual(6, healer.getCurrentHP());
    }

    [Test]
    public void BossSpecialAttack()
    {
        // target 1 : fighter
        fighter.setMaxHP(50);
        fighter.setCurrentHP(50);
        fighter.setDodgeProbability(0);
        fighter.setWeakenedMultiplier(1.5f);

        // target 2 : protector
        protector.setMaxHP(60);
        protector.setCurrentHP(60);
        protector.setDodgeProbability(0);
        protector.setWeakenedMultiplier(1.4f);

        // Boss setup
        Boss boss_var = boss as Boss;
        boss_var.MaxHP = 120;
        boss_var.AttackTypeUsed = AttackType.Melee;
        boss_var.ElementalAttack = false;

        //Special attack with only one target
        GameObject[] target = new GameObject[1] { protectorGameObject };
        boss_var.SpecialAttack(target);
        Assert.AreEqual(30, protector.getCurrentHP());

        // Multi-target + elemental
        boss_var.ElementalAttack = true;
        target = new GameObject[2] { fighterGameObject, protectorGameObject };
        boss_var.SpecialAttack(target);
        Assert.AreEqual(0, protector.getCurrentHP());
        Assert.AreEqual(20, fighter.getCurrentHP());
    }
}


