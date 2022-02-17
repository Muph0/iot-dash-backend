using IotDash.Services.Messaging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Domain.Events {
    internal class SaveChangesEventArgs {
        public virtual IEnumerable<EntityEntry> Entries { get; }
        public IServiceProvider Scope { get; }

        public SaveChangesEventArgs(IEnumerable<EntityEntry> changed, IServiceProvider scope) {
            Entries = changed;
            Scope = scope;
        }
    }

    internal sealed class SaveChangesEventArgs<TEntity> where TEntity : class {
        public IEnumerable<EntityEntry<TEntity>> Entries { get; }
        public IServiceProvider Scope { get; }

        public SaveChangesEventArgs(IEnumerable<EntityEntry<TEntity>> entries, IServiceProvider scope) {
            Entries = entries;
            Scope = scope;
        }

        public SaveChangesEventArgs(IEnumerable<EntityEntry> entries, IServiceProvider scope) {
            Entries = entries.Select(e => e.Context.Entry((TEntity)e.Entity));
            Scope = scope;
        }

        public SaveChangesEventArgs(SaveChangesEventArgs args) {
            Entries = args.Entries.Select(e => e.Context.Entry((TEntity)e.Entity));
            Scope = args.Scope;
        }
    }

    internal static class SaveChangesEventArgsExtension {
        public static Task SendSaveChangesEventArgs(this IMessageMediator mediator, Type TEntity, object sender, IEnumerable<EntityEntry> entries, IServiceProvider scope) {
            var eventType = typeof(Domain.Events.SaveChangesEventArgs<>).MakeGenericType(TEntity);
            object? message = Activator.CreateInstance(eventType, entries, scope);
            Debug.Assert(message != null);

            return mediator.Send(eventType, sender, message);
        }
    }

}
