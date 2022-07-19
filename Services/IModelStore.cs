using IotDash.Data.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IotDash.Contracts.V1;

namespace IotDash.Services {

    public interface IModelSaver {

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation. The task result is false, when no entities were modified.</returns>
        Task<bool> SaveChangesAsync();
    }

    public interface IModelStore<TEntity, TKey> : IModelSaver where TEntity : class {

        /// <summary>
        /// Retrieve all entities from the database.
        /// </summary>
        /// <returns>A read-only list of all entities.</returns>
        Task<IReadOnlyList<TEntity>> GetAllAsync();
        Task<TEntity?> GetByKeyAsync(TKey entityId);
        Task CreateAsync(TEntity entityToCreate);

        /// <summary>
        /// Deletes an entity from the databse.
        /// </summary>
        /// <param name="entityId">The identificator of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result is false, when no entity with such id exists.</returns>
        Task<bool> DeleteByKeyAsync(TKey entityId);
    }

    public interface IInterfaceStore : IModelStore<IotInterface, Guid> {
        
    }

    public interface IIdentityService {
        Task<AuthResponse> RegisterAsync(string username, string password);
        Task<AuthResponse> LoginAsync(string username, string password);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<int> CleanupRefreshTokens();
    }

    public interface IUserStore : IModelStore<IdentityUser, string> {
        Task<IdentityUser?> GetByNameAsync(string email);
        Task<IdentityResult> CreateAsync(IdentityUser newUser, string password);
        new Task<IdentityResult> CreateAsync(IdentityUser newUser);
        Task<IdentityResult> UpdateAsync(IdentityUser userToUpdate);
        Task<bool> CheckPasswordAsync(IdentityUser user, string password);
    }

    public interface IHistoryStore : IModelSaver {

        Task<IEnumerable<HistoryEntry>> GetPagedHistoryAsync(IotInterface iface, HistoryRequest request);
        Task CreateAsync(HistoryEntry entry);
    }

}