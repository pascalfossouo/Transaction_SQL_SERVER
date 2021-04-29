using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ObjectsAffaire.Models
{
    [Table("Members")]
    public class Employee : BDObject
    {
        [Column("PKMEM")]
        public override int Id { get; set; }

        [Column("LastName")]
        public string Nom { get; set; }

        [Column("FirstName")]
        public string Prenom { get; set; }

        [Column("Title")]
        public string Poste { get; set; }
    }
}
