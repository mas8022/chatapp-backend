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
        var endpoint = config.GetConnectionString("LIARA_ENDPOINT");
        var accessKey = config.GetConnectionString("LIARA_ACCESS_KEY");
        var secretKey = config.GetConnectionString("LIARA_SECRET_KEY");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            s3Config
        );

        _bucketName = config.GetConnectionString("LIARA_BUCKET_NAME")!;
        _publicEndpoint = $"{endpoint}/{_bucketName}";
    }

    public async Task<string> UploadFileAsync(
        IFormFile file,
        string folder = "avatars"
    )
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Only jpg, jpeg, png and webp files are allowed.");

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var objectKey = $"{folder}/{fileName}";

        await using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request);

        return $"{_publicEndpoint}/{objectKey}";
    }

    public (string uploadUrl, string fileUrl) GeneratePreSignedUrl(
        string fileName,
        string contentType
    )
    {
        var uniqueFileName =
            $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

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
