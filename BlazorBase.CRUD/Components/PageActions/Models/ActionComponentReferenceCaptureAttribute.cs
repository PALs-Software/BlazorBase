using BlazorBase.CRUD.Components.PageActions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.PageActions.Models;

public record ActionComponentReferenceCaptureAttribute(int Sequence, Action<object> Value) : IActionComponentAttribute;
