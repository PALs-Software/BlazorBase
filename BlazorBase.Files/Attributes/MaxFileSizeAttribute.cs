using System;

namespace BlazorBase.Files.Attributes
{
    public class MaxFileSizeAttribute : Attribute
    {
        /// <summary>Specifies the max file size in bytes</summary>
        public ulong MaxFileSize { get; set; }
    }
}
