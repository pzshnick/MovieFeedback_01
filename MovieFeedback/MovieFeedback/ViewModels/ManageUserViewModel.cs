﻿namespace MovieFeedback.ViewModels
{
    public class ManageUserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsBanned { get; set; }

    }
}
