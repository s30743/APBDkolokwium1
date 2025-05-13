using System.Data.Common;
using Kolos1.Exceptions;
using Kolos1.Models;
using Microsoft.Data.SqlClient;

namespace Kolos1.Services;

public class deliverService : IdeliverService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";
    
    public async Task<GetDeliversById> GetDeliversById(int id)
    {
        await using SqlConnection con = new SqlConnection(_connectionString);
        await using SqlCommand com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText =
            @"SELECT d.date,c.first_name,c.last_name,c.date_of_birth,dr.first_name,dr.last_name,dr.licence_number, p.name,p.price,PD.amount
        FROM Delivery d JOIN Customer C on d.customer_id = C.customer_id JOIN Driver dr ON d.driver_id = dr.driver_id JOIN Product_Delivery PD on d.delivery_id = PD.delivery_id
        JOIN Product P on PD.product_id = P.product_id where d.delivery_id = @id";
        com.Parameters.AddWithValue("@id", id);
        await con.OpenAsync();
        await using SqlDataReader reader = await com.ExecuteReaderAsync();
        GetDeliversById? result = null;
        while (await reader.ReadAsync())
        {
            if (result is null)
            {
                result = new GetDeliversById
                {
                    date = reader.GetDateTime(0),
                    customer = new CustomerDetails
                    {
                        firstName = reader.GetString(1),
                        lastName = reader.GetString(2),
                        dateOfBirth = reader.GetDateTime(3),
                    },
                    driver = new DriverDetails
                    {
                        firstName = reader.GetString(4),
                        lastName = reader.GetString(5),
                        licenceNumber = reader.GetString(6),
                    },
                };
            }
            var productName = reader.GetString(7);
            result.products.Add(new ProductDetails
            {
                name = productName,
                price = reader.GetDecimal(8),
                amount = reader.GetInt32(9),
            });
        }

        if (result is null)
        {
            throw new NotFoundEx("Nie istnieje dostawa o podanym Id ({id})");
        }
        return result;
    }

    public async Task addNewDeliver(PostDeliverModel deliver)
    {
        await using SqlConnection con = new SqlConnection(_connectionString);
        await using SqlCommand com = new SqlCommand();
        
        com.Connection = con;
        await con.OpenAsync();
        
        
        
        DbTransaction transaction = await con.BeginTransactionAsync();
        com.Transaction = transaction as SqlTransaction;

        try
        {
            com.Parameters.Clear();
            com.CommandText = @"Select count(*) from Delivery where delivery_id = @deliveryId";
            com.Parameters.AddWithValue("@deliveryId", deliver.deliveryId);
            var DeliverCheck = (int) await com.ExecuteScalarAsync();
            if (DeliverCheck > 0)
            {
                throw new ConflictEx($"ID podanej dostawy juz istnieje ({deliver.deliveryId})");
            }
            
            com.Parameters.Clear();
            com.CommandText = @"select count(*) from Customer where customer_id = @customerId";
            com.Parameters.AddWithValue("@customerId", deliver.customerId);
            var CustomerCheck = (int) await com.ExecuteScalarAsync();
            if (CustomerCheck == 0)
            {
                throw new NotFoundEx($"Klient o podanym ID nie istnieje ({deliver.customerId})");
            }
            
            com.Parameters.Clear();
            com.CommandText = @"select count(*) from Driver where licence_number = @licenceNumber";
            com.Parameters.AddWithValue("@licenceNumber", deliver.licenceNumber);
            var LicenceCheck = (int) await com.ExecuteScalarAsync();
            if (LicenceCheck == 0)
            {
                throw new NotFoundEx($"Dostawca o podanym numerze licencji nie istnieje ({deliver.licenceNumber})");
            }

            foreach (var product in deliver.products)
            {
                com.Parameters.Clear();
                com.CommandText = @"select count(*) from Product where name = @name";
                com.Parameters.AddWithValue("@name", product.name);
                var ProductCheck = (int) await com.ExecuteScalarAsync();

                if (ProductCheck == 0)
                {
                    throw new NotFoundEx($"Produkt {product.name} nie istnieje");
                }
                
            }
            
            com.Parameters.Clear();
            com.CommandText = @"INSERT INTO Delivery (delivery_id, customer_id, driver_id, date)
            VALUES (@deliveryId,@customerId,(SELECT driver_id FROM Driver WHERE licence_number = @licenceNumber),getdate())";
            com.Parameters.AddWithValue("@deliveryId", deliver.deliveryId);
            com.Parameters.AddWithValue("@customerId", deliver.customerId);
            com.Parameters.AddWithValue("@licenceNumber", deliver.licenceNumber);
            try
            {
                await com.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new ConflictEx(ex.Message);
            }

            foreach (var product in deliver.products)
            {
                com.Parameters.Clear();
                com.CommandText = @"INSERT INTO Product_Delivery (product_id, delivery_id, amount) 
                VALUES ((SELECT Product.product_id FROM Product where name = @name ), @deliveryId,@amount)";
                com.Parameters.AddWithValue("@deliveryId", deliver.deliveryId);
                com.Parameters.AddWithValue("@name", product.name);
                com.Parameters.AddWithValue("@amount", product.amount);
                await com.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}