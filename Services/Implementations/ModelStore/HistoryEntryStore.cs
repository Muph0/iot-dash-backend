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

            double secondSize = (request.To - request.From).TotalSeconds / request.PointCount;

            var aggregated = from ent in db.History
                             where ent.When >= request.From && ent.When <= request.To
                             group ent by new {
                                 SecondScaled = Math.Floor(
                                    (EF.Functions.DateDiffDay(DateTime.UnixEpoch, ent.When) * 24.0 * 3600.0
                                    + ent.When.Hour * 3600.0
                                    + ent.When.Minute * 60.0
                                    + ent.When.Second
                                    + ent.When.Millisecond / 1000.0)
                                 / secondSize)
                             } into g
                             select new {
                                 When = g.Key.SecondScaled * secondSize,
                                 Max = g.Select(x => x.Max).Max(),
                                 Min = g.Select(x => x.Min).Min(),
                                 Average = g.Select(x => x.Average).Average(),
                             };

            return (await aggregated.ToListAsync()).Select(x => new HistoryEntry() {
                When = DateTime.UnixEpoch.AddSeconds(x.When),
                DeviceId = iface.DeviceId,
                InterfaceId = iface.Id,
                Interface = iface,
                Max = x.Max,
                Min = x.Min,
                Average = x.Average,
            });
        }

        public async Task<bool> SaveChangesAsync() {
            return await db.SaveChangesAsync() > 0;
        }

        public async Task CreateAsync(HistoryEntry entry) {
            await db.History.AddAsync(entry);
        }
    }

}