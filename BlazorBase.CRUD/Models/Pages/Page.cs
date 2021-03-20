using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models.Pages
{
    public class TestModel : BaseModel<TestModel>, IBaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TestPage : Page<TestModel>
    {
        public override string Caption => "TestModel";

        public override PageType PageType => PageType.Card;

        public override PageLayout PageLayout => new PageLayout()
        {
            FieldGroups = new List<FieldGroup>()
            {
                new FieldGroup()
                {
                    Caption = "General",

                    Fields = new List<Field>()
                    {
                        new Field()
                        {
                            ToolTip = "asdf",
                            Property = GetProperty(nameof(TestModel.Id))
                        },
                        new Field()
                        {
                            ToolTip = "asdf",
                            Property = GetProperty(nameof(TestModel.Description))
                        }
                    }
                },
                new FieldGroup()
                {
                    Caption = "Facts",

                    Fields = new List<Field>()
                    {
                        new Field()
                        {
                            ToolTip = "asdf",
                            Property = GetProperty(nameof(TestModel.CreatedOn))
                        },
                        new Field()
                        {
                            ToolTip = "asdf",
                            Property = GetProperty(nameof(TestModel.ModifiedOn))
                        }
                    }
                }
            }
        };
    }

    public abstract class Page<T> where T : IBaseModel
    {
        public abstract string Caption { get; }
        public abstract PageType PageType { get; }
        public virtual string SourceModelView { get; }


        public abstract PageLayout PageLayout { get; }

        public virtual List<PageAction> PageActions { get; init; }

        public PropertyInfo GetProperty(string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
        }
    }

    public enum PageType
    {
        Card,
        List,
        CardPart,
        ListPart
    }

    public class PageLayout
    {
        public virtual IEnumerable<FieldGroup> FieldGroups { get; init; }
    }

    public class FieldGroup
    {
        public string Caption { get; init; }
        public virtual List<Field> Fields { get; init; }
        public virtual List<PagePart> PageParts { get; init; }
    }

    public class Field
    {
        public PropertyInfo Property { get; init; }

        public string Caption { get; init; }
        public string ToolTip { get; init; }

    }

    public class PagePart
    {
        public string Caption { get; init; }
        public virtual object Page { get; init; }
    }

    public class PageAction
    {
        public string Caption { get; init; }
        public string ToolTip { get; init; }
        public string Image { get; init; }

        public Action Action { get; init; }
    }
}
