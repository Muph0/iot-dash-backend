using IotDash.Data.Model;
using IotDash.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IotInterface = IotDash.Data.Model.IotInterface;
using IotDash.Contracts.V1;

namespace IotDash.Services {

    public interface IModelTracker {
        Task OnSaveChangesAsync(IEnumerable<EntityEntry> changed);
        Type EntityType { get; }
    }

    public interface IModelTracker<TEntity> : IModelTracker where TEntity : class {
        Task OnSaveChangesAsync(IEnumerable<EntityEntry<TEntity>> changed);

        Task IModelTracker.OnSaveChangesAsync(IEnumerable<EntityEntry> changed) {
            return OnSaveChangesAsync(changed.Select(e => e.Context.Entry((TEntity)e.Entity)));
        }
        Type IModelTracker.EntityType => typeof(TEntity);
    }

    public interface IModelStore<TEntity, TKey> where TEntity : class {

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

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation. The task result is false, when no entities were modified.</returns>
        Task<bool> SaveChangesAsync();
    }

    public interface IDeviceStore : IModelStore<IotDevice, Guid> {
        Task<IotDevice?> UserOwnsDeviceAsync(Guid userId, Guid deviceId);
    }

    public interface IInterfaceStore : IModelStore<IotInterface, (Guid, int)>, IModelTracker<IotInterface> {
        Task<IReadOnlyList<IotInterface>> GetAllByDeviceAsync(Guid deviceId);
    }

    public interface IIdentityService {
        Task<AuthResponse> RegisterAsync(string email, string password);
        Task<AuthResponse> LoginAsync(string email, string password);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<int> CleanupRefreshTokens();
    }

    public interface IUserStore : IModelStore<IdentityUser, string> {
        Task<IdentityUser?> GetByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(IdentityUser newUser, string password);
        new Task<IdentityResult> CreateAsync(IdentityUser newUser);
        Task<IdentityResult> UpdateAsync(IdentityUser userToUpdate);
        Task<bool> CheckPasswordAsync(IdentityUser user, string password);
    }

}