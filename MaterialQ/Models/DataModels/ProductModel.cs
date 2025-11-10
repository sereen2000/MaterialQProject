  using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaterialQ.Models.DataModels
{

    public class ProductsModel
        {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

            [Required]
            [StringLength(150)]
            public string Name { get; set; }           
            [StringLength(250)]
            public string Description { get; set; }   

            [Required]
            public decimal Price { get; set; }         

            [Required]
            public int UnitId { get; set; }            

            [StringLength(100)]
            public string UnitName { get; set; }        

            [Required]
            public int ColorId { get; set; }            

            [StringLength(100)]
            public string ColorName { get; set; }      

            public bool IsAvailable { get; set; } = true; 

            public decimal? DiscountPrice { get; set; } 
        }
    }


