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

            double secondSize = request.PointCount.HasValue
                ? (request.ToUTC - request.FromUTC).TotalSeconds / request.PointCount.Value
                : 1;

            if (request.PointCount.HasValue && secondSize > 1) {

                // recalculate points
                var withEpochSecond = from entry in db.History
                                      where entry.WhenUTC >= request.FromUTC && entry.WhenUTC <= request.ToUTC && entry.InterfaceId == iface.Id
                                      let epochSecond = (EF.Functions.DateDiffDay(DateTime.UnixEpoch, entry.WhenUTC) * 24.0 * 3600.0
                                             + entry.WhenUTC.Hour * 3600.0
                                             + entry.WhenUTC.Minute * 60.0
                                             + entry.WhenUTC.Second
                                             + entry.WhenUTC.Millisecond / 1000.0)
                                      select new { epochSecond, entry };

                var aggregated = from entry in withEpochSecond
                                 group entry by new {
                                     epochSecondScaled = Math.Floor(entry.epochSecond / secondSize),
                                 } into g
                                 select new {
                                     WhenAverage = g.Select(x => x.epochSecond).Average(),
                                     Max = g.Select(x => x.entry.Max).Max(),
                                     Min = g.Select(x => x.entry.Min).Min(),
                                     Average = g.Select(x => x.entry.Average).Average(),
                                 };

                var entries = (await aggregated.ToListAsync()).Select(x => new HistoryEntry() {
                    WhenUTC = DateTime.UnixEpoch.AddSeconds(x.WhenAverage),
                    InterfaceId = iface.Id,
                    Interface = iface,
                    Max = x.Max,
                    Min = x.Min,
                    Average = x.Average,
                }).ToList();

                for (int i = 1; i < entries.Count; i++) {
                    var e0 = entries[i - 1];
                    var e1 = entries[i];
                    var delta = (e1.WhenUTC - e0.WhenUTC).TotalSeconds;
                    if (delta > 3 * secondSize) {
                        entries.Insert(i, new HistoryEntry() {
                            WhenUTC = e0.WhenUTC.AddSeconds(delta / 2),
                            InterfaceId = iface.Id,
                            Interface = iface,
                        });
                    }
                }

                return entries;
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