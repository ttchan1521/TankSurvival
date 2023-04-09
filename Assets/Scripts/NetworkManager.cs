using System;
using BestHTTP;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using UnityEngine;

namespace DefaultNamespace
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        public const string UriString = "http://localhost:3000";
        public SocketManager Manager { get; private set; }

        private void Start()
        {
            HTTPRequest request = new HTTPRequest(new Uri(UriString), OnRequestFinished);
            request.Send();

            Manager = new SocketManager(new Uri(UriString));
            Manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
            Manager.Socket.On<Error>(SocketIOEventTypes.Error, error => Debug.LogError(error.message));
            Manager.Socket.On(SocketIOEventTypes.Disconnect, () => Debug.Log("disconnect"));
            Manager.Socket.On("connecting", () => Debug.Log("connecting"));
            Manager.Socket.On("reconnect", () => Debug.Log("reconnect"));
            Manager.Socket.On("reconnecting", () => Debug.Log("reconnecting"));
        }

        void OnRequestFinished(HTTPRequest request, HTTPResponse response)
        {
            Debug.Log("Request Finished! Text received: " + response.DataAsText);
        }

        void OnConnected(ConnectResponse resp)
        {
            // Method 1: received as parameter
            Debug.Log("Sid through parameter: " + resp.sid);

            // Method 2: access through the socket
            Debug.Log("Sid through socket: " + Manager.Socket.Id);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Manager.Socket.Disconnect();
        }
    }
}