using IotDash.Data.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IotDash.Contracts.V1;

namespace IotDash.Services {

    /// <summary>
    /// Represents a contract for a store object that saves changes to database.
    /// </summary>
    public interface IModelSaver {

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation. The task result is false, when no entities were modified.</returns>
        Task<bool> SaveChangesAsync();
    }

    /// <summary>
    /// Represents a contract for a store object which provides CRUD operations
    /// on <typeparamref name="TEntity"/> indexed by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IModelStore<TEntity, TKey> : IModelSaver where TEntity : class {

        /// <summary>
        /// Retrieve all entities from the database.
        /// </summary>
        /// <returns>A read-only list of all entities.</returns>
        Task<IReadOnlyList<TEntity>> GetAllAsync();

        /// <summary>
        /// Get an entity from database by <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>An entity with the matching key or <c>null</c> if not found.</returns>
        Task<TEntity?> GetByKeyAsync(TKey key);

        /// <summary>
        /// Create an entity in the database.
        /// </summary>
        /// <param name="entityToCreate">A new entity to create.</param>
        /// <returns>A task that represents the asynchronous creation action. It resolves when action is completed.</returns>
        Task CreateAsync(TEntity entityToCreate);

        /// <summary>
        /// Deletes an entity from the databse.
        /// </summary>
        /// <param name="entityId">The identificator of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result is false, when no entity with such id exists.</returns>
        Task<bool> DeleteByKeyAsync(TKey entityId);
    }

    /// <summary>
    /// This service provides CRUD operations on interfaces.
    /// </summary>
    public interface IInterfaceStore : IModelStore<IotInterface, Guid> {
    }

    /// <summary>
    /// This service provides basic user account functionality.
    /// </summary>
    public interface IIdentityService {

        /// <summary>
        /// Register new user account, and authenticate the user immediately.
        /// </summary>
        /// <param name="username">Username of the new user.</param>
        /// <param name="password">Password of the new user.</param>
        /// <returns>Result of the authentication.</returns>
        Task<AuthResponse> RegisterAsync(string username, string password);

        /// <summary>
        /// Authenticate a user.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <param name="password">Password of the user.</param>
        /// <returns>Result of the authentication.</returns>
        Task<AuthResponse> LoginAsync(string username, string password);

        /// <summary>
        /// Authenticate a user from a valid refresh token.
        /// </summary>
        /// <param name="token">An expired JWT token of the user.</param>
        /// <param name="refreshToken">A valid refresh token belonging to the user.</param>
        /// <returns>Result of the authentication.</returns>
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);

        /// <summary>
        /// Delete expired or revoked refresh tokens from the database.
        /// </summary>
        /// <returns>Number of deleted tokens.</returns>
        Task<int> CleanupRefreshTokens();
    }

    /// <summary>
    /// This service provides CRUD operations on <see cref="IdentityUser"/> indexed by their ids.
    /// </summary>
    public interface IUserStore : IModelStore<IdentityUser, string> {

        /// <summary>
        /// Get a user with the given <paramref name="email"/>.
        /// </summary>
        /// <param name="email">The email to filter by.</param>
        /// <returns>The user or <c>null</c> if no such user exists.</returns>
        Task<IdentityUser?> GetByNameAsync(string email);

        /// <summary>
        /// Add a new user to the database with the given <paramref name="password"/>.
        /// </summary>
        /// <param name="newUser">The user to add.</param>
        /// <param name="password"></param>
        /// <returns>a result of the creation.</returns>
        Task<IdentityResult> CreateAsync(IdentityUser newUser, string password);

        /// <summary>
        /// Add a new user to the database.
        /// </summary>
        /// <param name="newUser">The user to add.</param>
        /// <returns>a result of the creation.</returns>
        new Task<IdentityResult> CreateAsync(IdentityUser newUser);

        /// <summary>
        /// Update a user with matching <c>id</c> to match this user.
        /// </summary>
        /// <param name="userToUpdate">The template user.</param>
        /// <returns></returns>
        Task<IdentityResult> UpdateAsync(IdentityUser userToUpdate);

        /// <summary>
        /// Check if the given <paramref name="password"/> matches with
        /// the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="password">The password to check.</param>
        /// <returns><c>true</c> if matches, <c>false</c> otherwise.</returns>
        Task<bool> CheckPasswordAsync(IdentityUser user, string password);
    }

    /// <summary>
    /// This service provides database operations on <see cref="HistoryEntry"/>s.
    /// </summary>
    public interface IHistoryStore : IModelSaver {

        /// <summary>
        /// Get a series of measurements over a continuous interval of time.
        /// </summary>
        /// <param name="iface">The interface which to which the measurements belong.</param>
        /// <param name="request">The parameters of this request.</param>
        /// <returns>An enumerable of the retrieved measurements.</returns>
        Task<IEnumerable<HistoryEntry>> GetPagedHistoryAsync(IotInterface iface, HistoryRequest request);

        /// <summary>
        /// Add new measurement to the database.
        /// </summary>
        /// <param name="entry">The measurement to add.</param>
        /// <returns>A task which represents the async operation.</returns>
        Task CreateAsync(HistoryEntry entry);
    }

}