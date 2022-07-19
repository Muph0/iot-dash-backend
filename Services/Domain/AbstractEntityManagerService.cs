using IotDash.Data.Model;
using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using IotDash.Services.History;
using IotDash.Services.Messaging;
using IotDash.Services.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDash.Services.Domain {

    /// <summary>
    /// An abstract database entity manager.
    /// It listens for changes of the database and keeps internal collection of <b>TManager</b> per <b>TEntity</b>.
    /// </summary>
    /// <typeparam name="TEntity">Type of the database entity.</typeparam>
    /// <typeparam name="TManager">Type of the manager object.</typeparam>
    internal abstract class AEntityManagerService<TEntity, TManager> : IEntityManagementService<TEntity, TManager>
            where TEntity : class
            where TManager : IDisposable {

        protected readonly ILogger logger;
        protected readonly IManagerColleciton<TEntity, TManager> managers;
        protected bool DisposedValue => disposedValue;
        private bool disposedValue;

        public AEntityManagerService(ILogger logger) {
            this.logger = logger;
            this.managers = new EntityManagerCollection<TEntity, TManager>(this.ManagerFactory);
            this.managers.KeyExtractor = this.GetKey;
        }

        public abstract Task OnReceive(object? sender, SaveChangesEventArgs<TEntity> msg);
        public abstract TManager ManagerFactory(TEntity entity);
        public abstract bool NeedsManager(TEntity entity);
        public abstract object GetKey(TEntity entity);

        protected delegate Task AfterManagerUpHandler(TEntity entity, TManager manager);
        protected delegate Task BeforeManagerDownHandler(TEntity entity, TManager manager);

        protected event AfterManagerUpHandler AfterManagerUp;
        protected event BeforeManagerDownHandler BeforeManagerDown;

        /// <summary>
        /// Go through all entries and re-instantiate manager for that entry.
        /// </summary>
        /// <param name="entries">All entries</param>
        public async Task Refresh(IEnumerable<TEntity> entries) {
            StringBuilder message = new($"Refreshing managers ({typeof(TManager).Name}):");

            try {
                foreach (var entry in entries) {
                    bool managerExists = managers.HasManager(entry);
                    bool managerShouldExist = NeedsManager(entry);

                    message.Append($"\n    {{{entry}}}");

                    // re-instantiate each manager if needed
                    if (managerExists) {
                        message.Append(" discard");

                        if (BeforeManagerDown != null)
                            await BeforeManagerDown.Invoke(entry, managers.GetManager(entry));

                        managers.Discard(entry);
                    }
                    if (managerExists && managerShouldExist) message.Append(",");
                    if (managerShouldExist) {
                        message.Append(" create");

                        var mgr = managers.Create(entry);

                        if (AfterManagerUp != null)
                            await AfterManagerUp.Invoke(entry, mgr);
                    }
                }
            } finally {
                logger.LogDebug(message.ToString());
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    managers.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}

