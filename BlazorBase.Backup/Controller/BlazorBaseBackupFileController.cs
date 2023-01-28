using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace BlazorBase.Backup.Controller
{
    [Authorize(Policy = nameof(BlazorBaseBackupFileController))]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlazorBaseBackupFileController : ControllerBase
    {
        public record FileRequest(string FilePath, string FileDownloadName, string ContentType, FileOptions FileOptions);
        protected static ConcurrentDictionary<Guid, FileRequest> FileRequests { get; set; } = new ConcurrentDictionary<Guid, FileRequest>();

        internal static async Task<Guid> AddFileRequestAsync(IJSRuntime jsRuntime, FileRequest fileRequest)
        {
            Guid guid;
            while (!FileRequests.TryAdd(guid = Guid.NewGuid(), fileRequest)) ;

            var url = $"api/BlazorBaseBackupFile/GetFile/{guid}";
            await jsRuntime.InvokeVoidAsync("Custom.OpenLinkInNewTab", url);

            return guid;
        }


        [HttpGet("{fileRequestId}")]
        public IActionResult GetFile(Guid fileRequestId)
        {
            if (!FileRequests.ContainsKey(fileRequestId))
                return BadRequest();
            if (!FileRequests.TryRemove(fileRequestId, out FileRequest? fileRequest))
                return BadRequest();

            var fileStream = new FileStream(fileRequest.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, fileRequest.FileOptions);
            return File(fileStream, fileRequest.ContentType, fileRequest.FileDownloadName);
        }
    }
}
