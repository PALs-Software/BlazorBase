using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.Files.Models
{
    [Route("/BaseFiles")]
    public class BaseFile : BaseModel<BaseFile>, IBaseModel
    {
        [Key]
        [Visible(DisplayOrder = 100)]
        public Guid Id { get; set; }

        [Required]
        [DisplayKey]
        [Visible(DisplayOrder = 200)]
        public string FileName { get; set; }

        [Visible(DisplayOrder = 300)]
        public string Description { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 400)]
        public string BaseFileType { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 500)]
        public string MimeFileType { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 600)]
        public long FileSize { get; set; }

        /// <summary>
        /// This property is only needed to show the file in the general base file list and card.
        /// </summary>
        [NotMapped]
        [Editable(false)]
        [Visible(DisplayOrder = 700)]
        public BaseFile DisplayFile { get { return this; } }

        [NotMapped]
        public byte[] Data { get; set; }

        #region CRUD    
        public override Task OnBeforeAddEntry(OnBeforeAddEntryArgs args)
        {
            do
            {
                Id = Guid.NewGuid();
            } while (args.EventServices.BaseService.GetAsync<BaseFile>(Id) == null);

            return base.OnBeforeAddEntry(args);
        }

        public override async Task OnAfterAddEntry(OnAfterAddEntryArgs args)
        {
            await SaveBinaryFile(args.EventServices.ServiceProvider);
            await base.OnAfterAddEntry(args);
        }

        public override async Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args)
        {
            await SaveBinaryFile(args.EventServices.ServiceProvider);
            await base.OnAfterUpdateEntry(args);
        }

        #endregion

        #region File Handling

        protected async Task SaveBinaryFile(IServiceProvider serviceProvider)
        {
            await Task.Run(() =>
            {
                if (Data == null || FileSize == 0)
                    return;

                var options = serviceProvider.GetService<BlazorBaseFileOptions>();
                if (!Directory.Exists(options.FileStorePath))
                    Directory.CreateDirectory(options.FileStorePath);

                File.WriteAllBytes(Path.Join(options.FileStorePath, Id.ToString()), Data);
            });
        }

        public void LoadFileFromDisk(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<BlazorBaseFileOptions>();

            Data = File.ReadAllBytes(Path.Join(options.FileStorePath, Id.ToString()));
        }

        #endregion
    }




}
