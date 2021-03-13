using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Files.Components
{
    public partial class BaseInputFile : BaseInput
    {
        public string FileFilter = "*.";

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                CurrentValueAsString = String.Empty;
                FileFilter = String.Join(", ", Enum.GetNames(typeof(BaseFileType)).Select(type => $".{type}"));
            });
        }

        async Task OnFileChanged(FileChangedEventArgs e)
        {
            await Model.OnBeforePropertyChanged(Property, CurrentValueAsString);

            try
            {
                foreach (var file in e.Files)
                {
                    using var stream = new MemoryStream();
                    await file.WriteToStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    Property.SetValue(Model, new BinaryFile(file.Name, file.Size, stream.ToArray()));
                    SetValidation(showValidation: false);
                    await Model.OnAfterPropertyChanged(Property);
                }
            }
            catch (Exception ex)
            {
                SetValidation(feedback: ex.Message);
            }
        }
    }
}
