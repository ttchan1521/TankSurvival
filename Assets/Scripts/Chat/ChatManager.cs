using System.Collections;
using System.Text;
using Chat.DTO;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

namespace Chat
{
    public class ChatManager : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        private NetworkManager _networkManager;
        private StringBuilder _stringBuilder;

        private void Awake()
        {
            _networkManager = NetworkManager.Instance;
            button.onClick.AddListener(CreateChat);
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() =>
                _networkManager.Manager != null && _networkManager.Manager.Socket.IsOpen);
            FindAllChat();
            _networkManager.Manager.Socket.On<Entities.Chat>("newChat", AppendChat);
            _networkManager.Manager.Socket.On<string>("handleConnection",
                sid => _stringBuilder.AppendLine($"{sid} connect"));
            _networkManager.Manager.Socket.On<string>("handleDisconnect",
                sid => _stringBuilder.AppendLine($"{sid} disconnect"));
        }

        private void FindAllChat()
        {
            _networkManager.Manager.Socket
                .ExpectAcknowledgement<Entities.Chat[]>(chats =>
                {
                    _stringBuilder = new StringBuilder();
                    foreach (var chat in chats)
                    {
                        AppendChat(chat);
                    }
                })
                .Emit("findAllChat", new ListChat());
        }

        private void CreateChat()
        {
            _networkManager.Manager.Socket
                .ExpectAcknowledgement<Entities.Chat>(chat =>
                {
                    inputField.text = string.Empty;
                    AppendChat(chat);
                })
                .Emit("createChat", new CreateChat()
                {
                    username = _networkManager.Manager.Socket.Id,
                    message = inputField.text
                });
        }

        private void AppendChat(Entities.Chat chat)
        {
            _stringBuilder.AppendLine($"[{chat.createdAt.DateTime}]");
            _stringBuilder.AppendLine($"{chat.username}: {chat.message}");
            scrollRect.content.GetComponent<Text>().text = _stringBuilder.ToString();
        }
    }
}