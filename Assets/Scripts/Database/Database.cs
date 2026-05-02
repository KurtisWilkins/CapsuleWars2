using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Database", menuName = "UnitBuilder/Database")]
public class Database : ScriptableObject
{
    [SerializeField] public UnitObject_SO[] unitObjects;
    [SerializeField] public UnitBody_SO[] unitBodys;
    [SerializeField] public UnitEye_SO[] unitEyes;
    [SerializeField] public UnitHair_SO[] unitHair;
    [SerializeField] public UnitEquipment_SO[] unitEquipment;
    [SerializeField] public ElementType_SO[] elementTypes;
    [SerializeField] public UnitClass_SO[] classTypes;
    [SerializeField] public MovementTargeting[] movementTargetings;
    [SerializeField] public Ability_SO[] baseAbilities;
    [SerializeField] public Ability_SO[] passiveAbilities;
    [SerializeField] public Evolve_SO[] evolutions;
    [SerializeField] public Rune_SO[] runes;
    [SerializeField] public Rarity_SO[] rarity;
    [SerializeField] public BattleFeild_SO[] battleFeilds;
    [SerializeField] public BattleWeather_SO[] battleWeather;
    [SerializeField] public StatusEffects_SO[] statusEffects;




    private void OnValidate()
    {
        ValidateUnitArmatures();
        ValidateUnitBodies();
        ValidateUnitEyes();
        ValidateUnitHair();
        ValidateUnitEquipment();
        ValidateElementTypes();
        ValidateClassTypes();
        ValidateMovementTargeting();
        ValidateBaseAbilities();
        ValidatePassiveAbilities();
        ValidateRunes();
        ValidateRarity();
        ValidateBattleFeilds();
        ValidateBattleWeather();
        ValidateStatusEffects();
        ValidateEvolutions();
    }

    public void ValidateUnitArmatures()
    {
        for (int i = 0; i < unitObjects.Length; i++)
        {
            unitObjects[i].armatureID = i;
        }
    }

    public void ValidateUnitBodies()
    {
        for (int i = 0; i < unitBodys.Length; i++)
        {
            unitBodys[i].BodyID = i;
        }
    }

    public void ValidateUnitEyes()
    {
        for (int i = 0; i < unitEyes.Length; i++)
        {
            unitEyes[i].eyeID = i;
        }
    }

    public void ValidateUnitHair()
    {
        for (int i = 0; i < unitHair.Length; i++)
        {
            unitHair[i].hairID = i;
        }
    }

    public void ValidateUnitEquipment()
    {
        for (int i = 0; i < unitEquipment.Length; i++)
        {
            unitEquipment[i].equipmentID = i;
        }
    }

    public void ValidateElementTypes()
    {
        for (int i = 0; i < elementTypes.Length; i++)
        {
            elementTypes[i].elementTypeID = i;
        }
    }

    public void ValidateClassTypes()
    {
        for (int i = 0; i < classTypes.Length; i++)
        {
            classTypes[i].classTypeID = i;
        }
    }

    public void ValidateMovementTargeting()
    {
        for (int i = 0; i < movementTargetings.Length; i++)
        {
            movementTargetings[i].targetingID = i;
        }
    }

    public void ValidateBaseAbilities()
    {
        for (int i = 0; i < baseAbilities.Length; i++)
        {
            baseAbilities[i].abilityID = i;
        }
    }

    public void ValidatePassiveAbilities()
    {
        for (int i = 0; i < passiveAbilities.Length; i++)
        {
            passiveAbilities[i].abilityID = i;
        }
    }

    public void ValidateRunes()
    {
        for (int i = 0; i < runes.Length; i++)
        {
            runes[i].runeID = i;
        }
    }

    public void ValidateRarity()
    {
        for (int i = 0; i < rarity.Length; i++)
        {
            rarity[i].rarityID = i;
        }
    }

    public void ValidateBattleFeilds()
    {
        for (int i = 0; i < battleFeilds.Length; i++)
        {
            battleFeilds[i].battleGroundID = i;
        }
    }

    public void ValidateBattleWeather()
    {
        for (int i = 0; i < battleWeather.Length; i++)
        {
            battleWeather[i].battleWeatherID = i;
        }
    }

    public void ValidateStatusEffects()
    {
        for (int i = 0; i < statusEffects.Length; i++)
        {
            statusEffects[i].id = i;
        }
    }
    
    public void ValidateEvolutions()
    {
        for (int i = 0; i < evolutions.Length; i++)
        {
            evolutions[i].evolveID = i;
        }
    }
}
