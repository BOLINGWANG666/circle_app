using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Linq;
using System.Text.Json; // 引入 JSON 库
using Circle.Models;    // 引入刚建好的数据模型

namespace Circle.Pages;

public partial class BattleFieldPage : ContentPage
{
    private double _timeRemaining = 60.0;
    private bool _isGameOver = false;
    private Circle.ViewModels.CharacterViewModel _viewModel = new Circle.ViewModels.CharacterViewModel();

    private int _fpsCount = 0;
    private DateTime _lastFpsTime = DateTime.Now;

    private Color _playerColor;
    private int _playerMaxHp;
    private int _playerHp;
    private int _playerAtk;
    private double _playerCd;
    private double _playerDodge; // 玩家当前的闪避率
    private bool _isShowingMiss = false; // 防止 Miss 动画重叠

    private int _playerLevel = 0;
    private int _playerExp = 0;
    private int _expToNextLevel = 10;
    private int _framesUntilNextSpawn = 0;
    private DateTime _lastAttackTime = DateTime.Now;
    private DateTime _lastVibrationTime = DateTime.MinValue;
    private Random _rand = new Random();

    private double _moveDirectionX = 0;
    private double _moveDirectionY = 0;
    private double _playerWorldX = 0;
    private double _playerWorldY = 0;
    private readonly double _maxJoystickRadius = 35;
    private IDispatcherTimer _gameLoopTimer;

    private class EnemyInfo
    {
        public required Border UIContainer { get; set; }
        public required BoxView HpBar { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }

        // 区分大红方块和普通小红方块
        public bool IsBig { get; set; } = false;
        // 大红方块的移动方向和转向计时器
        public double MoveDirX { get; set; }
        public double MoveDirY { get; set; }
        public int FramesUntilDirChange { get; set; }

        public int FramesUntilNextShoot { get; set; }
    }

    // 子弹的数据结构与对象池 
    private class ProjectileInfo
    {
        public required Border UIContainer { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public double MoveDirX { get; set; }
        public double MoveDirY { get; set; }
    }
    private List<ProjectileInfo> _activeProjectiles = new List<ProjectileInfo>();
    private Queue<ProjectileInfo> _deadProjectilesPool = new Queue<ProjectileInfo>();

    private class AttackWave
    {
        public required Border UIElement { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public double CurrentRadius { get; set; }
        public double MaxRadius { get; set; }
        public HashSet<int> HitEnemyIds { get; set; } = new HashSet<int>();
    }

    private List<EnemyInfo> _enemiesList = new List<EnemyInfo>();
    private List<AttackWave> _activeWaves = new List<AttackWave>();
    private List<BoxView> _gems = new List<BoxView>();
    private Queue<EnemyInfo> _deadEnemiesPool = new Queue<EnemyInfo>();

    // 大红方块专属的对象池和计时器
    private Queue<EnemyInfo> _deadBigEnemiesPool = new Queue<EnemyInfo>();
    private bool _bigEnemiesInitialized = false;
    private int _framesUntilNextBigSpawn = 0;

    private int _currentSaveSessionId = 0;

    // JSON 数据管理核心变量
    private List<SpellData> _allSpells = new List<SpellData>();
    private List<SpellData> _currentOfferedSpells = new List<SpellData>();
    private SpellData? _selectedSpell = null;

    public BattleFieldPage(Color charColor, int hp, int atk, double cd,double dodge, int charType = 1)
    {
        InitializeComponent();
        _playerColor = charColor;
        _playerMaxHp = hp;
        _playerHp = hp;
        _playerAtk = atk;
        _playerCd = cd;

        //根据传进来的 charType 决定显示圆还是三角
        if (charType == 2)
        {
            PlayerCircle.IsVisible = false;
            PlayerTriangle.IsVisible = true;
            PlayerTriangle.Fill = _playerColor; // 三角形涂色
        }
        else
        {
            PlayerCircle.IsVisible = true;
            PlayerTriangle.IsVisible = false;
            PlayerCircle.BackgroundColor = _playerColor; // 圆形涂色
        }

        HpText.Text = $"{_playerHp}/{_playerMaxHp}";

        PlayerCircle.BackgroundColor = _playerColor;

        HpText.Text = $"{_playerHp}/{_playerMaxHp}";
        HpBar.Progress = 1.0;
        AtkText.Text = $"ATK: {_playerAtk}";
        CdText.Text = $"CD: {_playerCd}s";
        _playerDodge = dodge;
        DodgeText.Text = $"DODGE: {_playerDodge * 100:F0}%";
        ExpText.Text = $"LV{_playerLevel} {_playerExp}/{_expToNextLevel}";
        ExpBar.Progress = 0.0;

        _gameLoopTimer = Dispatcher.CreateTimer();
        _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(12);
        _gameLoopTimer.Tick += GameLoop;
    }

    public BattleFieldPage(Circle.Models.PlayerSaveData saveData, Color charColor)
    {
        InitializeComponent();
        _playerColor = charColor;
        _playerMaxHp = saveData.MaxHp;
        _playerHp = saveData.Hp;
        _playerAtk = saveData.Atk;
        _playerCd = saveData.Cd;

        _playerLevel = saveData.Level;
        _playerExp = saveData.Exp;
        _expToNextLevel = 10 + (_playerLevel * 5);
        _timeRemaining = saveData.TimeRemaining > 0 ? saveData.TimeRemaining : 60.0;
        _currentSaveSessionId = saveData.Id;

        PlayerCircle.BackgroundColor = _playerColor;

        HpText.Text = $"{_playerHp}/{_playerMaxHp}";
        HpBar.Progress = (double)_playerHp / _playerMaxHp;
        AtkText.Text = $"ATK: {_playerAtk}";
        CdText.Text = $"CD: {_playerCd:F1}s";
        _playerDodge = saveData.DodgeChance;
        DodgeText.Text = $"DODGE: {_playerDodge * 100:F0}%";
        ExpText.Text = $"LV{_playerLevel} {_playerExp}/{_expToNextLevel}";
        ExpBar.Progress = (double)_playerExp / _expToNextLevel;
        TimeLabel.Text = Math.Ceiling(_timeRemaining).ToString();

        _gameLoopTimer = Dispatcher.CreateTimer();
        _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(12);
        _gameLoopTimer.Tick += GameLoop;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _activeWaves.Clear();

        // 动态加载 JSON 技能库
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("spells.json");
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();
            _allSpells = JsonSerializer.Deserialize<List<SpellData>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SpellData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading JSON: {ex.Message}");
        }

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
        if (_isGameOver || PausePanel.IsVisible) return;

        _timeRemaining -= 0.012;
        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            TimeLabel.Text = "0";
            if (_playerHp > 0) TriggerVictory();
            return;
        }
        else
        {
            TimeLabel.Text = Math.Ceiling(_timeRemaining).ToString();
        }

        _fpsCount++;
        if ((DateTime.Now - _lastFpsTime).TotalSeconds >= 1.0)
        {
            FpsLabel.Text = $"FPS: {_fpsCount}";
            _fpsCount = 0;
            _lastFpsTime = DateTime.Now;
        }

        if (_moveDirectionX != 0 || _moveDirectionY != 0)
        {
            _playerWorldX += _moveDirectionX * 5.0;
            _playerWorldY += _moveDirectionY * 5.0;
            _playerWorldX = Math.Clamp(_playerWorldX, -722, 722);
            _playerWorldY = Math.Clamp(_playerWorldY, -722, 722);
        }

        double screenWidth = this.Width > 0 ? this.Width : 400;
        double screenHeight = this.Height > 0 ? this.Height : 800;

        double cameraX = Math.Clamp(-_playerWorldX, -(750 - screenWidth / 2), 750 - screenWidth / 2);
        double cameraY = Math.Clamp(-_playerWorldY, -(750 - screenHeight / 2), 750 - screenHeight / 2);

        MapGrid.TranslationX = cameraX;
        MapGrid.TranslationY = cameraY;
        PlayerVisual.TranslationX = _playerWorldX;
        PlayerVisual.TranslationY = _playerWorldY;

        double cullDistX = (screenWidth / 2) + 100;
        double cullDistY = (screenHeight / 2) + 100;
        double screenCenterX = -cameraX;
        double screenCenterY = -cameraY;

        // 45秒后初始化大红方块对象池 
        if (_timeRemaining <= 45.0 && !_bigEnemiesInitialized)
        {
            _bigEnemiesInitialized = true;
            _framesUntilNextBigSpawn = _rand.Next(167, 334); // 2到4秒

            for (int i = 0; i < 15; i++)
            {
                var hpBox = new BoxView { Color = Colors.DarkRed, WidthRequest = 50, HeightRequest = 50, ScaleX = 1.0, AnchorX = 0 };
                var container = new Border { WidthRequest = 50, HeightRequest = 50, Stroke = Colors.Black, StrokeThickness = 2, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, IsVisible = false, Content = hpBox };
                MapGrid.Children.Add(container);
                _deadBigEnemiesPool.Enqueue(new EnemyInfo { UIContainer = container, HpBar = hpBox, WorldX = 0, WorldY = 0, Hp = 60, MaxHp = 60, IsBig = true });
            }
        }

        // 处理大红方块的生成倒计时 
        if (_bigEnemiesInitialized)
        {
            _framesUntilNextBigSpawn--;
            if (_framesUntilNextBigSpawn <= 0 && _deadBigEnemiesPool.Count > 0)
            {
                SpawnBigEnemy();
                _framesUntilNextBigSpawn = _rand.Next(167, 334);
            }
        }

        //小方形生成逻辑
        _framesUntilNextSpawn--;
        if (_framesUntilNextSpawn <= 0 && _enemiesList.Count < 20)
        {
            SpawnEnemy();
            _framesUntilNextSpawn = _rand.Next(40, 80);
        }

        // 新的怪物移动与碰撞逻辑 
        for (int i = _enemiesList.Count - 1; i >= 0; i--)
        {
            var enemy = _enemiesList[i];

            if (enemy.IsBig)
            {
                // 大红方块移动逻辑：随机游走 + 碰壁反弹
                enemy.FramesUntilDirChange--;
                if (enemy.FramesUntilDirChange <= 0)
                {
                    double angle = _rand.NextDouble() * 2 * Math.PI;
                    enemy.MoveDirX = Math.Cos(angle);
                    enemy.MoveDirY = Math.Sin(angle);
                    enemy.FramesUntilDirChange = _rand.Next(250, 417); // 3到5秒换向
                }

                enemy.WorldX += enemy.MoveDirX * 2.0;
                enemy.WorldY += enemy.MoveDirY * 2.0;

                // 碰到地图边缘反弹
                if (enemy.WorldX < -725 || enemy.WorldX > 725) { enemy.MoveDirX *= -1; enemy.WorldX = Math.Clamp(enemy.WorldX, -725, 725); }
                if (enemy.WorldY < -725 || enemy.WorldY > 725) { enemy.MoveDirY *= -1; enemy.WorldY = Math.Clamp(enemy.WorldY, -725, 725); }
                
                
                // 大怪射击倒计时控制 
                enemy.FramesUntilNextShoot--;
                if (enemy.FramesUntilNextShoot <= 0)
                {
                    // 在大怪当前的位置生成一颗子弹
                    SpawnProjectile(enemy.WorldX, enemy.WorldY);
                    // 重置下一次开火的时间为 1到 3秒
                    enemy.FramesUntilNextShoot = _rand.Next(84, 250);
                }
            }
            else
            {
                // 普通小红方块移动逻辑：追逐玩家
                double dx = _playerWorldX - enemy.WorldX;
                double dy = _playerWorldY - enemy.WorldY;
                double distSqToPlayer = dx * dx + dy * dy;

                if (distSqToPlayer > 0.1)
                {
                    double distToPlayer = Math.Sqrt(distSqToPlayer);
                    enemy.WorldX += (dx / distToPlayer) * 1.5;
                    enemy.WorldY += (dy / distToPlayer) * 1.5;
                }
            }

            enemy.UIContainer.TranslationX = enemy.WorldX;
            enemy.UIContainer.TranslationY = enemy.WorldY;

            // 视锥剔除控制显示
            if (Math.Abs(screenCenterX - enemy.WorldX) > cullDistX || Math.Abs(screenCenterY - enemy.WorldY) > cullDistY)
                enemy.UIContainer.IsVisible = false;
            else
                enemy.UIContainer.IsVisible = true;

            // 重合的分离
            double pushX = 0;
            double pushY = 0;
            double myRadius = enemy.IsBig ? 25.0 : 15.0;

            for (int j = 0; j < _enemiesList.Count; j++)
            {
                if (i == j) continue;

                var otherEnemy = _enemiesList[j];
                double otherRadius = otherEnemy.IsBig ? 25.0 : 15.0;
                //计算两个怪物之间的距离
                double ex = enemy.WorldX - otherEnemy.WorldX;
                double ey = enemy.WorldY - otherEnemy.WorldY;
                double distSq = ex * ex + ey * ey;
                //每个敌人安全距离是两者半径之和
                double safeDist = myRadius + otherRadius;

                // 只要发生重叠（距离小于安全距离）
                if (distSq < safeDist * safeDist)
                {
                    // 1. 如果距离几乎为0，给一个随机方向的强震荡力打破平衡
                    if (distSq < 0.001)
                    {
                        pushX += (_rand.NextDouble() - 0.5) * 10;
                        pushY += (_rand.NextDouble() - 0.5) * 10;
                    }
                    else
                    {
                        // 2. 正常排斥：把排斥系数从 0.15 提升到了 0.5
                        double dist = Math.Sqrt(distSq);
                        double overlap = safeDist - dist;
                        // 0.5 意味着怪物会被强行向外推挤重叠部分的一半
                        //归一化向量并乘以重叠距离和排斥系数
                        pushX += (ex / dist) * overlap * 0.5;
                        pushY += (ey / dist) * overlap * 0.5;
                    }
                }
            }

            enemy.WorldX += pushX;
            enemy.WorldY += pushY;
            


            // 碰撞玩家判定
            double pDx = _playerWorldX - enemy.WorldX;
            double pDy = _playerWorldY - enemy.WorldY;
            double pDistSq = pDx * pDx + pDy * pDy;

            // 大方块体型大，碰撞判定范围相应增大
            double collisionThreshold = enemy.IsBig ? 2500 : 1600;

            if (pDistSq < collisionThreshold)
            {
                enemy.UIContainer.IsVisible = false;

                // 死亡后放回各自正确的对象池
                if (enemy.IsBig) _deadBigEnemiesPool.Enqueue(enemy);
                else _deadEnemiesPool.Enqueue(enemy);

                _enemiesList.RemoveAt(i);

                // 大怪扣 20 滴血，小怪扣 5 滴血
                TakeDamage(enemy.IsBig ? 20 : 5);
            }
        }

        // 子弹的飞行、边界消失与碰撞判定 
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            var proj = _activeProjectiles[i];

            // 1. 子弹飞行（速度设定为 4.5）
            proj.WorldX += proj.MoveDirX * 4.5;
            proj.WorldY += proj.MoveDirY * 4.5;
            proj.UIContainer.TranslationX = proj.WorldX;
            proj.UIContainer.TranslationY = proj.WorldY;

            // 2. 如果碰到地图边缘，直接消失并回收
            if (proj.WorldX < -725 || proj.WorldX > 725 || proj.WorldY < -725 || proj.WorldY > 725)
            {
                proj.UIContainer.IsVisible = false;
                _deadProjectilesPool.Enqueue(proj);
                _activeProjectiles.RemoveAt(i);
                continue;
            }

            // 3. 检测是否打中玩家
            double pDx = _playerWorldX - proj.WorldX;
            double pDy = _playerWorldY - proj.WorldY;
            double pDistSq = pDx * pDx + pDy * pDy;

            // 玩家圆圈半径25，子弹半径10
            
            if (pDistSq < 900)
            {
                // 打中玩家后子弹消失回收
                proj.UIContainer.IsVisible = false;
                _deadProjectilesPool.Enqueue(proj);
                _activeProjectiles.RemoveAt(i);

                // 触发玩家扣除 5 点血和震动反馈
                TakeDamage(5);
            }
        }

        for (int i = _gems.Count - 1; i >= 0; i--)
        {
            var gem = _gems[i];
            double dx = _playerWorldX - gem.TranslationX;
            double dy = _playerWorldY - gem.TranslationY;

            gem.IsVisible = !(Math.Abs(screenCenterX - gem.TranslationX) > cullDistX || Math.Abs(screenCenterY - gem.TranslationY) > cullDistY);

            if (dx * dx + dy * dy < 60 * 60)
            {
                MapGrid.Children.Remove(gem);
                _gems.RemoveAt(i);
                GainExp(2);
            }
        }

        if ((DateTime.Now - _lastAttackTime).TotalSeconds >= _playerCd)
        {
            SpawnAttackWave();
            _lastAttackTime = DateTime.Now;
        }

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
            if (wave.UIElement.StrokeShape is RoundRectangle rect) rect.CornerRadius = wave.CurrentRadius;

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

                    if (enemy.Hp <= 0)
                    {
                        DropGem(enemy.WorldX, enemy.WorldY);
                        enemy.UIContainer.IsVisible = false;

                        // 打死大方块放回大方块池，打死小方块放回小方块池
                        if (enemy.IsBig) _deadBigEnemiesPool.Enqueue(enemy);
                        else _deadEnemiesPool.Enqueue(enemy);

                        _enemiesList.RemoveAt(j);
                    }

                    else
                    {
                        // 如果怪物没死，根据剩余血量比例动态缩放红色血条
                        enemy.HpBar.ScaleX = Math.Max(0, (double)enemy.Hp / enemy.MaxHp);
                    }
                }
            }
        }
    }

    private async void TriggerVictory()
    {
        _isGameOver = true;
        _gameLoopTimer.Stop();
        BgmPlayer.Stop();

        if (_currentSaveSessionId > 0)
        {
            var saveToDelete = Circle.ViewModels.CharacterViewModel.Current?.Characters?.FirstOrDefault(c => c.Id == _currentSaveSessionId);
            if (saveToDelete != null) Circle.ViewModels.CharacterViewModel.Current?.DeleteCharacter(saveToDelete);
        }

        await Task.Delay(300);
        await Navigation.PushAsync(new VictoryPage());
    }

    private void SpawnEnemy()
    {
        double x = _rand.Next(-725, 725);
        double y = _rand.Next(-725, 725);
        if ((x - _playerWorldX) * (x - _playerWorldX) + (y - _playerWorldY) * (y - _playerWorldY) < 150 * 150) return;

        if (_deadEnemiesPool.Count > 0)
        {
            var recycledEnemy = _deadEnemiesPool.Dequeue();
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
            var hpBox = new BoxView { Color = Colors.Red, WidthRequest = 30, HeightRequest = 30, ScaleX = 1.0, AnchorX = 0 };
            var container = new Border { WidthRequest = 30, HeightRequest = 30, Stroke = Colors.Black, StrokeThickness = 1, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationX = x, TranslationY = y, Content = hpBox };
            MapGrid.Children.Add(container);
            _enemiesList.Add(new EnemyInfo { UIContainer = container, HpBar = hpBox, WorldX = x, WorldY = y, Hp = 30, MaxHp = 30 });
        }
    }


    // 专门生成随机游走的大红方块 
    private void SpawnBigEnemy()
    {
        double x, y;
        do
        {
            x = _rand.Next(-700, 700);
            y = _rand.Next(-700, 700);
        }
        // 保证出生点不要太靠近玩家 
        while ((x - _playerWorldX) * (x - _playerWorldX) + (y - _playerWorldY) * (y - _playerWorldY) < 40000);

        var bigEnemy = _deadBigEnemiesPool.Dequeue();

        // 重置大红方块的属性
        bigEnemy.Hp = bigEnemy.MaxHp;
        bigEnemy.HpBar.ScaleX = 1.0;
        bigEnemy.WorldX = x;
        bigEnemy.WorldY = y;
        bigEnemy.UIContainer.TranslationX = x;
        bigEnemy.UIContainer.TranslationY = y;
        bigEnemy.UIContainer.IsVisible = true;

        // 随机给一个初始的运动方向角度
        double angle = _rand.NextDouble() * 2 * Math.PI;
        bigEnemy.MoveDirX = Math.Cos(angle);
        bigEnemy.MoveDirY = Math.Sin(angle);
        bigEnemy.FramesUntilDirChange = _rand.Next(84, 250); // 1到3秒的时间更换方向

        // 初始化发射子弹的倒计时（1到3秒）
        bigEnemy.FramesUntilNextShoot = _rand.Next(84, 250);

        _enemiesList.Add(bigEnemy);
    }

    // 生成红色圆形子弹的方法 
    private void SpawnProjectile(double startX, double startY)
    {
        // 随机一个 360 度的发射方向
        double angle = _rand.NextDouble() * 2 * Math.PI;
        double dirX = Math.Cos(angle);
        double dirY = Math.Sin(angle);

        if (_deadProjectilesPool.Count > 0)
        {
            // 从回收池复用子弹
            var proj = _deadProjectilesPool.Dequeue();
            proj.WorldX = startX;
            proj.WorldY = startY;
            proj.MoveDirX = dirX;
            proj.MoveDirY = dirY;
            proj.UIContainer.TranslationX = startX;
            proj.UIContainer.TranslationY = startY;
            proj.UIContainer.IsVisible = true;
            _activeProjectiles.Add(proj);
        }
        else
        {
            // 新建一个红色圆形带黑色边框
            var ui = new Border
            {
                WidthRequest = 20,
                HeightRequest = 20,
                BackgroundColor = Colors.Red,
                Stroke = Colors.Black,
                StrokeThickness = 2,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TranslationX = startX,
                TranslationY = startY
            };
            // 使其变成圆形
            ui.StrokeShape = new RoundRectangle { CornerRadius = 10 };
            MapGrid.Children.Add(ui);

            _activeProjectiles.Add(new ProjectileInfo
            {
                UIContainer = ui,
                WorldX = startX,
                WorldY = startY,
                MoveDirX = dirX,
                MoveDirY = dirY
            });
        }
    }


    private void SpawnAttackWave()
    {
        var waveUI = new Border { WidthRequest = 10, HeightRequest = 10, BackgroundColor = Color.FromRgba(211, 211, 211, 100), Stroke = Colors.Transparent, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationX = _playerWorldX, TranslationY = _playerWorldY };
        waveUI.StrokeShape = new RoundRectangle { CornerRadius = 5 };
        MapGrid.Children.Add(waveUI);
        _activeWaves.Add(new AttackWave { UIElement = waveUI, WorldX = _playerWorldX, WorldY = _playerWorldY, CurrentRadius = 5, MaxRadius = 180 });
    }

    private void DropGem(double x, double y)
    {
        var gem = new BoxView { Color = Colors.Blue, WidthRequest = 16, HeightRequest = 16, Rotation = 45, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationX = x, TranslationY = y };
        MapGrid.Children.Add(gem);
        _gems.Add(gem);
    }

    private async void ShowDamageText(int damage)
    {
        var dmgLabel = new Label { Text = $"-{damage}", TextColor = Colors.Red, FontAttributes = FontAttributes.Bold, FontSize = 22, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationX = _playerWorldX, TranslationY = _playerWorldY - 30 };
        MapGrid.Children.Add(dmgLabel);
        _ = dmgLabel.TranslateToAsync(dmgLabel.TranslationX, dmgLabel.TranslationY - 50, 600, Easing.CubicOut);
        await dmgLabel.FadeToAsync(0, 600, Easing.CubicIn);
        MapGrid.Children.Remove(dmgLabel);
    }

    private async void TakeDamage(int amount)
    {
        if (_playerHp <= 0 || _isGameOver || PausePanel.IsVisible) return;


        // 闪避判定逻辑
        //怪物碰到玩家，先生成一个 0-1 的随机数，如果小于闪避率，则Miss
        if (_rand.NextDouble() < _playerDodge)
        {
            ShowMissText(); // 触发 Miss 动画
            return;         // 直接 return，免除后续的扣血和震动
        }
        

        _playerHp -= amount;
        ShowDamageText(amount);
        _playerHp -= amount;
        ShowDamageText(amount);

        // 处理手机震动反馈 1秒冷却
        if ((DateTime.Now - _lastVibrationTime).TotalSeconds >= 1.0)
        {
            try
            {
                // 让手机震动 150 毫秒
                Microsoft.Maui.Devices.Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(150));
            }
            catch
            {
                // 忽略不支持震动的设备抛出的异常
            }

            _lastVibrationTime = DateTime.Now; // 更新上次震动时间
        }

        if (_playerHp <= 0)
        {
            _playerHp = 0;
            _isGameOver = true;
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

    private async void ShowMissText()
    {
        // 防止屏幕被 Miss 刷屏重叠
        if (_isShowingMiss) return;
        _isShowingMiss = true;

        var missLabel = new Label { Text = "Miss", TextColor = Colors.LimeGreen, FontAttributes = FontAttributes.Bold, FontSize = 22, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TranslationX = _playerWorldX, TranslationY = _playerWorldY - 30 };
        MapGrid.Children.Add(missLabel);

        _ = missLabel.TranslateToAsync(missLabel.TranslationX, missLabel.TranslationY - 50, 600, Easing.CubicOut);
        await missLabel.FadeToAsync(0, 600, Easing.CubicIn);

        MapGrid.Children.Remove(missLabel);

        // 动画完全消失后，解锁允许下一次显示
        _isShowingMiss = false;
    }

    private void GainExp(int amount)
    {
        if (_isGameOver || PausePanel.IsVisible) return;

        _playerExp += amount;
        if (_playerExp >= _expToNextLevel)
        {
            _playerExp -= _expToNextLevel;
            _playerLevel++;
            _expToNextLevel = 10 + (_playerLevel * 5);

            _gameLoopTimer.Stop();

            // 使用 Guid.NewGuid() 进行真随机洗牌
            if (_allSpells.Count >= 3)
            {
                _currentOfferedSpells = _allSpells.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

                Card1Icon.Text = _currentOfferedSpells[0].Icon;
                Card1Name.Text = _currentOfferedSpells[0].Name;
                Card1Desc.Text = _currentOfferedSpells[0].Description;

                Card2Icon.Text = _currentOfferedSpells[1].Icon;
                Card2Name.Text = _currentOfferedSpells[1].Name;
                Card2Desc.Text = _currentOfferedSpells[1].Description;

                Card3Icon.Text = _currentOfferedSpells[2].Icon;
                Card3Name.Text = _currentOfferedSpells[2].Name;
                Card3Desc.Text = _currentOfferedSpells[2].Description;
            }

            PauseButton.IsEnabled = false;
            PauseButton.Opacity = 0.5;
            LevelUpPanel.IsVisible = true;
        }

        ExpText.Text = $"LV{_playerLevel} {_playerExp}/{_expToNextLevel}";
        ExpBar.Progress = (double)_playerExp / _expToNextLevel;
    }


    //  精简的点击总控
    private async void OnCardTapped(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        Border tappedCard = (Border)sender;

        if (tappedCard == Card1) await SelectCard(Card1, _currentOfferedSpells[0]);
        else if (tappedCard == Card2) await SelectCard(Card2, _currentOfferedSpells[1]);
        else if (tappedCard == Card3) await SelectCard(Card3, _currentOfferedSpells[2]);
    }

    private async Task SelectCard(Border selectedCard, SpellData spell)
    {
        _selectedSpell = spell;

        Card1.BackgroundColor = Color.Parse("#222222");
        Card2.BackgroundColor = Color.Parse("#222222");
        Card3.BackgroundColor = Color.Parse("#222222");

        selectedCard.BackgroundColor = Color.Parse("#555555");

        ConfirmUpgradeButton.IsEnabled = true;
        ConfirmUpgradeButton.BackgroundColor = Colors.Green;

        await selectedCard.ScaleToAsync(0.9, 100, Easing.CubicOut);
        await selectedCard.ScaleToAsync(1.05, 100, Easing.CubicIn);
        await selectedCard.ScaleToAsync(1.0, 100, Easing.SpringOut);
    }

    private void OnConfirmUpgradeClicked(object sender, EventArgs e)
    {
        if (_selectedSpell == null) return;

        // 动态计算属性，不再写死
        switch (_selectedSpell.EffectType)
        {
            case "Atk":
                _playerAtk += (int)_selectedSpell.Value;
                break;
            case "Cd":
                _playerCd += _selectedSpell.Value;
                if (_playerCd < 0.2) _playerCd = 0.2;
                break;
            case "MaxHp":
                _playerMaxHp += (int)_selectedSpell.Value;
                _playerHp += (int)_selectedSpell.Value;
                break;
            case "Dodge":
                _playerDodge += _selectedSpell.Value; 
                break;
            case "Heal":
                _playerHp += (int)_selectedSpell.Value;
                if (_playerHp > _playerMaxHp) _playerHp = _playerMaxHp;
                break;
        }

        AtkText.Text = $"ATK: {_playerAtk}";
        DodgeText.Text = $"DODGE: {_playerDodge * 100:F0}%";
        CdText.Text = $"CD: {_playerCd:F1}s";
        HpText.Text = $"{_playerHp}/{_playerMaxHp}";
        HpBar.Progress = (double)_playerHp / _playerMaxHp;

        var snapshot = new Circle.Models.PlayerSaveData
        {
            Id = _currentSaveSessionId,
            Hp = _playerHp,
            MaxHp = _playerMaxHp,
            Atk = _playerAtk,
            Cd = _playerCd,
            DodgeChance = _playerDodge,
            Level = _playerLevel,
            Exp = _playerExp,
            TimeRemaining = _timeRemaining
        };
        Circle.ViewModels.CharacterViewModel.Current?.SaveCharacter(snapshot);

        if (_currentSaveSessionId == 0) _currentSaveSessionId = snapshot.Id;

        LevelUpPanel.IsVisible = false;
        _selectedSpell = null;

        ConfirmUpgradeButton.IsEnabled = false;
        ConfirmUpgradeButton.BackgroundColor = Colors.Gray;
        Card1.BackgroundColor = Color.Parse("#222222");
        Card2.BackgroundColor = Color.Parse("#222222");
        Card3.BackgroundColor = Color.Parse("#222222");

        PauseButton.IsEnabled = true;
        PauseButton.Opacity = 1.0;

        _gameLoopTimer.Start();
    }

    // 暂停与交互方法
    private async void OnPauseClicked(object sender, EventArgs e)
    {
        if (_isGameOver) return;
        SoundManager.PlayClick();
        await PauseButton.ScaleToAsync(0.9, 100);
        await PauseButton.ScaleToAsync(1.0, 100);
        _gameLoopTimer.Stop();
        BgmPlayer.Pause();
        JoystickContainer.InputTransparent = true;
        MapGrid.InputTransparent = true;
        PausePanel.IsVisible = true;
    }

    private async void OnResumeClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        await ResumeBtn.ScaleToAsync(0.9, 100);
        await ResumeBtn.ScaleToAsync(1.0, 100);
        PausePanel.IsVisible = false;
        JoystickContainer.InputTransparent = false;
        MapGrid.InputTransparent = false;
        BgmPlayer.Play();
        _gameLoopTimer.Start();
    }

    private async void OnRestartInPauseClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        await RestartBtnInPause.ScaleToAsync(0.9, 100);
        await RestartBtnInPause.ScaleToAsync(1.0, 100);

        bool confirm = await DisplayAlertAsync("Restart Game", "Are you sure you want to restart?\nThis will DELETE your current session's save file!", "Delete & Restart", "Cancel");
        if (!confirm) return;

        _gameLoopTimer.Stop();
        BgmPlayer.Stop();

        if (_currentSaveSessionId > 0)
        {
            var saveToDelete = Circle.ViewModels.CharacterViewModel.Current?.Characters?.FirstOrDefault(c => c.Id == _currentSaveSessionId);
            if (saveToDelete != null) Circle.ViewModels.CharacterViewModel.Current?.DeleteCharacter(saveToDelete);
        }

        await Navigation.PopAsync();
    }

    // 点击暂停面板左侧的“退出”直接停止应用进程
    private async void OnQuitInPauseClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();

        
        if (this.FindByName<Border>("QuitBtnInPause") is Border quitBtn)
        {
            await quitBtn.ScaleToAsync(0.9, 100);
            await quitBtn.ScaleToAsync(1.0, 100);
        }

        bool confirm = await DisplayAlertAsync("Exit Game", "Are you sure you want to exit entirely?\nYour progress is safely stored.", "Exit", "Cancel");
        if (!confirm) return;

        _gameLoopTimer.Stop();
        BgmPlayer.Stop();

        // 强行关闭进程，退出应用
        Application.Current?.Quit();
    }
}