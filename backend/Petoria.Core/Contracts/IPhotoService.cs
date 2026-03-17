using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Petoria.Core.Contracts
{
    /// <summary>
    /// Интерфейс за качване и изтриване на снимки в облачно хранилище (Cloudinary).
    /// Използва се за хотелски снимки и потребителски аватари.
    /// </summary>
    public interface IPhotoService
    {
        /// <summary>Качва снимка и връща резултат с URL адреса</summary>
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

        /// <summary>Изтрива снимка по нейното публично ID в Cloudinary</summary>
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
