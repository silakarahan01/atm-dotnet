using ATM.Core.Entities;

namespace ATM.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
}
