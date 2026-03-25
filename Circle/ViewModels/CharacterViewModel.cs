using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using Circle.Models;
using Circle.Services;

namespace Circle.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    public static CharacterViewModel? Current { get; set; }

    private SQLiteConnection connection;

    public CharacterViewModel()
    {
        Current = this;
        connection = DatabaseService.Connection;
    }

    

    // READ (查)：从数据库读取所有保存
    public List<PlayerSaveData> Characters
    {
        get
        {
            return connection.Table<PlayerSaveData>().ToList();
        }
    }

    // CREATE & UPDATE (增/改)：自动判断写入硬盘
    public void SaveCharacter(PlayerSaveData model)
    {
        if (model.Id > 0)
        {
            connection.Update(model);
        }
        else
        {
            connection.Insert(model);
        }
    }

    // DELETE (删)：从硬盘物理删除
    public void DeleteCharacter(PlayerSaveData model)
    {
        if (model.Id > 0)
        {
            connection.Delete(model);
        }
    }
}