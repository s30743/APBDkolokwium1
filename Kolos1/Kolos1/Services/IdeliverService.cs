using Kolos1.Models;

namespace Kolos1.Services;

public interface IdeliverService
{
    Task<GetDeliversById> GetDeliversById(int id);
    Task addNewDeliver(PostDeliverModel deliver);
}