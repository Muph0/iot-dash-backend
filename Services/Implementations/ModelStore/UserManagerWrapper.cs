using IotDash.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IotDash.Services.ModelStore {
    internal class UserManagerWrapper : IUserStore {

        private readonly UserManager<IdentityUser> userManager;
        private readonly DataContext db;

        public UserManagerWrapper(UserManager<IdentityUser> userManager, DataContext db) {
            this.userManager = userManager;
            this.db = db;
        }

        public async Task<IReadOnlyList<IdentityUser>> GetAllAsync() {
            return (await db.Users.ToListAsync()).AsReadOnly();
        }

        public async Task<IdentityUser?> GetByKeyAsync(string userId) {
            return await userManager.FindByIdAsync(userId);
        }

        Task IModelStore<IdentityUser, string>.CreateAsync(IdentityUser entityToCreate) {
            var ex = new InvalidOperationException("Sorry, this operation is not possible.");
            Debug.Assert(false, ex.Message);
            throw ex;
        }
        public async Task<IdentityResult> CreateAsync(IdentityUser newUser) {
            return await userManager.CreateAsync(newUser);
        }
        public async Task<IdentityResult> CreateAsync(IdentityUser newUser, string password) {
            return await userManager.CreateAsync(newUser, password);
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser userToUpdate) {
            return await userManager.UpdateAsync(userToUpdate);
        }

        public async Task<IdentityResult> DeleteByIdAsync(string userId) {
            IdentityUser user = new() {
                Id = userId,
            };
            var result = await userManager.DeleteAsync(user);
            return result;
        }
        async Task<bool> IModelStore<IdentityUser, string>.DeleteByKeyAsync(string entityId) {
            return (await DeleteByIdAsync(entityId)).Succeeded;
        }

        public Task<bool> SaveChangesAsync() {
            return Task.FromResult(false);
        }

        public async Task<IdentityUser?> GetByNameAsync(string name) {
            return await userManager.FindByNameAsync(name);
        }

        public Task<bool> CheckPasswordAsync(IdentityUser user, string password) {
            return userManager.CheckPasswordAsync(user, password);
        }
    }
}