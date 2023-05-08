using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.Files.Controller
{
    
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = nameof(BaseFileController))]    
    public class BaseFileController : ControllerBase
    {
        #region Injects
        protected IBlazorBaseFileOptions Options { get; set; }
        #endregion

        protected static DateTime NextCheckIfOldTemporaryFilesMustBeDeleted = DateTime.MinValue;

        public BaseFileController(IBlazorBaseFileOptions options)
        {
            Options = options;
        }

        #region Base File Download
        
        [HttpGet("{id}/{fileName}")]
        public virtual async Task<IActionResult> GetFile(string id, string fileName)
        {
            if (!Guid.TryParse(id, out Guid result))
                return BadRequest("Id is not valid");

            if (!await AccessToFileIsGrantedAsync(result))
                return Unauthorized();

            var filePath = Directory.EnumerateFiles(Options.FileStorePath, $"{result}_*").FirstOrDefault();
            if (filePath == null || !System.IO.File.Exists(filePath))
                return BadRequest("File does not exist");

            var mimeType = GetMimeTypeOfFileName(Path.GetFileName(filePath));
            if (mimeType == "video/mp4")
                return DownloadVideoWithRangeProcessing(filePath, fileName);

            return PhysicalFile(filePath, mimeType);
        }

        [HttpGet("{temporaryFileId}/{fileName}")]
        public virtual async Task<IActionResult> GetTemporaryFile(string temporaryFileId, string fileName)
        {
            if (!Guid.TryParse(temporaryFileId, out Guid result))
                return BadRequest("Id is not valid");

            if (!await AccessToTemporaryFileIsGrantedAsync(result))
                return Unauthorized();

            var filePath = Directory.EnumerateFiles(Options.TempFileStorePath, $"{result}_*").FirstOrDefault();
            if (filePath == null || !System.IO.File.Exists(filePath))
                return BadRequest("File does not exist");

            DeleteOldTemporaryFiles();
            var mimeType = GetMimeTypeOfFileName(Path.GetFileName(filePath));
            if (mimeType == "video/mp4")
                return DownloadVideoWithRangeProcessing(filePath, fileName);

            return PhysicalFile(filePath, mimeType);
        }

        protected IActionResult DownloadVideoWithRangeProcessing(string videoFilePath, string fileName)
        {
            var stream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return File(stream, "application/octet-stream", fileName, true); //enableRangeProcessing = true
        }

        #endregion

        #region Binary Download

        public record BinaryData(string Name, string MimeContentType, byte[] Data);
        protected static ConcurrentDictionary<Guid, BinaryData> BinaryDataDownloadRequests { get; set; } = new ConcurrentDictionary<Guid, BinaryData>();

        public static async Task<Guid> DownloadBinaryDataToClientAsync(IJSRuntime jsRuntime, BinaryData binaryData)
        {
            Guid guid;
            while (!BinaryDataDownloadRequests.TryAdd(guid = Guid.NewGuid(), binaryData)) ;

            var url = $"{BlazorBaseFileOptions.Instance.ControllerRoute}/DownloadBinaryData/{guid}";
            await jsRuntime.InvokeVoidAsync("open", url, "_blank");

            return guid;
        }

        [HttpGet("{binaryDataDownloadRequestId}")]
        public virtual IActionResult DownloadBinaryData(Guid binaryDataDownloadRequestId)
        {
            if (!BinaryDataDownloadRequests.ContainsKey(binaryDataDownloadRequestId))
                return BadRequest();

            BinaryDataDownloadRequests.TryRemove(binaryDataDownloadRequestId, out BinaryData binaryData);

            return File(binaryData.Data, binaryData.MimeContentType, binaryData.Name);
        }

        #endregion


        #region Access Controll

        protected virtual Task<bool> AccessToFileIsGrantedAsync(Guid fileId) { return Task.FromResult(true); }
        protected virtual Task<bool> AccessToTemporaryFileIsGrantedAsync(Guid temporaryFileId) { return Task.FromResult(true); }
        
        #endregion

        #region MISC
        
        protected virtual void DeleteOldTemporaryFiles()
        {
            if (!Options.AutomaticallyDeleteOldTemporaryFiles)
                return;

            if (
                NextCheckIfOldTemporaryFilesMustBeDeleted > DateTime.Now ||
                String.IsNullOrEmpty(Options.TempFileStorePath) ||
                !Directory.Exists(Options.TempFileStorePath) ||
                Options.TempFileStorePath.ToLower().Replace("/", "").Replace(@"\", "").Replace(@":", "") == "c")
                return;

            NextCheckIfOldTemporaryFilesMustBeDeleted = DateTime.Now.AddSeconds(Options.DeleteTemporaryFilesOlderThanXSeconds);

            var dirInfo = new DirectoryInfo(Options.TempFileStorePath);

            var files = dirInfo.GetFiles().Where(fileInfo => fileInfo.CreationTime < DateTime.Now.Subtract(TimeSpan.FromSeconds(Options.DeleteTemporaryFilesOlderThanXSeconds)));
            foreach (var fileInfo in files)
                try { fileInfo.Delete(); } catch (Exception) { throw; }
        }

        protected virtual string GetMimeTypeOfFileName(string fileName)
        {
            return fileName.Split("_")[1].Replace("'", "/").Replace("^", ".");
        }

        #endregion
    }
}
