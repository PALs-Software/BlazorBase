namespace BlazorBase.CRUD.Models;

public class ExplainText
{
    public ExplainText(string text, ExplainTextLocation location)
    {
        this.Text = text;
        this.Location = location;
    }

    public string Text { get; set; }
    public ExplainTextLocation Location { get; set; }
}

public enum ExplainTextLocation
{
    Top,
    Bottom
}