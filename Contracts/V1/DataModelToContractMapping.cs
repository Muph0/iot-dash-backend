using IotDash.Contracts.V1.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IotDash.Contracts.V1 {

    /// <summary>
    /// Extension class for converting <see cref="IotDash.Data.Model"/> objects to their REST API counterparts.
    /// </summary>
    public static class DataModelToContractMapping {

        public static IotDevice ToContract(this Data.Model.IotDevice device) {
            return new IotDevice(device);
        }
        public static IotDeviceWInterfaces ToContractDetail(this Data.Model.IotDevice device) {
            return new IotDeviceWInterfaces(device);
        }

        public static IotInterface ToContract(this Data.Model.IotInterface iface) {
            return new IotInterface(iface);
        }

        public static User ToContract(this Microsoft.AspNetCore.Identity.IdentityUser user) {
            return new User(user);
        }

        public static HistoryEntry ToContract(this Data.Model.HistoryEntry entry) {
            return new HistoryEntry(entry);
        }
    }
}
