using System;

namespace Users.Entities
{
    [Serializable]
    public class User 
    {
        public int rank;
        public string username;
        public int score;
        public string _id;
    }

    public class UpdateScore
    {
        public int score;
        public string userId;
        public string mode;
        public string op;
    }
}