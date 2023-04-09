using System;

namespace Users.DTO
{
    [Serializable]
    public class CreateUser
    {
        public string username;
        public string email;
        public string password;
    }
    
    [Serializable]
    public class CreateUserMessage
    {
        public string[] message;
    }
}