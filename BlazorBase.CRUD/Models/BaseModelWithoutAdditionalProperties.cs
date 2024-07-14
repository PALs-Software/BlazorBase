﻿using BlazorBase.Abstractions.CRUD.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBase.CRUD.Models;

public class BaseModelWithoutAdditionalProperties : BaseModel
{
    [NotMapped]
    [Visible(false)]
    public override DateTime CreatedOn { get => base.CreatedOn; set => base.CreatedOn = value; }

    [NotMapped]
    [Visible(false)]
    public override DateTime ModifiedOn { get => base.ModifiedOn; set => base.ModifiedOn = value; }

}