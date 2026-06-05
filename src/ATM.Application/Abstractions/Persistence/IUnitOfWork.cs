namespace ATM.Application.Abstractions.Persistence;

/// <summary>
/// Bir iş işleminin tüm değişikliklerini tek bir atomik commit'te kalıcı kılar.
/// Repository'ler kayıt yapmaz; commit sorumluluğu burada toplanır.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
