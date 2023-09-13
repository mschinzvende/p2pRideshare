namespace p2pRideshare.FileUploadService
{
    public class LocalFileUploadService : IFileUploadService
    {

        public LocalFileUploadService(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public string UploadFile(IFormFile user_picture)
        {
            var filePath = Path.Combine(Environment.ContentRootPath,@"wwwroot\mugshots", user_picture.FileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            user_picture.CopyTo(fileStream);
            return user_picture.FileName;
            
        }
    }
}
