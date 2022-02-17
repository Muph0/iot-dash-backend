
using IotDash.Data.Model;
using IotDash.Utils.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IotDash.Services.Domain {


    interface IManagerColleciton<TEntity, TManager> : IDisposable, IEnumerable<TManager> where TManager : IDisposable {
        TManager Create(TEntity entity);
        void Discard(TEntity entity);
        bool HasManager(TEntity entity);
        TManager GetManager(TEntity entry);
        Func<TEntity, object> KeyExtractor { get; set; }
    }

    internal sealed class EntityManagerCollection<TEntity, TManager> : IManagerColleciton<TEntity, TManager>
            where TEntity : class
            where TManager : IDisposable {
        public Func<TEntity, TManager> ManagerFactory { get; }
        private readonly Dictionary<object, TManager> managers = new();

        public Func<TEntity, object> KeyExtractor { get; set; } = (entity) => entity;

        public EntityManagerCollection(Func<TEntity, TManager> managerFactory) {
            this.ManagerFactory = managerFactory;
        }

        /// <summary>
        /// Create a manager for the interface and add it to the collection.
        /// </summary>
        /// <param name="entry">The interface managed by the manager.</param>
        // <returns>The newly created manager</returns>
        public TManager Create(TEntity entry) {
            var manager = ManagerFactory(entry);
            var key = KeyExtractor(entry);
            managers.Add(key, manager);
            return manager;
        }
        /// <summary>
        /// Remove a manager from the collection and <see cref="IDisposable.Dispose"/> it.
        /// </summary>
        /// <param name="entry">The interface managed by the manager.</param>
        public void Discard(TEntity entry) {
            var key = KeyExtractor(entry);
            var manager = managers[key];
            manager.Dispose();
            managers.Remove(key);
        }

        /// <summary>
        /// Check if the <see cref="IotInterface"/> has manager in this collection.
        /// </summary>
        /// <param name="entry">The interface in question.</param>
        /// <returns>True if manager exists.</returns>
        public bool HasManager(TEntity entry) {
            var key = KeyExtractor(entry);
            return managers.ContainsKey(key);
        }

        public TManager GetManager(TEntity entry) {
            var key = KeyExtractor(entry);
            return managers[key];
        }

        public IEnumerator<TManager> GetEnumerator() => managers.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() {
            managers.Values.ForEach(w => w.Dispose());
            managers.Clear();
        }
    }

}