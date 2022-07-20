using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IotDash.Services.Domain {

    /// <summary>
    /// All implementations of this interface get installed by <see cref="Installers.MiscServiceInstaller"/>.
    /// </summary>
    interface IEntityManagementService { }

    /// <summary>
    /// Represents a contract for all services which monitor an entity and manage some objects in relation to that entity.
    /// </summary>
    interface IEntityManagementService<TEntity, TManager> : IEntityManagementService, IDisposable, IMessageTarget<SaveChangesEventArgs<TEntity>>
            where TEntity : class
            where TManager : IDisposable {

        /// <summary>
        /// Update the internal collection of managers.
        /// </summary>
        Task Refresh(IEnumerable<TEntity> entries);

    }

}