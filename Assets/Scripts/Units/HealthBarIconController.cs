using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarIconController : MonoBehaviour
{
    [SerializeField] public UnitStatusController unitStatusController;
    [SerializeField] public GameObject iconProtected;
    [SerializeField] public GameObject iconShield;
    [SerializeField] public GameObject iconStunned;
    [SerializeField] public GameObject iconFrozen;
    [SerializeField] public GameObject iconTrapped;
    [SerializeField] public GameObject iconMarked;
    [SerializeField] public GameObject iconUnLucky;
    [SerializeField] public GameObject iconLastStand;
    [SerializeField] public GameObject iconMadness;
    [SerializeField] public GameObject iconCursed;
    [SerializeField] public GameObject iconAttackBoosted;
    [SerializeField] public GameObject iconAttackBroken;
    [SerializeField] public GameObject iconDefenseBoosted;
    [SerializeField] public GameObject iconDefenseBroken;
    [SerializeField] public GameObject iconSpeedBoosted;
    [SerializeField] public GameObject iconSpeedBroken;
    [SerializeField] public GameObject iconAccuracyBoosted;
    [SerializeField] public GameObject iconAccuracyBroken;
    [SerializeField] public GameObject iconCritRateBoosted;
    [SerializeField] public GameObject iconCritRateBroken;
    [SerializeField] public GameObject iconCritDamageBoosted;
    [SerializeField] public GameObject iconCritDamageBroken;
    [SerializeField] public GameObject iconResistenceBoosted;
    [SerializeField] public GameObject iconResistenceBroken;
    // Start is called before the first frame update
    void Start()
    {
        unitStatusController.onIsProtected += SetIconProtected;
        unitStatusController.onIsShield += SetIconShield;
        unitStatusController.onIsStunned += SetIconStunned;
        unitStatusController.onIsFrozen += SetIconFrozen;
        unitStatusController.onIsTrapped += SetIconTrapped;
        unitStatusController.onIsMarked += SetIconMarked;
        unitStatusController.onIsUnLucky += SetIconUnLucky;
        unitStatusController.onIsLastStand += SetIconLastStand;
        unitStatusController.onIsMadness += SetIconMadness;
        unitStatusController.onIsCursed += SetIconCursed;
        unitStatusController.onIsAttackBoosted += SetIconAttackBoosted;
        unitStatusController.onIsAttackBroken += SetIconAttackBroken;
        unitStatusController.onIsDefenseBoosted += SetIconDefenseBoosted;
        unitStatusController.onIsDefenseBroken += SetIconDefenseBroken;
        unitStatusController.onIsSpeedBoosted += SetIconSpeedBoosted;
        unitStatusController.onIsSpeedBroken += SetIconSpeedBroken;
        unitStatusController.onIsAccuracyBoosted += SetIconAccuracyBoosted;
        unitStatusController.onIsAccuracyBroken += SetIconAccuracyBroken;
        unitStatusController.onIsCritRateBoosted += SetIconCritRateBoosted;
        unitStatusController.onIsCritRateBroken += SetIconCritRateBroken;
        unitStatusController.onIsCritDamageBoosted += SetIconCritDamageBoosted;
        unitStatusController.onIsCritDamageBroken += SetIconCritDamageBroken;
        unitStatusController.onIsResistenceBoosted += SetIconResistenceBoosted;
        unitStatusController.onIsResistenceBroken += SetIconResistenceBroken;
    }

    void OnDestroy()
    {
        unitStatusController.onIsProtected -= SetIconProtected;
        unitStatusController.onIsShield -= SetIconShield;
        unitStatusController.onIsStunned -= SetIconStunned;
        unitStatusController.onIsFrozen -= SetIconFrozen;
        unitStatusController.onIsTrapped -= SetIconTrapped;
        unitStatusController.onIsMarked -= SetIconMarked;
        unitStatusController.onIsUnLucky -= SetIconUnLucky;
        unitStatusController.onIsLastStand -= SetIconLastStand;
        unitStatusController.onIsMadness -= SetIconMadness;
        unitStatusController.onIsCursed -= SetIconCursed;
        unitStatusController.onIsAttackBoosted -= SetIconAttackBoosted;
        unitStatusController.onIsAttackBroken -= SetIconAttackBroken;
        unitStatusController.onIsDefenseBoosted -= SetIconDefenseBoosted;
        unitStatusController.onIsDefenseBroken -= SetIconDefenseBroken;
        unitStatusController.onIsSpeedBoosted -= SetIconSpeedBoosted;
        unitStatusController.onIsSpeedBroken -= SetIconSpeedBroken;
        unitStatusController.onIsAccuracyBoosted -= SetIconAccuracyBoosted;
        unitStatusController.onIsAccuracyBroken -= SetIconAccuracyBroken;
        unitStatusController.onIsCritRateBoosted -= SetIconCritRateBoosted;
        unitStatusController.onIsCritRateBroken -= SetIconCritRateBroken;
        unitStatusController.onIsCritDamageBoosted -= SetIconCritDamageBoosted;
        unitStatusController.onIsCritDamageBroken -= SetIconCritDamageBroken;
        unitStatusController.onIsResistenceBoosted -= SetIconResistenceBoosted;
        unitStatusController.onIsResistenceBroken -= SetIconResistenceBroken;

    }

    public void SetIconProtected(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconProtected.SetActive(value);
    }

    public void SetIconShield(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconShield.SetActive(value);
    }

    public void SetIconStunned(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconStunned.SetActive(value);
    }

    public void SetIconFrozen(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconFrozen.SetActive(value);
    }

    public void SetIconTrapped(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconTrapped.SetActive(value);
    }

    public void SetIconMarked(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconMarked.SetActive(value);
    }

    public void SetIconUnLucky(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconUnLucky.SetActive(value);
    }

    public void SetIconLastStand(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconLastStand.SetActive(value);
    }

    public void SetIconMadness(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconMadness.SetActive(value);
    }

    public void SetIconCursed(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconCursed.SetActive(value);
    }

    public void SetIconAttackBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconAttackBoosted.SetActive(value);
    }
    public void SetIconAttackBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconAttackBroken.SetActive(value);
    }
    public void SetIconDefenseBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconDefenseBoosted.SetActive(value);
    }
    public void SetIconDefenseBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconDefenseBroken.SetActive(value);
    }
    public void SetIconSpeedBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconSpeedBoosted.SetActive(value);
    }
    public void SetIconSpeedBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconSpeedBroken.SetActive(value);
    }
    public void SetIconAccuracyBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconAccuracyBoosted.SetActive(value);
    }
    public void SetIconAccuracyBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconAccuracyBroken.SetActive(value);
    }
    public void SetIconCritRateBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconCritRateBoosted.SetActive(value);
    }
    public void SetIconCritRateBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconCritRateBroken.SetActive(value);
    }
    public void SetIconCritDamageBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconCritDamageBoosted.SetActive(value);
    }
    public void SetIconCritDamageBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconCritDamageBroken.SetActive(value);
    }
    public void SetIconResistenceBoosted(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconResistenceBoosted.SetActive(value);
    }
    public void SetIconResistenceBroken(bool value, UnitStatusController attacker, UnitStatusController self)
    {
        iconResistenceBroken.SetActive(value);
    }

}
