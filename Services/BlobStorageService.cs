using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Event_Ease.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _accountKey;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];
            _accountKey = configuration["AzureBlobStorage:AccountKey"];
            
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            
            // Create container if it doesn't exist (no public access)
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Debug info
                Console.WriteLine($"Uploading file: {file.FileName}, Length: {file.Length}");
            // Create a unique name for the blob
            string blobName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            Console.WriteLine($"Generated blob name: {blobName}");
            
            // Get a reference to the blob
            var blobClient = _containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Created blob client for: {blobClient.Uri}");
            
            // Upload the file
            await using var stream = file.OpenReadStream();
            Console.WriteLine("Opened file stream, uploading...");
            
            // Set the content type
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            Console.WriteLine("File uploaded successfully");
            
            // Generate a SAS token that's valid for a long time
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b", // "b" for blob
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(10) // 10 years
            };
            
            // Allow read access only
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            
            // Generate the SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(
                new Azure.Storage.StorageSharedKeyCredential(
                    _blobServiceClient.AccountName,
                    _accountKey
                )
            ).ToString();
            
            // Return the full URL with SAS token
            return blobClient.Uri + "?" + sasToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to see the error in the UI
            }
        }

        public async Task DeleteImageAsync(string blobUrl)
        {
            try
            {
                // Parse the URL to get just the blob name
                var uri = new Uri(blobUrl);
                string path = uri.AbsolutePath;
                // The path includes the container name, so extract just the filename
                string blobName = path.Substring(path.LastIndexOf('/') + 1);
                
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Log the error but don't throw - we don't want deletion issues to break the application
            }
        }
    }
}