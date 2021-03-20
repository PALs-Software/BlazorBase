using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    [Route("/NoSeries")]
    public class NoSeries : BaseModel<NoSeries>, IBaseModel
    {
        [Key]
        [Required]
        [Visible]
        [StringLength(20)]
        public string Code { get; set; }

        [Visible]
        public string Description { get; set; }

        [Visible]
        [Required]
        public string StartingNo { get; set; }

        [Visible]
        [Required]
        public string EndingNo { get; set; }

        [Visible]
        [Editable(false)]
        public string LastNoUsed { get; set; }
    }
}
