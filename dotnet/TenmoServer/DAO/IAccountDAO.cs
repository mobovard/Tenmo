using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDAO
    {
        Account GetAccount(int userId);
        void UpdateBalance(int accountId, decimal balance);
    }
}
