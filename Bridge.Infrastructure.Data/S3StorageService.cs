using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;

namespace Bridge.Infrastructure.Data;

public class S3StorageService : IStorageService, IDisposable
{
    private readonly AmazonS3Client _s3Client;
    private readonly IOptions<S3Settings> _configs;

    public S3StorageService(IOptions<S3Settings> configs)
    {
        _configs = configs;
        _s3Client = ConfigureAmazonS3Client();
    }
    
    private AmazonS3Client ConfigureAmazonS3Client()
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(Configurations.RegionName),
        };

        if (!string.IsNullOrEmpty(Configurations.ServiceUrl))
        {
            config.ServiceURL = Configurations.ServiceUrl;
            config.ForcePathStyle = Configurations.ForcePathStyle;
        }

        if (!string.IsNullOrEmpty(Configurations.AccessKey))
        {
            var credentials = new BasicAWSCredentials(Configurations.AccessKey, Configurations.SecretKey);
            return new AmazonS3Client(credentials, config);
        }

        return new AmazonS3Client(config);
    }

    private S3Settings Configurations => _configs.Value;
    
    private async Task TryCreateBucket(CancellationToken cancellationToken)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, Configurations.BucketName))
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = Configurations.BucketName,
                UseClientRegion = true,
            };

            await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);
        }
    }

    private Protocol GetServiceUrlProtocol()
    {
        if (Uri.TryCreate(Configurations.ServiceUrl, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == "http" ? Protocol.HTTP : Protocol.HTTPS;
        }

        return Protocol.HTTPS;
    }


    public async Task UploadAsync(string key, Stream stream, CancellationToken cancellationToken)
    {
        await TryCreateBucket(cancellationToken);
        using var transferUtility = new TransferUtility(_s3Client);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = Configurations.BucketName,
            Key = key,
        };

        await transferUtility.UploadAsync(uploadRequest, cancellationToken);
    }

    public Task<string> GetPreSignedUrlAsync(string key, string fileName, bool isUpload, DateTimeOffset expiredAt)
    {
        var preSignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = Configurations.BucketName,
            Key = key,
            Verb = isUpload ? HttpVerb.PUT : HttpVerb.GET,
            Expires = expiredAt.UtcDateTime,
            Protocol = GetServiceUrlProtocol()
        };
        if (isUpload)
        {
            preSignedUrlRequest.ContentType = "application/octet-stream";
        }

        return _s3Client.GetPreSignedURLAsync(preSignedUrlRequest);
    }

    public async Task DeleteObjectsAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        // TODO: Find out why DeleteObjects did not work
        await Parallel.ForEachAsync(keys, cancellationToken, (key, ct)
            => new(_s3Client.DeleteObjectAsync(Configurations.BucketName, key, ct)));
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}