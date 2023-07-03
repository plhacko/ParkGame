using Managers;
using UnityEngine;
using UnityEngine.UI;

/*
 * This class is responsible for the UI of the Game menu.
 * Players can go back to the Join Game menu from here.
 * It's still possible to reconnect to the game from there.
 */
namespace UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private string joinGameSceneName;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            backButton.onClick.AddListener(() => SessionManager.Singleton.EndSessionAndGoToScene(joinGameSceneName));
        }
    }
}
