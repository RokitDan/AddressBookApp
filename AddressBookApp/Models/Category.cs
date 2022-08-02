﻿

using System.ComponentModel.DataAnnotations;

namespace AddressBookApp.Models
{

    public class Category
    {   //naming this property "Id" makes it a primary ket by default
        public int Id { get; set; }

        [Required]
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();



    }
}
