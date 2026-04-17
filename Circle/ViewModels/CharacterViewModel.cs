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



    // READ: Read all saves from the database
    public List<PlayerSaveData> Characters
    {
        get
        {
            return connection.Table<PlayerSaveData>().ToList();
        }
    }

    // CREATE & UPDATE: Automatically determine whether to write to disk
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

    // DELETE: Physically delete from the hard drive
    public void DeleteCharacter(PlayerSaveData model)
    {
        if (model.Id > 0)
        {
            connection.Delete(model);
        }
    }
}