namespace Kolos1.Models;

public class GetDeliversById
{
    public DateTime date { get; set; }
    public CustomerDetails customer { get; set; }
    public DriverDetails driver { get; set; }
    public List<ProductDetails> products { get; set; } = [];
    
}

public class CustomerDetails
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
    
}

public class DriverDetails
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string licenceNumber { get; set; }
    
}

public class ProductDetails
{
    public string name { get; set; }
    public decimal price { get; set; }
    public int amount { get; set; }
}