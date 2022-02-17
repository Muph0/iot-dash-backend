using IotDash.Domain.Events;
using System;
using System.Threading.Tasks;



namespace IotDash.Services {
    internal interface IModelTracker {
        Task OnSaveChangesAsync(SaveChangesEventArgs args);
        Type EntityType { get; }
    }

    internal interface IModelTracker<TEntity> : IModelTracker where TEntity : class {
        Task OnSaveChangesAsync(SaveChangesEventArgs<TEntity> args);

        Task IModelTracker.OnSaveChangesAsync(SaveChangesEventArgs args) {
            return OnSaveChangesAsync(new SaveChangesEventArgs<TEntity>(args));
        }
        Type IModelTracker.EntityType => typeof(TEntity);
    }
}