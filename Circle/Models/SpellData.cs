namespace Circle.Models;

public class SpellData
{
    public required string Id { get; set; }
    public required string Icon { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string EffectType { get; set; }
    public double Value { get; set; }
}