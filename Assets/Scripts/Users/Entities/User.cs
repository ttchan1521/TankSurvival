using System;

namespace Users.Entities
{
    [Serializable]
    public class User 
    {
        public string username;
        public string email;
        public string password;
        public int score;
        public string _id;
    }
}