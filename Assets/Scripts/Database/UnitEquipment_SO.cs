using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "UnitBuilder/Equipment")]
public class UnitEquipment_SO : ScriptableObject
{
    [SerializeField] public int equipmentID;
    [SerializeField] public List<SpriteHolder> equipmentSpriteR;
    [SerializeField] public List<SpriteHolder> equipmentSpriteL;
    [SerializeField] public Color equipmentColor;
    [SerializeField] public EquipmentSlot equipmentSlot;
    [SerializeField] public ItemType itemType;
    [SerializeField] public CoveredBodyParts coveredBody;
    [SerializeField] public bool hasSprite = true;
}

[System.Serializable]
public class CoveredBodyParts
{
    [SerializeField] public bool coverChest = false;
    [SerializeField] public bool coverHair = false;
    [SerializeField] public bool coverFace = false;
}

[System.Serializable]
public class SpriteHolder 
{
    [SerializeField] public List<Sprite> equipmentSprites;
}

public enum ItemType
{
    Axe,
    Bomb,
    Book,
    Bow,
    Claw,
    Crossbow,
    Dagger,
    Equipment,
    FlintLockRifle,
    Lantern,
    Mace,
    Polearm,
    Potion,
    Sheild,
    Spear,
    Staff,
    Standard,
    Sword
}

public enum EquipmentSlot
{
    EquipmentSlotHelmet,
    EquipmentSlotChest,
    EquipmentSlotRightHand,
    EquipmentSlotLeftHand,
    EquipmentSlotArms,
    EquipmentSlotShoulders,
    EquipmentSlotBack,
    EquipmentSlotLegs
}
