namespace Bridge.Infrastructure;

public interface IStorageService
{
    Task UploadAsync(string key, Stream stream, CancellationToken cancellationToken);

    Task<string> GetPreSignedUrlAsync(string key, string fileName, bool isUpload, DateTimeOffset expiredAt);

    Task DeleteObjectsAsync(IEnumerable<string> keys, CancellationToken cancellationToken);
}