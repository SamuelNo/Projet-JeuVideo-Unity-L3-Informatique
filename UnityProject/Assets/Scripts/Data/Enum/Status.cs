using UnityEngine;

public enum Status {
    FROZEN,
    PROTECTED,
    SHIELDED,
    STRENGTHENED
}

/*
Mage lvl2 : makes the target skip 1 turn (frozen)       [DONE]
Protector lvl1 : takes on damage in place of the target for 1 turn (protected)      [DONE]
Protector lvl3 : shields all allies for 1 turn, can’t be used 2 time in a row (shielded)
Fighter lvl2 : enhances personal damage for 1 turn
Healer lvl2 : enhances the target’s damage for 2 turns (strengthened)
*/