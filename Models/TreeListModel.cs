using System.Globalization;

public class TreeListModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Value { get; set; }
    public double OriginalValue { get; set; }
    public int? TreeListModelId { get; set; }
    public List<TreeListModel> Children { get; set; } = new();
}
