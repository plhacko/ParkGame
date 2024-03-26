using System.Collections.Generic;
using UnityEngine;

public class BoxFormation : MonoBehaviour, IFormationType
{
    private GameObject BoxRoot;
    private Formation formation;

    public List<GameObject> FormationBox = new List<GameObject>(); // are in hierarchy under BoxRoot
    public List<GameObject> FormationBoxForHorses = new List<GameObject>(); // are in hierarchy under BoxRoot

    public void Reset() {
        FormationBox.Clear();
        FormationBoxForHorses.Clear();
        
        if (BoxRoot) {
            BoxRoot.GetComponent<FormationDescriptor>().NumberOfPositions = 0;
            BoxRoot.GetComponent<FormationDescriptor>().NumberOfHorsemenPositions = 0;
        }
    }

    public void SetRoots(Formation f, GameObject r) {
        formation = f;
        BoxRoot = r;
    }

    // rotate the box formation according to the commander's direction of movement
    private void RotateBoxFormation() {
        if (!BoxRoot) { return; }
        Vector3 from = BoxRoot.transform.up;
        Vector3 to = transform.position - BoxRoot.transform.position;

        float angle = Vector3.SignedAngle(from, to, BoxRoot.transform.forward);
        float distance = Vector3.Distance(transform.position, BoxRoot.transform.position);
        if (Mathf.Abs(angle) > 10 && distance > 0.9) { // otherwise don't rotate
            BoxRoot.transform.Rotate(0.0f, 0.0f, angle);
        }
        if (distance > 1) {
            Vector3 direction = (transform.position - BoxRoot.transform.position);
            BoxRoot.transform.position += direction * Time.deltaTime * 0.6f;
        }
    }

    public GameObject GetPosition(SoldierBase.UnitType unitType) {
        var positionList = FormationBox;
        int freeInBox = formation.GetNumberOfUnassignedPositions(FormationBox);
        int freeHorses = formation.GetNumberOfUnassignedPositions(FormationBoxForHorses);
        if (unitType != SoldierBase.UnitType.Molerider && freeInBox < 1) {
            Add1PositionToBoxFormation();
        }
        if (unitType == SoldierBase.UnitType.Molerider) {
            if (freeHorses < 1) {
                Add1PositionOnTheSide();
            }
            positionList = FormationBoxForHorses;
        }

        foreach (var go in positionList) {
            var pos = go.GetComponent<PositionDescriptor>();
            if (pos.isAssigned) { continue; }
            pos.isAssigned = true;
            return go;
        }
        return null;
    }

    private void Add1PositionOnTheSide() {
        var formDescr = BoxRoot.GetComponent<FormationDescriptor>();
        int c = formDescr.NumberOfHorsemenPositions;
        float horseinc = formDescr.HorseIncrement;
        var pos = formDescr.StartingPosition;
        float x = c % 2 == 1 ? -1 : 1;
        Vector3 position = new Vector3(x * horseinc, -c / 2 * horseinc / 2 + pos.y, 0);

        var sphere = formation.AddSphere(position, 0.2f, "BoxPosHorse", BoxRoot);
        FormationBoxForHorses.Add(sphere);
        formDescr.NumberOfHorsemenPositions++;
    }

    public void RemoveFromFormation(GameObject soldier, GameObject position) {
        SoldierBase.UnitType unitType = soldier.GetComponent<SoldierBase>().GetUnitType();
        var positionList = FormationBox;
        var soldierList = formation.soldiersSwordmen; 
        if (unitType == SoldierBase.UnitType.Molerider) {
            positionList = FormationBoxForHorses;
            soldierList = formation.soldiersMolemen;
        } else {
            positionList = FormationBox;
            if (unitType == SoldierBase.UnitType.Archer) {
                soldierList = formation.soldiersArchers;
            }
        }
        formation.Remove(soldier, position, soldierList, positionList, false);
    }

    private void Add1PositionToBoxFormation() {
        var formDescr = BoxRoot.GetComponent<FormationDescriptor>();
        int c = formDescr.NumberOfPositions;
        float inc = formDescr.Increment;
        var pos = formDescr.StartingPosition;
        Vector3 position = pos + new Vector3(-(c % 3) * inc, -c / 3 * inc, 0);

        var sphere = formation.AddSphere(position, 0.2f, "BoxPos", BoxRoot);
        FormationBox.Add(sphere);
        formDescr.NumberOfPositions++;
    }

    private void Update()
    {
        RotateBoxFormation();
    }
}
