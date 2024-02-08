using UnityEngine;
using UnityEngine.UI;

public class ToggleSwitchIcon : MonoBehaviour
{
    [SerializeField] GameObject iconOn;
    [SerializeField] GameObject iconOff;
    private bool state;
    private void Start() {
        state = true;
        iconOn.GetComponent<Image>().enabled = true;
        iconOff.GetComponent<Image>().enabled = false;
    }

    public bool Toggle() {
        state = !state;
        iconOn.GetComponent<Image>().enabled = state;
        iconOff.GetComponent<Image>().enabled = !state;
        return state;
    }
}
