using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IotDash.Services.Domain {

    interface IEntityManagementService { }
    interface IEntityManagementService<TEntity, TManager> : IEntityManagementService, IDisposable, IMessageTarget<SaveChangesEventArgs<TEntity>>
            where TEntity : class
            where TManager : IDisposable {
        Task Refresh(IEnumerable<TEntity> entries);

    }

}