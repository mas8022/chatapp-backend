using Amazon.S3;
using Amazon.S3.Model;

namespace backend.common.services;

public class StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicEndpoint;

    public StorageService(IConfiguration config)
    {
        var s3Config = new AmazonS3Config
        {
            ServiceURL = config["Liara:Endpoint"],
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(config["Liara:AccessKey"], config["Liara:SecretKey"], s3Config);
        _bucketName = config["Liara:BucketName"]!;
        _publicEndpoint = config["Liara:PublicDomain"] ?? $"{config["Liara:Endpoint"]}/{_bucketName}";
    }

    public (string uploadUrl, string fileUrl) GeneratePreSignedUrl(string fileName, string contentType)
    {

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = uniqueFileName,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            ContentType = contentType
        };

        string uploadUrl = _s3Client.GetPreSignedURL(request);
        string fileUrl = $"{_publicEndpoint}/{uniqueFileName}";

        return (uploadUrl, fileUrl);
    }
}
