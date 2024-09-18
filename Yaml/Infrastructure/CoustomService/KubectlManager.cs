namespace Yaml.Infrastructure.CoustomService;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class KubectlManager
{
    public async Task ApplyYamlAsync(string yamlFilePath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "kubectl",
            Arguments = $"apply -f {yamlFilePath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine($"ERROR: {e.Data}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"kubectl apply command failed with exit code {process.ExitCode}");
            }
        }
    }
}
