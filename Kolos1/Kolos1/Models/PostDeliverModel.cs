using System.ComponentModel.DataAnnotations;

namespace Kolos1.Models;

public class PostDeliverModel
{
    [Required]
    public int deliveryId { get; set; }
    [Required]
    public int customerId { get; set; }
    [Required]
    public string licenceNumber { get; set; }
    [Required]
    public List<PostProductDetails> products { get; set; }
}

public class PostProductDetails
{
    public string name { get; set; }
    public int amount { get; set; }
}