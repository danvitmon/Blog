using Blog.Services.Interfaces;

namespace Blog.Services
{
    public class IImageService : Interfaces.ImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            throw new NotImplementedException();
        }

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension)
        {
            throw new NotImplementedException();
        }
    }
}
