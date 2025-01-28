using System.Runtime.InteropServices;

namespace Yaml.Infrastructure.CoustomService;

using System.Diagnostics;
using System.Threading.Tasks;

public static class ShellHelper
{
    /// <summary>
    /// 执行 shell 命令并返回命令的输出。
    /// </summary>
    /// <param name="command">要执行的 shell 命令。</param>
    /// <returns>命令输出的字符串。</returns>
    public static async Task<string> RunShellCommand(string command)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "/bin/bash", // 根据操作系统选择
                Arguments = $"-c \"{command}\"", // -c 选项允许传递完整的命令字符串
                RedirectStandardOutput = true, // 重定向标准输出
                RedirectStandardError = true, // 重定向标准错误
                UseShellExecute = false, // 不使用系统外壳启动进程
                CreateNoWindow = true, // 不创建窗口
            }
        };

        process.Start();
        // 读取标准输出文本
        string result = await process.StandardOutput.ReadToEndAsync();
     
        // 等待进程结束
        await process.WaitForExitAsync();
        return result;
    }
}
