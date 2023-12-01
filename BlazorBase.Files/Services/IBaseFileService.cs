using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public interface IBaseFileService
{
    Task<IBaseFile> CreateFileAsync(EventServices eventServices, string fileName, string baseFileType, string mimeFileType, byte[] fileContent);
    Task<IBaseFile> CreateCopyAsync(IBaseFile sourceFile, EventServices eventServices);


    string ComputeSha256Hash(Func<SHA256, byte[]> computeHash);

    string ComputeSha256Hash(FileStream fileStream);

    string ComputeSha256Hash(byte[] buffer);
}
