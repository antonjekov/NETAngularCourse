﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace API.Entities
{
    public class AppUser: IdentityUser<int>
    {
        public AppUser()
        {
            this.Photos= new HashSet<Photo>();
            this.LikedByUsers = new HashSet<UserLike>();
            this.LikedUsers = new HashSet<UserLike>();
            this.MessagesSent = new HashSet<Message>();
            this.MessagesReceived = new HashSet<Message>();
            this.UserRoles = new HashSet<AppUserRole>();
        }
        public DateTime BirthDate { get; set; }

        public string KnownAs { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime LastActive { get; set; } = DateTime.Now;

        public string Gender { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public ICollection<Photo> Photos { get; set; }

        public ICollection<UserLike> LikedByUsers { get; set; }

        public ICollection<UserLike> LikedUsers { get; set; }

        public ICollection<Message> MessagesSent { get; set; }

        public ICollection<Message> MessagesReceived { get; set; }

        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}
