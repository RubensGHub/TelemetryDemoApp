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
        };
        
        // Example of masking sensitive data
        if (data.Attributes != null)
        {
            var attributes = data.Attributes.ToList();
            
            // Find a key value pair with key "password" and update its value to "masked value"
            var foundPair = attributes.Find(kvp => kvp.Key.Equals("password", StringComparison.OrdinalIgnoreCase));
            if (!foundPair.Equals(default(KeyValuePair<string, object?>)))
            {
                // Find the index of the original pair in the list
                var index = attributes.IndexOf(foundPair);

                // Replace the original pair with the updated pair at the same index
                attributes[index] = new KeyValuePair<string, object?>(foundPair.Key, "masked value");
                data.FormattedMessage = "Message masked due to sensitive data";
            }

            data.Attributes = new ReadOnlyCollectionBuilder<KeyValuePair<string, object?>>(attributes.Concat(logState))
                .ToReadOnlyCollection();
        }

        base.OnEnd(data);
    }
}
