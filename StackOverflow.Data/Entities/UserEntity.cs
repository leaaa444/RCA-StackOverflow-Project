using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Entities
{
    public class UserEntity : TableEntity
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        [IgnoreProperty]
        public Pol Pol { get; set; }
        public string PolAsString
        {
            get { return Pol.ToString(); }
            set { Pol = (Pol)Enum.Parse(typeof(Pol), value); }
        }
        public string Drzava { get; set; }
        public string Grad { get; set; }
        public string Adresa { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; } 
        public string SlikaUrl { get; set; }

        public UserEntity(string email)
        {
            PartitionKey = "User"; 
            RowKey = email;
            Email = email;
        }

        public UserEntity() { }
    }
}
