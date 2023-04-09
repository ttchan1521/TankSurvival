using System;
using System.Text;
using BestHTTP;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

namespace Leaderboard
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button closeButton;
        private Text _text;

        private void Start()
        {
            _text = scrollRect.content.GetComponent<Text>();
            closeButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
                Resources.UnloadUnusedAssets();
            });
        }

        private void OnEnable()
        {
            HTTPRequest request =
                new HTTPRequest(new Uri($"{NetworkManager.UriString}/users/leaderboard?page=1&perPage=10"),
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
                            Newtonsoft.Json.JsonConvert.DeserializeObject<Users.Entities.User[]>(resp.DataAsText);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (var user in users)
                        {
                            stringBuilder.AppendLine($"{user.username}: {user.score}");
                        }

                        _text.text = stringBuilder.ToString();
                    }
                    else
                    {
                        Debug.LogWarning(string.Format(
                            "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                            resp.StatusCode,
                            resp.Message,
                            resp.DataAsText));
                        _text.text = resp.DataAsText;
                        _text.color = Color.yellow;
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    _text.text = "Request Finished with Error!";
                    _text.color = Color.red;
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    _text.text = "Request Aborted!";
                    _text.color = Color.red;
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    _text.text = "Connection Timed Out!";
                    _text.color = Color.red;
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    _text.text = "Processing the request Timed Out!";
                    _text.color = Color.red;
                    break;
            }
        }
    }
}