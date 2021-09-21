using IotDash.Contracts.V1.Model;
using System.Collections.Generic;

namespace IotDash.Contracts.V1 {
    public class HistoryResponse : StatusResponse<IEnumerable<HistoryEntry>, HistoryResponse> {

        public IEnumerable<HistoryEntry>? Values => base.Value;

    }
}