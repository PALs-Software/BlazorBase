using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class CRUDException : Exception
    {
        public CRUDException(): base() { }
        public CRUDException(string message) : base(message) { }
        public CRUDException(string message, Exception innerException) : base(message, innerException) { }
    }
}
