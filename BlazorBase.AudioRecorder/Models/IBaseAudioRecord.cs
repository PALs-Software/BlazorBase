using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Files.Models;

namespace BlazorBase.AudioRecorder.Models;

public interface IBaseAudioRecord : IBaseModel
{
    Guid Id { get; set; }
    string FileName { get; set; }
    string? MimeFileType { get; set; }
    string? Hash { get; set; }
    Guid? AudioFileId { get; set; }
    Guid? TempAudioFileId { get; set; }
    IBaseFile? AudioFile { get; set; }

    string? GetFileLink();
}
