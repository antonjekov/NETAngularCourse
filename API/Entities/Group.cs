using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Group
    {
        public Group()
        {
            this.Connections = new HashSet<Connection>();
        }

        public Group(string name)
        {
            Name = name;
            this.Connections = new HashSet<Connection>();
        }

        [Key]
        public string Name { get; set; }

        public ICollection<Connection> Connections { get; set; }
    }
}
