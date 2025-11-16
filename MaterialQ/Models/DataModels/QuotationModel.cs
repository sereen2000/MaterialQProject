using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaterialQ.Models.DataModels
{
    public class QuotationModel
        {
            [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string QuotationNumber { get; set; }  

            [Required(ErrorMessage = "Company name is required")]
            [StringLength(200)]
            public string CustomerName { get; set; }      

            [Required]
            public DateTime DateCreated { get; set; } = DateTime.Now; 

            [StringLength(200)]
            public string CreatedBy { get; set; }        

            public decimal TotalAmount { get; set; }      

            public decimal Discount { get; set; }         

            public decimal NetAmount { get; set; }        

            [StringLength(50)]
            public string Status { get; set; }
        public List<QuotationItemsModel> Items { get; set; } = new();

    }


}


