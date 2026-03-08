using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Circle.Pages;

public partial class BattleFieldPage : ContentPage
{
    // --- 性能监控变量 ---
    private int _fpsCount = 0;
    private DateTime _lastFpsTime = DateTime.Now;
    // --- 玩家基础属性 ---
    private Color _playerColor;
    private int _playerMaxHp;
    private int _playerHp;
    private int _playerAtk;
    private double _playerCd;

    // --- 游戏系统变量 ---
    private int _playerLevel = 0;
    private int _playerExp = 0;
    private int _expToNextLevel = 10;
    private int _framesUntilNextSpawn = 0;
    private DateTime _lastAttackTime = DateTime.Now;
    private Random _rand = new Random();

    // --- 摇杆与坐标 ---
    private double _moveDirectionX = 0;
    private double _moveDirectionY = 0;
    private double _playerWorldX = 0;
    private double _playerWorldY = 0;
    private readonly double _maxJoystickRadius = 35;
    private IDispatcherTimer _gameLoopTimer;

    // ==================== 实体数据结构 ====================
    private class EnemyInfo
    {
        public required Border UIContainer { get; set; }
        public required BoxView HpBar { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
    }

    private class AttackWave
    {
        public required Border UIElement { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public double CurrentRadius { get; set; }
        public double MaxRadius { get; set; }
        public HashSet<int> HitEnemyIds { get; set; } = new HashSet<int>();
    }

    // --- 场上实体列表 ---
    private List<EnemyInfo> _enemiesList = new List<EnemyInfo>();
    private List<AttackWave> _activeWaves = new List<AttackWave>();
    private List<BoxView> _gems = new List<BoxView>();

    // 死亡敌人回收站 (对象池)
    private Queue<EnemyInfo> _deadEnemiesPool = new Queue<EnemyInfo>();

    public BattleFieldPage(Color charColor, int hp, int atk, double cd)
    {
        InitializeComponent();

        _playerColor = charColor;
        _playerMaxHp = hp;
        _playerHp = hp;
        _playerAtk = atk;
        _playerCd = cd;

        PlayerCircle.BackgroundColor = _playerColor;

        HpText.Text = $"{_playerHp}/{_playerMaxHp}";
        HpBar.Progress = 1.0;
        AtkText.Text = $"ATK: {_playerAtk}";
        CdText.Text = $"CD: {_playerCd}s";
        ExpText.Text = $"LV{_playerLevel} {_playerExp}/{_expToNextLevel}";
        ExpBar.Progress = 0.0;

        //每帧生成时间
        _gameLoopTimer = Dispatcher.CreateTimer();
        _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(12);
        _gameLoopTimer.Tick += GameLoop;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _activeWaves.Clear();
        BgmPlayer.Source = MediaSource.FromResource("BGM.mp3");
        _gameLoopTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BgmPlayer.Stop();
        _activeWaves.Clear();
        _gameLoopTimer.Stop();
    }

    private void OnJoystickPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                double dx = e.TotalX;
                double dy = e.TotalY;
                double distanceSq = dx * dx + dy * dy;

                if (distanceSq > _maxJoystickRadius * _maxJoystickRadius)
                {
                    double distance = Math.Sqrt(distanceSq);
                    double ratio = _maxJoystickRadius / distance;
                    dx *= ratio;
                    dy *= ratio;
                }

                JoystickInner.TranslationX = dx;
                JoystickInner.TranslationY = dy;
                _moveDirectionX = dx / _maxJoystickRadius;
                _moveDirectionY = dy / _maxJoystickRadius;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _ = JoystickInner.TranslateToAsync(0, 0, 150, Easing.BounceOut);
                _moveDirectionX = 0;
                _moveDirectionY = 0;
                break;
        }
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        // ================= 0. FPS 性能统计 =================
        _fpsCount++;
        if ((DateTime.Now - _lastFpsTime).TotalSeconds >= 1.0)
        {
            FpsLabel.Text = $"FPS: {_fpsCount}";
            _fpsCount = 0;
            _lastFpsTime = DateTime.Now;
        }
        // 1. 玩家移动与摄像机
        if (_moveDirectionX != 0 || _moveDirectionY != 0)
        {
            _playerWorldX += _moveDirectionX * 5.0;
            _playerWorldY += _moveDirectionY * 5.0;
            _playerWorldX = Math.Clamp(_playerWorldX, -725, 725);
            _playerWorldY = Math.Clamp(_playerWorldY, -725, 725);
        }

        double screenWidth = this.Width > 0 ? this.Width : 400;
        double screenHeight = this.Height > 0 ? this.Height : 800;

        double cameraX = Math.Clamp(-_playerWorldX, -(750 - screenWidth / 2), 750 - screenWidth / 2);
        double cameraY = Math.Clamp(-_playerWorldY, -(750 - screenHeight / 2), 750 - screenHeight / 2);

        MapGrid.TranslationX = cameraX;
        MapGrid.TranslationY = cameraY;
        PlayerCircle.TranslationX = _playerWorldX;
        PlayerCircle.TranslationY = _playerWorldY;

        double cullDistX = (screenWidth / 2) + 100;
        double cullDistY = (screenHeight / 2) + 100;

        // 2. 敌人生成
        _framesUntilNextSpawn--;
        // 屏幕上的敌人 + 回收站的敌人，总数绝对不会无限制增长
        if (_framesUntilNextSpawn <= 0 && _enemiesList.Count < 50)
        {
            SpawnEnemy();
            _framesUntilNextSpawn = _rand.Next(15, 46);
        }

        // 3. 敌人移动与伤害玩家
        for (int i = _enemiesList.Count - 1; i >= 0; i--)
        {
            var enemy = _enemiesList[i];
            double dx = _playerWorldX - enemy.WorldX;
            double dy = _playerWorldY - enemy.WorldY;
            double distSq = dx * dx + dy * dy;

            if (distSq > 0.1)
            {
                double dist = Math.Sqrt(distSq);
                enemy.WorldX += (dx / dist) * 1.5;
                enemy.WorldY += (dy / dist) * 1.5;
                enemy.UIContainer.TranslationX = enemy.WorldX;
                enemy.UIContainer.TranslationY = enemy.WorldY;
            }

            // 视锥剔除
            if (Math.Abs(dx) > cullDistX || Math.Abs(dy) > cullDistY)
            {
                enemy.UIContainer.IsVisible = false;
            }
            else
            {
                enemy.UIContainer.IsVisible = true;
            }

            // 物理碰撞：主角碰到敌人
            if (distSq < 40 * 40)
            {
                // 改为隐身并放入回收站！
                enemy.UIContainer.IsVisible = false;
                _deadEnemiesPool.Enqueue(enemy);
                _enemiesList.RemoveAt(i);

                TakeDamage(5);
            }
        }

        // 4. 经验拾取逻辑
        for (int i = _gems.Count - 1; i >= 0; i--)
        {
            var gem = _gems[i];
            double dx = _playerWorldX - gem.TranslationX;
            double dy = _playerWorldY - gem.TranslationY;

            gem.IsVisible = !(Math.Abs(dx) > cullDistX || Math.Abs(dy) > cullDistY);

            if (dx * dx + dy * dy < 60 * 60)
            {
                MapGrid.Children.Remove(gem);
                _gems.RemoveAt(i);
                GainExp(2);
            }
        }

        // 5. 发射攻击波
        if ((DateTime.Now - _lastAttackTime).TotalSeconds >= _playerCd)
        {
            SpawnAttackWave();
            _lastAttackTime = DateTime.Now;
        }

        // 6. 攻击波扩散与判定
        for (int i = _activeWaves.Count - 1; i >= 0; i--)
        {
            var wave = _activeWaves[i];
            wave.CurrentRadius += 6.0;

            if (wave.CurrentRadius >= wave.MaxRadius)
            {
                MapGrid.Children.Remove(wave.UIElement);
                _activeWaves.RemoveAt(i);
                continue;
            }

            double size = wave.CurrentRadius * 2;
            wave.UIElement.WidthRequest = size;
            wave.UIElement.HeightRequest = size;

            if (wave.UIElement.StrokeShape is RoundRectangle rect)
            {
                rect.CornerRadius = wave.CurrentRadius;
            }

            wave.UIElement.Opacity = 1.0 - (wave.CurrentRadius / wave.MaxRadius);

            wave.UIElement.TranslationX = _playerWorldX;
            wave.UIElement.TranslationY = _playerWorldY;
            wave.WorldX = _playerWorldX;
            wave.WorldY = _playerWorldY;

            double waveRadiusSq = (wave.CurrentRadius + 15) * (wave.CurrentRadius + 15);
            for (int j = _enemiesList.Count - 1; j >= 0; j--)
            {
                var enemy = _enemiesList[j];
                int enemyId = enemy.UIContainer.GetHashCode();

                if (wave.HitEnemyIds.Contains(enemyId)) continue;

                double dx = wave.WorldX - enemy.WorldX;
                double dy = wave.WorldY - enemy.WorldY;

                if (dx * dx + dy * dy <= waveRadiusSq)
                {
                    wave.HitEnemyIds.Add(enemyId);
                    enemy.Hp -= _playerAtk;

                    //  物理碰撞：敌人被冲击波打死
                    if (enemy.Hp <= 0)
                    {
                        DropGem(enemy.WorldX, enemy.WorldY);

                        // 【绝对不用 Remove】改为隐身并放入回收站！
                        enemy.UIContainer.IsVisible = false;
                        _deadEnemiesPool.Enqueue(enemy);
                        _enemiesList.RemoveAt(j);
                    }
                    else
                    {
                        enemy.HpBar.ScaleX = (double)enemy.Hp / enemy.MaxHp;
                    }
                }
            }
        }
    }

    // ==================== 辅助方法 ====================

    // 真正使用对象池的生成逻辑
    private void SpawnEnemy()
    {
        double x = _rand.Next(-725, 725);
        double y = _rand.Next(-725, 725);
        if ((x - _playerWorldX) * (x - _playerWorldX) + (y - _playerWorldY) * (y - _playerWorldY) < 150 * 150) return;

        
        if (_deadEnemiesPool.Count > 0)
        {
            var recycledEnemy = _deadEnemiesPool.Dequeue();

            // 重置状态：满血、血条拉满、更新坐标、取消隐身
            recycledEnemy.Hp = recycledEnemy.MaxHp;
            recycledEnemy.HpBar.ScaleX = 1.0;
            recycledEnemy.WorldX = x;
            recycledEnemy.WorldY = y;
            recycledEnemy.UIContainer.TranslationX = x;
            recycledEnemy.UIContainer.TranslationY = y;
            recycledEnemy.UIContainer.IsVisible = true;

            _enemiesList.Add(recycledEnemy);
        }
        else
        {
            // 回收站空了（比如游戏刚开始），才真正消耗内存去 new 控件
            var hpBox = new BoxView
            {
                Color = Colors.Red,
                WidthRequest = 30,
                HeightRequest = 30,
                ScaleX = 1.0,
                AnchorX = 0
            };

            var container = new Border
            {
                WidthRequest = 30,
                HeightRequest = 30,
                Stroke = Colors.Black,
                StrokeThickness = 1,
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TranslationX = x,
                TranslationY = y,
                Content = hpBox
            };

            MapGrid.Children.Add(container);
            _enemiesList.Add(new EnemyInfo { UIContainer = container, HpBar = hpBox, WorldX = x, WorldY = y, Hp = 30, MaxHp = 30 });
        }
    }

    private void SpawnAttackWave()
    {
        var waveUI = new Border
        {
            WidthRequest = 10,
            HeightRequest = 10,
            BackgroundColor = Color.FromRgba(211, 211, 211, 100),
            Stroke = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationX = _playerWorldX,
            TranslationY = _playerWorldY
        };
        waveUI.StrokeShape = new RoundRectangle { CornerRadius = 5 };

        MapGrid.Children.Add(waveUI);
        _activeWaves.Add(new AttackWave
        {
            UIElement = waveUI,
            WorldX = _playerWorldX,
            WorldY = _playerWorldY,
            CurrentRadius = 5,
            MaxRadius = 180
        });
    }

    private void DropGem(double x, double y)
    {
        var gem = new BoxView
        {
            Color = Colors.Blue,
            WidthRequest = 16,
            HeightRequest = 16,
            Rotation = 45,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationX = x,
            TranslationY = y
        };
        MapGrid.Children.Add(gem);
        _gems.Add(gem);
    }

    private async void ShowDamageText(int damage)
    {
        var dmgLabel = new Label
        {
            Text = $"-{damage}",
            TextColor = Colors.Red,
            FontAttributes = FontAttributes.Bold,
            FontSize = 22,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationX = _playerWorldX,
            TranslationY = _playerWorldY - 30
        };

        MapGrid.Children.Add(dmgLabel);
        _ = dmgLabel.TranslateToAsync(dmgLabel.TranslationX, dmgLabel.TranslationY - 50, 600, Easing.CubicOut);
        await dmgLabel.FadeToAsync(0, 600, Easing.CubicIn);
        MapGrid.Children.Remove(dmgLabel);
    }

    private async void TakeDamage(int amount)
    {
        if (_playerHp <= 0) return;

        _playerHp -= amount;
        ShowDamageText(amount);

        if (_playerHp <= 0)
        {
            _playerHp = 0;
            HpText.Text = $"{_playerHp}/{_playerMaxHp}";
            HpBar.Progress = 0;

            _gameLoopTimer.Stop();

            BgmPlayer.Stop();

            await Task.Delay(300);
            await Navigation.PushAsync(new FailedPage());
        }
        else
        {
            HpText.Text = $"{_playerHp}/{_playerMaxHp}";
            HpBar.Progress = (double)_playerHp / _playerMaxHp;
        }
    }

    private void GainExp(int amount)
    {
        _playerExp += amount;
        if (_playerExp >= _expToNextLevel)
        {
            _playerExp -= _expToNextLevel;
            _playerLevel++;
            _expToNextLevel = 10 + (_playerLevel * 5);
        }
        ExpText.Text = $"LV{_playerLevel} {_playerExp}/{_expToNextLevel}";
        ExpBar.Progress = (double)_playerExp / _expToNextLevel;
    }
}