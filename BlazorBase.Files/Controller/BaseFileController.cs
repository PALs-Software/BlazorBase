using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace BlazorBase.Files.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseFileController : ControllerBase
    {
        //Needed because iis rejects + in a url (also if its encoded with HttpUtility) because of double escaping. (For example by serving svg files -> image/svg+xml)
        //And UrlPathEncode cause other troubles
        //Its securer to escape this special char here instead of allowing "allowDoubleEscaping" in the iis
        public static readonly string PlusEscapeSequence = "[%PLUS%]";
        public static readonly string SpaceEscapeSequence = "[%SPACE%]";

        protected BlazorBaseFileOptions Options { get; set; }

        public BaseFileController(BlazorBaseFileOptions options)
        {
            Options = options;
        }

        [HttpGet("{contentType}/{id}")]
        public IActionResult Get(string contentType, string id)
        {
            if (!Guid.TryParse(id, out Guid result))
                return BadRequest("Id is not valid");

            return PhysicalFile(Path.Join(Options.FileStorePath, result.ToString()), DecodeUrl(contentType));
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
