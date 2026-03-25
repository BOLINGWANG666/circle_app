using SQLite;

namespace Circle.Models;

[Table("characters")]
public class PlayerSaveData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // 战斗与进度属性
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Atk { get; set; }
    public double Cd { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public double TimeRemaining { get; set; } = 60.0;
}