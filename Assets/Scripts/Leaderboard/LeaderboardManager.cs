using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Users.Entities;

namespace Leaderboard
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button campaignBtn;
        [SerializeField] private Button pvpBtn;
        private List<LeaderboardItem> items = new List<LeaderboardItem>();
        private GameObject content;
        //private Text _text;

        private void Start()
        {
            //_text = scrollRect.content.GetComponent<Text>();
            content = scrollRect.content.gameObject;
            closeButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
                Resources.UnloadUnusedAssets();
            });

            campaignBtn.onClick.RemoveAllListeners();
            campaignBtn.onClick.AddListener(() => SelectMode(0));

            pvpBtn.onClick.RemoveAllListeners();
            pvpBtn.onClick.AddListener(() => SelectMode(1));
        }

        private void OnEnable()
        {
            SelectMode(0);
        }

        private void SelectMode(int mode)
        {
            ResetItem();
            Uri uri = null;
            if (mode == 0)
            {
                campaignBtn.GetComponent<RectTransform>().localScale = new Vector3(1, 1.2f, 1);
                pvpBtn.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                uri = new Uri($"{NetworkManager.UriString}/users/leaderboard?page=1&perPage=10&mode=campaign");

            }
            else if (mode == 1)
            {
                campaignBtn.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                pvpBtn.GetComponent<RectTransform>().localScale = new Vector3(1, 1.2f, 1);
                uri = new Uri($"{NetworkManager.UriString}/users/leaderboard?page=1&perPage=10&mode=pvp");
            }
            HTTPRequest request = new HTTPRequest(uri,
                    OnRequestFinished);
            request.Send();
            
        }

        private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        // Everything went as expected!
                        Debug.Log(resp.DataAsText);
                        var users =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<User[]>(resp.DataAsText);
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int i = 0; i < users.Length; i++)
                        {
                            FillContent(i, users[i]);
                        }

                        //_text.text = stringBuilder.ToString();
                    }
                    else
                    {
                        Debug.LogWarning(string.Format(
                            "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                            resp.StatusCode,
                            resp.Message,
                            resp.DataAsText));
                        //_text.text = resp.DataAsText;
                        //_text.color = Color.yellow;
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    //_text.text = "Request Finished with Error!";
                    //_text.color = Color.red;
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    //_text.text = "Request Aborted!";
                    //_text.color = Color.red;
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    //_text.text = "Connection Timed Out!";
                    //_text.color = Color.red;
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    //_text.text = "Processing the request Timed Out!";
                    //_text.color = Color.red;
                    break;
            }
        }

        private void FillContent(int index, User user)
        {
            var item = GetItem(index);
            item.UpdateView(user.rank.ToString(), user.username, user.score.ToString());
        }

        private LeaderboardItem GetItem(int index)
        {
            if (index >= items.Count)
            {
                var prefab = Resources.Load<LeaderboardItem>("LeaderboardItem");
                var newItem = Instantiate(prefab, content.transform);
                items.Add(newItem);
                return newItem;
            }
            items[index].gameObject.SetActive(true);
            return items[index];
        }

        private void ResetItem()
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}