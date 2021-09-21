using IotDash.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Domain {

    public class SaveChangesEventArgs {
        public IEnumerable<EntityEntry> Changed { get; }
        public IServiceProvider Scope { get; }

        public SaveChangesEventArgs(IEnumerable<EntityEntry> changed, IServiceProvider scope) {
            Changed = changed;
            Scope = scope;
        }
    }
    public class SaveChangesEventArgs<TEntity> where TEntity : class {
        public IEnumerable<EntityEntry<TEntity>> Changed { get; }
        public IServiceProvider Scope { get; }

        public SaveChangesEventArgs(IEnumerable<EntityEntry<TEntity>> changed, IServiceProvider scope) {
            Changed = changed;
            Scope = scope;
        }

        public SaveChangesEventArgs(SaveChangesEventArgs args) {
            Changed = args.Changed.Select(e => e.Context.Entry((TEntity)e.Entity));
            Scope = args.Scope;
        }
    }
}


namespace IotDash.Services {
    public interface IModelTracker {
        Task OnSaveChangesAsync(SaveChangesEventArgs args);
        Type EntityType { get; }
    }

    public interface IModelTracker<TEntity> : IModelTracker where TEntity : class {
        Task OnSaveChangesAsync(SaveChangesEventArgs<TEntity> args);

        Task IModelTracker.OnSaveChangesAsync(SaveChangesEventArgs args) {
            return OnSaveChangesAsync(new SaveChangesEventArgs<TEntity>(args));
        }
        Type IModelTracker.EntityType => typeof(TEntity);
    }

}