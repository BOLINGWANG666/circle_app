using Plugin.Maui.Audio;

namespace Circle;

// Note the static keyword, which means it is globally unique and can be used without new
public static class SoundManager
{
    private static IAudioPlayer? _clickPlayer;
    private static bool _isInitialized = false;

    // Global initialization method (executed only once)
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

    // Global play method
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