using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;

namespace Circle;

// 注意static 关键字，意味着它是全局唯一的，不需要 new 就能用
public static class SoundManager
{
    private static IAudioPlayer? _clickPlayer;
    private static bool _isInitialized = false;

    // 全局初始化方法（只执行一次）
    public static async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            var audioStream = await FileSystem.OpenAppPackageFileAsync("BottonSound.mp3");
            _clickPlayer = AudioManager.Current.CreatePlayer(audioStream);
            _clickPlayer.Volume = 0.5;
            _isInitialized = true;
        }
    }

    // 全局播放方法
    public static void PlayClick()
    {
        if (_clickPlayer != null)
        {
            if (_clickPlayer.IsPlaying)
            {
                _clickPlayer.Stop();
            }
            _clickPlayer.Play();
        }
    }
}