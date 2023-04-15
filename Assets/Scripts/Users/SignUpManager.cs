using System;
using System.Collections;
using BestHTTP;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Users.DTO;

namespace Users
{
    public class SignUpManager : MonoBehaviour
    {
        [SerializeField] private InputField username;
        [SerializeField] private InputField email;
        [SerializeField] private InputField password;
        [SerializeField] private Button button;
        [SerializeField] private Text status;

        private void Start()
        {
            status.text = string.Empty;
            button.onClick.AddListener(CreateUser);
        }

        private void Interactable(bool interactable)
        {
            username.interactable = interactable;
            email.interactable = interactable;
            password.interactable = interactable;
            button.interactable = interactable;
        }

        private void CreateUser()
        {
            var user = new CreateUser()
            {
                username = username.text,
                email = email.text,
                password = password.text
            };
            HTTPRequest request = new HTTPRequest(new Uri($"{NetworkManager.UriString}/users"), HTTPMethods.Post,
                OnRequestFinished);
            request.SetHeader("Content-Type", "application/json; charset=UTF-8");
            Debug.Log(JsonUtility.ToJson(user));
            request.RawData = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(user));
            request.Send();
            Interactable(false);
        }

        private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            Interactable(!(req.State == HTTPRequestStates.Finished && resp.IsSuccess));
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        // Everything went as expected!
                        Debug.Log(resp.DataAsText);
                        status.text = "Successfully";
                        status.color = Color.green;
                        PlayerPrefsManager.UserId = JsonUtility.FromJson<Entities.User>(resp.DataAsText)._id;
                        StartCoroutine(DestroyYield(3));
                        
                    }
                    else
                    {
                        Debug.LogWarning(string.Format(
                            "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                            resp.StatusCode,
                            resp.Message,
                            resp.DataAsText));
                        var message = JsonUtility.FromJson<CreateUserMessage>(resp.DataAsText).message;
                        foreach (var s in message)
                        {
                            status.text = s;
                            status.color = Color.yellow;
                            break;
                        }
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    status.text = "Request Finished with Error!";
                    status.color = Color.red;
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    status.text = "Request Aborted!";
                    status.color = Color.red;
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    status.text = "Connection Timed Out!";
                    status.color = Color.red;
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    status.text = "Processing the request Timed Out!";
                    status.color = Color.red;
                    break;
            }
        }

        private IEnumerator DestroyYield(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
            TDS.OnSignUp(true);
        }
    }
}