using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Files.Models
{
    [Display(Name = "Datei")]
    [PluralDisplayName("Dateien")]
    public class BaseFile : BaseModel
    {
        [Key]
        [HideInGUI]
        [Required]
        [Display(Name = "Dateikey")]
        public Guid FileKey { get; set; }

        [FileLink]
        [HideInCard]
        [Display(Name = "Datei")]
        public string FileLink
        {
            get
            {
                return GetBinaryFileLink();
            }
        }

        [Required]
        [PlaceholderText("Dateinamen eingeben...")]
        [StringLength(250)]
        [Display(Name = "Dateinamen")]
        public string FileName { get; set; }

        [HideInGUI]
        [Required]
        [PlaceholderText("Dateityp eingeben...")]
        [Display(Name = "Dateityp")]
        public BaseFileType BaseFileType { get; set; }

        [PlaceholderText("Gruppierungspfad angeben z.B. \"Gegner\\Magisch\"")]
        [Display(Name = "Gruppierungspfad")]
        public string FolderPath { get; set; }

        [PlaceholderText("Beschreibung eingeben...")]
        [Display(Name = "Beschreibung")]
        public string Description { get; set; }

        [HideInGUI]
        [Display(Name = "Besitzer")]
        public virtual List<ApplicationUser> Owners { get; set; }

        [NotMapped]
        [HideInList]
        [Display(Name = "Datei")]
        public BinaryFile BinaryFile { get; set; }

        [ReadOnlyInGUI]
        [Display(Name = "Dateigröße")]
        public long FileSize { get; set; }



        #region Helper Methods
        public bool IsImage()
        {
            return (int)BaseFileType >= 1000 && (int)BaseFileType < 2000;
        }

        #endregion

        #region CRUD
        public override async Task OnAfterPropertyChanged(PropertyInfo property)
        {
            if (property.Name != nameof(BinaryFile))
                return;

            if (BinaryFile == null || BinaryFile.Data == null || String.IsNullOrEmpty(BinaryFile.Name))
                return;

            FileName = Path.GetFileNameWithoutExtension(BinaryFile.Name);
            ForcePropertyRepaint(nameof(FileName));
            FileSize = BinaryFile.FileSize;
            ForcePropertyRepaint(nameof(FileSize));

            var extension = Path.GetExtension(BinaryFile.Name);
            if (!String.IsNullOrEmpty(extension) && extension.Length > 1 && Enum.TryParse(extension.Substring(1), true, out BaseFileType baseFileType))
            {
                BaseFileType = baseFileType;
                ForcePropertyRepaint(nameof(BaseFileType));
            }

            await base.OnAfterPropertyChanged(property);
        }

        public override async Task<bool> OnBeforeAddEntry()
        {
            FileKey = Guid.NewGuid();

            return await base.OnBeforeAddEntry();
        }

        public override async Task OnAfterAddEntry()
        {
            await SaveBinaryFile();

            await base.OnAfterAddEntry();
        }

        public override async Task OnAfterUpdateEntry()
        {
            await SaveBinaryFile();

            await base.OnAfterUpdateEntry();
        }

        #endregion

        #region File Handling

        private async Task SaveBinaryFile()
        {
            await Task.Run(() =>
            {
                if (BinaryFile == null || BinaryFile.Data == null || BinaryFile.Data.Length == 0)
                    return;

                File.WriteAllBytes(Path.Join(VDMSettings.Instance.FileStorePath, FileKey.ToString()), BinaryFile.Data);
            });
        }

        public BinaryFile GetBinaryFileFromDisk()
        {
            var bytes = File.ReadAllBytes(Path.Join(VDMSettings.Instance.FileStorePath, FileKey.ToString()));
            return new BinaryFile($"{FileName}.{BaseFileType}", FileSize, bytes);
        }

        public string GetBinaryFileLink()
        {
            return $"/api/BaseFile/{GetMimeType()}/{FileKey}";
        }

        public string GetMimeType()
        {
            switch (BaseFileType)
            {
                case BaseFileType.PNG:
                    return "image/png";
                case BaseFileType.JPG:
                    return "image/jpeg";
                case BaseFileType.JPEG:
                    return "image/jpeg";
                case BaseFileType.GIF:
                    return "image/gif";
                case BaseFileType.TIFF:
                    return "image/tiff";
                case BaseFileType.BMP:
                    return "image/bmp";
                case BaseFileType.PDF:
                    return "application/pdf";
                case BaseFileType.DOCX:
                    return "application/octet-stream";
                case BaseFileType.XLSX:
                    return "application/octet-stream";
                case BaseFileType.MP3:
                    return "audio/mpeg";
                case BaseFileType.WAV:
                    return "audio/wav";
                case BaseFileType.MP4:
                    return "video/mp4";
                case BaseFileType.MPEG:
                    return "video/mpeg";
                case BaseFileType.AVI:
                    return "video/x-msvideo";
                case BaseFileType.ZIP:
                    return "application/zip";
                default:
                    return "application/octet-stream";
            }
        }
        #endregion
    }

    public record BinaryFile(string Name, long FileSize, byte[] Data);

    public enum BaseFileType
    {
        PNG = 1000,
        JPG = 1010,
        JPEG = 1020,
        GIF = 1030,
        TIFF = 1040,
        BMP = 1050,
        PDF = 2000,
        DOCX = 3000,
        XLSX = 4000,
        MP3 = 5000,
        WAV = 5010,
        MP4 = 6000,
        MPEG = 6010,
        AVI = 6020,
        ZIP = 7000
    }
}
