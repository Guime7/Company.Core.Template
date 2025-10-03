using OpenTelemetry;
using OpenTelemetry.Logs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.Core.Template.Infrastructure.Telemetry
{
    public class ActivityEventLogProcessor : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord data)
        {
            base.OnEnd(data);
            var activity = Activity.Current;
            activity?.AddEvent(new ActivityEvent(data.Attributes.ToString()));
        }
    }
}
