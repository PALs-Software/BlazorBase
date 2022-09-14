using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BlazorBase.Files.Controller
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseFileController : ControllerBase
    {
        //Needed because iis rejects + in a url (also if its encoded with HttpUtility) because of double escaping. (For example by serving svg files -> image/svg+xml)
        //And UrlPathEncode cause other troubles
        //Its securer to escape this special char here instead of allowing "allowDoubleEscaping" in the iis
        public static readonly string PlusEscapeSequence = "[%PLUS%]";
        public static readonly string SpaceEscapeSequence = "[%SPACE%]";

        protected IBlazorBaseFileOptions Options { get; set; }
        protected static DateTime NextCheckIfOldTemporaryFilesMustBeDeleted = DateTime.MinValue;

        public BaseFileController(IBlazorBaseFileOptions options)
        {
            Options = options;
        }

        [HttpGet("{contentType}/{id}")]
        public virtual async Task<IActionResult> GetFile(string contentType, string id)
        {
            if (!Guid.TryParse(id, out Guid result))
                return BadRequest("Id is not valid");

            if (!await AccessToFileIsGrantedAsync(contentType, result))
                return Unauthorized();

            var decodedContentType = DecodeUrl(contentType);
            var filePath = Path.Join(Options.FileStorePath, result.ToString());

            if (!System.IO.File.Exists(filePath))
                return BadRequest("File does not exist");

            if (decodedContentType == "video/mp4")
                return DownloadVideoWithRangeProcessing(filePath);

            return PhysicalFile(filePath, decodedContentType);
        }

        [HttpGet("{contentType}/{temporaryFileId}")]
        public virtual async Task<IActionResult> GetTemporaryFile(string contentType, string temporaryFileId)
        {
            if (!Guid.TryParse(temporaryFileId, out Guid result))
                return BadRequest("Id is not valid");
            
            if (!await AccessToTemporaryFileIsGrantedAsync(contentType, result))
                return Unauthorized();
            
            DeleteOldTemporaryFiles();

            var decodedContentType = DecodeUrl(contentType);
            var filePath = Path.Join(Options.TempFileStorePath, result.ToString());

            if (!System.IO.File.Exists(filePath))
                return BadRequest("File does not exist");

            if (decodedContentType == "video/mp4")
                return DownloadVideoWithRangeProcessing(filePath);

            return PhysicalFile(filePath, decodedContentType);
        }

        protected IActionResult DownloadVideoWithRangeProcessing(string videoFilePath)
        {
            var stream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return File(stream, "application/octet-stream", Path.GetFileName(videoFilePath), true); //enableRangeProcessing = true
        }

        protected virtual Task<bool> AccessToFileIsGrantedAsync(string contentType, Guid fileId) { return Task.FromResult(true); }
        protected virtual Task<bool> AccessToTemporaryFileIsGrantedAsync(string contentType, Guid temporaryFileId) { return Task.FromResult(true); }

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

        public static string EncodeUrl(string input)
        {
            return HttpUtility.UrlEncode(input.Replace(" ", SpaceEscapeSequence).Replace("+", PlusEscapeSequence));
        }

        public static string DecodeUrl(string input)
        {
            return HttpUtility.UrlDecode(input).Replace(SpaceEscapeSequence, " ").Replace(PlusEscapeSequence, "+");
        }
    }
}
