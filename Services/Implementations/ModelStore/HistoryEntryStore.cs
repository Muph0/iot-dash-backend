using IotDash.Contracts.V1;
using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services.ModelStore {

    internal class HistoryEntryStore : IHistoryStore {

        private readonly DataContext db;

        public HistoryEntryStore(DataContext db) {
            this.db = db;
        }

        public async Task<IEnumerable<HistoryEntry>> GetPagedHistoryAsync(IotInterface iface, HistoryRequest request) {

            if (request.PointCount.HasValue) {
                // recalculate points
                double secondSize = (request.ToUTC - request.FromUTC).TotalSeconds / request.PointCount.Value;

                var aggregated = from ent in db.History
                                 where ent.WhenUTC >= request.FromUTC && ent.WhenUTC <= request.ToUTC && ent.InterfaceId == iface.Id
                                 group ent by new {
                                     SecondScaled = Math.Floor(
                                        (EF.Functions.DateDiffDay(DateTime.UnixEpoch, ent.WhenUTC) * 24.0 * 3600.0
                                        + ent.WhenUTC.Hour * 3600.0
                                        + ent.WhenUTC.Minute * 60.0
                                        + ent.WhenUTC.Second
                                        + ent.WhenUTC.Millisecond / 1000.0)
                                     / secondSize)
                                 } into g
                                 select new {
                                     When = g.Key.SecondScaled * secondSize,
                                     Max = g.Select(x => x.Max).Max(),
                                     Min = g.Select(x => x.Min).Min(),
                                     Average = g.Select(x => x.Average).Average(),
                                 };

                return (await aggregated.ToListAsync()).Select(x => new HistoryEntry() {
                    WhenUTC = DateTime.UnixEpoch.AddSeconds(x.When),
                    InterfaceId = iface.Id,
                    Interface = iface,
                    Max = x.Max,
                    Min = x.Min,
                    Average = x.Average,
                });
            } else {
                // get all points in range
                var data = from entry in db.History
                           where entry.InterfaceId == iface.Id
                           where request.FromUTC <= entry.WhenUTC
                                && entry.WhenUTC <= request.ToUTC
                           select entry;

                var result = await data.ToListAsync();
                return result;
            }
        }

        public async Task<bool> SaveChangesAsync() {
            return await db.SaveChangesAsync() > 0;
        }

        public async Task CreateAsync(HistoryEntry entry) {
            await db.History.AddAsync(entry);
        }
    }

}