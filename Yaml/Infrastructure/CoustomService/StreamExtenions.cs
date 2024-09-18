namespace Yaml.Infrastructure.CoustomService;

public class StreamExtenions
{
    public static async Task<FileInfo> SaveToFileAsync( Stream stream, string fileName)
    {
        // 创建临时文件路径
        var uploadDirectory = "uploads";
        if (!Directory.Exists(uploadDirectory))
        {
            Directory.CreateDirectory(uploadDirectory);
        }
        var tempFilePath = Path.Combine(uploadDirectory, fileName);
        
        // 将 Stream 写入临时文件
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream);
        }

        // 返回 FileInfo 对象
        return new FileInfo(tempFilePath);
    }
}