using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.Files.Attributes
{
    public class MaxFileSizeAttribute : Attribute
    {
        /// <summary>Specifies the max file size in bytes</summary>
        public ulong MaxFileSize { get; set; }
    }
}
