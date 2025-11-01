using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public interface IImageService
{
    Task CreateThumbnailAsync(byte[] inputImageBytes, uint imageThumbnailSize, string destinationPath);
    Task<byte[]> ResizeImageAsync(byte[] inputImageBytes, uint width, uint height);
    Task<byte[]> ResizeImageToMaxSizeAsync(byte[] inputImageBytes, uint maxSize);
    Task ResizeImageAsync(string path, uint width, uint height);
}
