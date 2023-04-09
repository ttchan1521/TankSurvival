using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Text userId;
        [SerializeField] private Button chatButton;
        [SerializeField] private Button leaderboardButton;

        private void Start()
        {
            userId.text = PlayerPrefsManager.UserId;
            chatButton.onClick.AddListener(() =>
            {
                var chat = Resources.Load<GameObject>("Chat");
                Instantiate(chat, GetComponentInParent<Canvas>().transform);
            });
            leaderboardButton.onClick.AddListener(() =>
            {
                var leaderboard = Resources.Load<GameObject>("Leaderboard");
                Instantiate(leaderboard, GetComponentInParent<Canvas>().transform);
            });
        }
    }
}