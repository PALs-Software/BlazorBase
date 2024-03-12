using BlazorBase.Models;

namespace BlazorBase.CRUD.Models;
public interface IBlazorBaseCRUDOptions : IBaseOptions
{
    /// <summary>
    /// Sets the default behavior of BaseDbContext regarding asyncronous or syncronous db context methods.
    /// Currently there is a performance problem in the .net SQLClient when data records are loaded from the database that contain large amounts of data of type string.
    /// This performance problem only occurs with the async methods.
    /// For this reason, it may be useful to use the sync methods of the db context.
    /// </summary>
    bool UseAsyncDbContextMethodsPerDefaultInBaseDbContext { get; set; }
}