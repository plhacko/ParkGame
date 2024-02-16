using System.Collections.Generic;
using UnityEngine;

public class CircleFormation : MonoBehaviour, IFormationType {
    private GameObject HorseRoot;
    private Formation formation;

    private List<GameObject> FormationCircleInner = new List<GameObject>();
    private List<GameObject> FormationCircleOuter = new List<GameObject>();
    private List<GameObject> FormationCircleForHorses = new List<GameObject>();

    public void Reset() {
        FormationCircleInner.Clear();
        FormationCircleOuter.Clear();
        FormationCircleForHorses.Clear();
    }

    public void RemoveFromFormation(GameObject soldier, GameObject position) {
        ISoldier.UnitType unitType = soldier.GetComponent<ISoldier>().GetUnitType();
        var positionList = FormationCircleOuter;
        var soldierList = formation.soldiersSwordmen; 

        if (unitType == ISoldier.UnitType.Pawn) {
            positionList = FormationCircleOuter;
        } else if (unitType == ISoldier.UnitType.Archer) {
            positionList = FormationCircleInner;
            soldierList = formation.soldiersArchers;
        } else {
            positionList = FormationCircleForHorses;
            soldierList = formation.soldiersMolemen;
        }
        formation.Remove(soldier, position, soldierList, positionList, true);
    }

    public void SetRoots(Formation f, GameObject hr, GameObject br) {
        formation = f;
        HorseRoot = hr;
    }

    private void Add1PositionToCircularFormation(Vector3 pos, List<GameObject> lst, GameObject parent) {
        var sphere = formation.AddSphere(pos, 0.2f, "CirclePos", parent);

        lst.Add(sphere);
    }

    // add one position to circle formation: inner or outer circle
    public GameObject GetPosition(ISoldier.UnitType unitType) {
        var positionList = FormationCircleOuter;
        GameObject parent = gameObject;

        if (unitType == ISoldier.UnitType.Archer) {
            positionList = FormationCircleInner;
        } else if (unitType == ISoldier.UnitType.Horseman) {
            positionList = FormationCircleForHorses;
            parent = HorseRoot;
        }

        var positions = ListCircularPositionsByUnitType(unitType);
        int freeCount = formation.GetNumberOfUnassignedPositions(positionList);
        if (freeCount < 1) {
            Add1PositionToCircularFormation(positions[positions.Count - 1], positionList, parent);
        }

        AdjustFormation(positionList, positions);

        foreach (var go in positionList) {
            var pos = go.GetComponent<PositionDescriptor>();
            if (pos.isAssigned) { continue; }
            pos.isAssigned = true;
            return go;
        }
        return null;

    }

    private List<Vector3> ListCircularPositionsByUnitType(ISoldier.UnitType unitType) {
        var listOfSoldiers = formation.soldiersSwordmen;
        float radius = 1.6f;
        float j = 0;
        if (unitType == ISoldier.UnitType.Archer) {
            listOfSoldiers = formation.soldiersArchers;
            radius = 0.8f;
            j = 0.5f;
        } else if (unitType == ISoldier.UnitType.Horseman) {
            listOfSoldiers = formation.soldiersMolemen;
            radius = 2.5f;
        }

        if (listOfSoldiers.Count < 1) { return null; }

        float alpha = 2 * Mathf.PI / listOfSoldiers.Count;

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < listOfSoldiers.Count; i++) {
            float x = transform.position.x - radius * Mathf.Cos(i * alpha + j);
            float y = transform.position.y - radius * Mathf.Sin(i * alpha + j);
            Vector3 vec = new Vector3(x, y, 0);
            positions.Add(vec);
        }

        return positions;
    }

    private List<GameObject> AdjustFormation(List<GameObject> formationList, List<Vector3> positions) {
        int j = 0;
        for (int i = 0; i < formationList.Count; i++) {
            if (j >= formationList.Count) {
                return formationList;
            }

            var a = formationList[i].transform.position;
            var b = positions[j];
            double eps = 0.002f;

            if (Vector3.Distance(a, b) <= eps) {
                // identical, moving on
                j++;
            } else {
                // not identical
                formationList[i].transform.position = b;
                j++;
            }
        }
        return formationList;
    }

}
