using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTelemetry;
using OpenTelemetry.Logs;

public class CustomLogProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        // Custom state information
        var logState = new List<KeyValuePair<string, object?>>
        {
            new("ProcessID", Environment.ProcessId),
            new("DotnetFramework", RuntimeInformation.FrameworkDescription),
            new("Runtime", RuntimeInformation.RuntimeIdentifier),
            new("hello", "salut")
        };
    
        base.OnEnd(data);
    }
}
