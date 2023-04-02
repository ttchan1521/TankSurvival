using System;

namespace Chat.Entities
{
    public class Chat
    {
        public string username;
        public string message;
        public DateTimeOffset createdAt;
        public DateTimeOffset updatedAt;
    }
}