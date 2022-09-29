namespace AddressBookApp.Services.Interfaces
{
    public interface IImageService
    {
        //converts uploaded image to byte array
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        //converts byte array from database to image
        public string ConvertByeArrayToFile(byte[] fileData, string extension);
    }
}
