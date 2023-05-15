using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NumberPicker : MonoBehaviour
    {
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private TextMeshProUGUI numberText;
        
        public int Number
        {
            get => number;
            private set
            {
                number = value;
                numberText.text = value.ToString();
            }
        }

        private int number;
        private int min;
        private int max;

        private void Awake()
        {
            SetRange(2, 4);
            minusButton.onClick.AddListener(decrement);
            plusButton.onClick.AddListener(increment);
        }

        public void SetRange(int min, int max)
        {
            this.min = min;
            this.max = max;
            Number = Mathf.Min(Mathf.Max(min, Number), max);
            updateIsInteractable();
        }
        
        public void SetInteractable(bool interactable)
        {
            plusButton.interactable = interactable;
            minusButton.interactable = interactable;
            if (interactable)
            {
                updateIsInteractable();   
            }
        }

        private void decrement()
        {
            Number--;
            updateIsInteractable();
        }

        private void increment()
        {
            Number++;
            updateIsInteractable();
        }

        private void updateIsInteractable()
        {
            minusButton.interactable = Number != min;
            plusButton.interactable = Number != max;
        }
    }
}