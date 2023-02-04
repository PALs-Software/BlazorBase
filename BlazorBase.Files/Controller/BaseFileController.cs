using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
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

        protected virtual Task<bool> AccessToFileIsGrantedAsync(Guid fileId) { return Task.FromResult(true); }
        protected virtual Task<bool> AccessToTemporaryFileIsGrantedAsync(Guid temporaryFileId) { return Task.FromResult(true); }

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
    }
}
