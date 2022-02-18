using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchOrchestrator.Models
{
    public class RunnerTokenResponse
    {
        public string token { get; set; }

        public DateTimeOffset expires_at { get; set; }
    }
}
