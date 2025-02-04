using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class TreeListModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
    public string? Value { get; set; }
    public string? OriginalValue { get; set; }
    public int? TreeListModelId { get; set; }
    public List<int>? Children { get; set; } = new List<int>();

    private bool _visited = false;

    [JsonIgnore]
    public bool Visited
    {
        get
        {
            return _visited;
        }
        set
        {
            _visited = value;
        }
    }
}
