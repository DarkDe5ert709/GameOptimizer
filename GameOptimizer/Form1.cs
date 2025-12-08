using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;

namespace GameOptimizer
{
    public partial class Form1 : Form
    {
        // Структура для хранения информации о системе
        public class SystemInfo
        {
            public string CPUName { get; set; }
            public int CPUCores { get; set; }
            public int CPUThreads { get; set; }
            public string GPUName { get; set; }
            public long RAMTotalGB { get; set; }
            public string SystemLevel { get; set; }
            public float SystemScore { get; set; }
        }

        // Структура для хранения информации об игре
        public class GameInfo
        {
            public string Name { get; set; }
            public string ExecutableName { get; set; }
            public string ConfigPath { get; set; }
            public string Engine { get; set; }
            public bool IsInstalled { get; set; }
        }

        private SystemInfo currentSystemInfo;
        private List<GameInfo> availableGames;
        private GameInfo selectedGame;

        // Контролы UI
        private Panel systemInfoPanel;
        private FlowLayoutPanel gamesListPanel;
        private Panel gameDetailsPanel;
        private Button optimizeButton;
        private ProgressBar progressBar;
        private Label statusLabel;

        public Form1()
        {
            InitializeComponent();
            CreateUI(); // Создаем интерфейс
            LoadSystemInfo();
            InitializeGamesList();
        }

        private void CreateUI()
        {
            // =========== 1. ОСНОВНЫЕ НАСТРОЙКИ ФОРМЫ ===========
            this.Text = "Game Optimizer Pro";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 40);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            // =========== 2. ЗАГОЛОВОК ===========
            Panel headerPanel = new Panel();
            headerPanel.Name = "headerPanel";
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.FromArgb(45, 45, 60);

            Label titleLabel = new Label();
            titleLabel.Text = "🎮 GAME OPTIMIZER PRO";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Dock = DockStyle.Left;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.Padding = new Padding(20, 0, 0, 0);

            headerPanel.Controls.Add(titleLabel);
            this.Controls.Add(headerPanel);

            // =========== 3. ОСНОВНОЙ КОНТЕЙНЕР ===========
            TableLayoutPanel mainContainer = new TableLayoutPanel();
            mainContainer.Name = "mainContainer";
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.ColumnCount = 2;
            mainContainer.RowCount = 1;
            mainContainer.Padding = new Padding(10);

            // Левая колонка - 40%
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            // Правая колонка - 60%
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // =========== 4. ЛЕВАЯ ПАНЕЛЬ (ИНФОРМАЦИЯ О СИСТЕМЕ) ===========
            Panel leftPanel = new Panel();
            leftPanel.Name = "leftPanel";
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.BackColor = Color.FromArgb(40, 40, 55);
            leftPanel.Padding = new Padding(15);

            // Заголовок левой панели
            Label systemLabel = new Label();
            systemLabel.Text = "💻 СИСТЕМНАЯ ИНФОРМАЦИЯ";
            systemLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            systemLabel.ForeColor = Color.Cyan;
            systemLabel.Dock = DockStyle.Top;
            systemLabel.Height = 40;

            // Панель для информации о системе
            systemInfoPanel = new Panel();
            systemInfoPanel.Name = "systemInfoPanel";
            systemInfoPanel.Dock = DockStyle.Fill;
            systemInfoPanel.AutoScroll = true;

            leftPanel.Controls.Add(systemInfoPanel);
            leftPanel.Controls.Add(systemLabel);

            // =========== 5. ПРАВАЯ ПАНЕЛЬ (ИГРЫ) ===========
            Panel rightPanel = new Panel();
            rightPanel.Name = "rightPanel";
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(35, 35, 50);
            rightPanel.Padding = new Padding(15);

            // Заголовок правой панели
            Label gamesLabel = new Label();
            gamesLabel.Text = "🎯 ДОСТУПНЫЕ ИГРЫ";
            gamesLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            gamesLabel.ForeColor = Color.LightGreen;
            gamesLabel.Dock = DockStyle.Top;
            gamesLabel.Height = 40;

            // Панель списка игр
            gamesListPanel = new FlowLayoutPanel();
            gamesListPanel.Name = "gamesListPanel";
            gamesListPanel.Dock = DockStyle.Top;
            gamesListPanel.Height = 200;
            gamesListPanel.AutoScroll = true;
            gamesListPanel.BackColor = Color.FromArgb(50, 50, 65);
            gamesListPanel.Padding = new Padding(10);
            gamesListPanel.WrapContents = false;
            gamesListPanel.FlowDirection = FlowDirection.TopDown;

            // Панель деталей игры
            gameDetailsPanel = new Panel();
            gameDetailsPanel.Name = "gameDetailsPanel";
            gameDetailsPanel.Dock = DockStyle.Fill;
            gameDetailsPanel.BackColor = Color.FromArgb(45, 45, 60);
            gameDetailsPanel.Padding = new Padding(15);

            rightPanel.Controls.Add(gameDetailsPanel);
            rightPanel.Controls.Add(gamesListPanel);
            rightPanel.Controls.Add(gamesLabel);

            // =========== 6. ДОБАВЛЯЕМ ПАНЕЛИ В КОНТЕЙНЕР ===========
            mainContainer.Controls.Add(leftPanel, 0, 0);
            mainContainer.Controls.Add(rightPanel, 1, 0);

            // =========== 7. НИЖНЯЯ ПАНЕЛЬ (КНОПКА И СТАТУС) ===========
            Panel bottomPanel = new Panel();
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 80;
            bottomPanel.BackColor = Color.FromArgb(40, 40, 55);
            bottomPanel.Padding = new Padding(20);

            // Кнопка оптимизации
            optimizeButton = new Button();
            optimizeButton.Name = "optimizeButton";
            optimizeButton.Text = "⚡ ОПТИМИЗИРОВАТЬ";
            optimizeButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            optimizeButton.ForeColor = Color.White;
            optimizeButton.BackColor = Color.FromArgb(70, 130, 180);
            optimizeButton.FlatStyle = FlatStyle.Flat;
            optimizeButton.Height = 45;
            optimizeButton.Width = 200;
            optimizeButton.Dock = DockStyle.Right;
            optimizeButton.Click += OptimizeButton_Click;
            optimizeButton.FlatAppearance.BorderSize = 0;

            // Прогресс бар
            progressBar = new ProgressBar();
            progressBar.Name = "progressBar";
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 20;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;

            // Статус
            statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Готов к оптимизации";
            statusLabel.Dock = DockStyle.Left;
            statusLabel.ForeColor = Color.LightGray;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.Font = new Font("Segoe UI", 9);

            bottomPanel.Controls.Add(optimizeButton);
            bottomPanel.Controls.Add(progressBar);
            bottomPanel.Controls.Add(statusLabel);

            // =========== 8. ДОБАВЛЯЕМ ВСЕ НА ФОРМУ ===========
            this.Controls.Add(mainContainer);
            this.Controls.Add(bottomPanel);
        }

        private void LoadSystemInfo()
        {
            currentSystemInfo = new SystemInfo();

            try
            {
                // =========== СПОСОБ 1: Через WMI (основной) ===========
                // Получаем информацию о CPU
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        currentSystemInfo.CPUName = obj["Name"].ToString();
                        currentSystemInfo.CPUCores = int.Parse(obj["NumberOfCores"].ToString());
                        currentSystemInfo.CPUThreads = int.Parse(obj["NumberOfLogicalProcessors"].ToString());
                        break;
                    }
                }

                // Получаем информацию о RAM
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                        currentSystemInfo.RAMTotalGB = (long)(totalBytes / (1024 * 1024 * 1024));
                        break;
                    }
                }

                // Получаем информацию о GPU
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        currentSystemInfo.GPUName = obj["Name"].ToString();
                        break;
                    }
                }

                // Рассчитываем уровень системы
                CalculateSystemLevel();

                // Обновляем UI
                UpdateSystemInfoUI();
            }
            catch (Exception ex)
            {
                // =========== СПОСОБ 2: Если WMI не работает ===========
                MessageBox.Show($"WMI не работает, использую тестовые данные. Ошибка: {ex.Message}",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Тестовые данные для вашей системы
                currentSystemInfo.CPUName = "Intel Core i5-9400F @ 2.90GHz";
                currentSystemInfo.CPUCores = 6;
                currentSystemInfo.CPUThreads = 6;
                currentSystemInfo.GPUName = "NVIDIA GeForce GTX 1660 SUPER";
                currentSystemInfo.RAMTotalGB = 32;

                CalculateSystemLevel();
                UpdateSystemInfoUI();
            }
        }

        private void CalculateSystemLevel()
        {
            float score = 0;

            if (currentSystemInfo == null) return;

            // Оценка CPU (0-40 баллов)
            string cpuName = currentSystemInfo.CPUName.ToUpper();
            if (cpuName.Contains("I9") || cpuName.Contains("RYZEN 9"))
                score += 40;
            else if (cpuName.Contains("I7") || cpuName.Contains("RYZEN 7"))
                score += 35;
            else if (cpuName.Contains("I5") || cpuName.Contains("RYZEN 5"))
                score += 30;
            else if (cpuName.Contains("I3") || cpuName.Contains("RYZEN 3"))
                score += 20;
            else
                score += 15;

            // Оценка GPU (0-40 баллов)
            string gpuName = currentSystemInfo.GPUName.ToUpper();
            if (gpuName.Contains("RTX 40") || gpuName.Contains("4090") || gpuName.Contains("4080"))
                score += 40;
            else if (gpuName.Contains("RTX 30") || gpuName.Contains("3080") || gpuName.Contains("3070"))
                score += 35;
            else if (gpuName.Contains("RTX 20") || gpuName.Contains("2080") || gpuName.Contains("2070"))
                score += 30;
            else if (gpuName.Contains("1660") || gpuName.Contains("RTX 2060"))
                score += 25;
            else if (gpuName.Contains("1060") || gpuName.Contains("1650"))
                score += 20;
            else
                score += 10;

            // Оценка RAM (0-20 баллов)
            if (currentSystemInfo.RAMTotalGB >= 32)
                score += 20;
            else if (currentSystemInfo.RAMTotalGB >= 16)
                score += 15;
            else if (currentSystemInfo.RAMTotalGB >= 8)
                score += 10;
            else
                score += 5;

            currentSystemInfo.SystemScore = score;

            // Определяем уровень
            if (score >= 80)
                currentSystemInfo.SystemLevel = "ВЫСОКИЙ";
            else if (score >= 60)
                currentSystemInfo.SystemLevel = "СРЕДНИЙ";
            else if (score >= 40)
                currentSystemInfo.SystemLevel = "НИЗКИЙ";
            else
                currentSystemInfo.SystemLevel = "МИНИМАЛЬНЫЙ";
        }

        private void UpdateSystemInfoUI()
        {
            if (systemInfoPanel == null || currentSystemInfo == null)
                return;

            systemInfoPanel.Controls.Clear();

            int y = 10;

            // CPU информация
            Label cpuLabel = CreateInfoLabel("💻 Процессор:", currentSystemInfo.CPUName);
            cpuLabel.Location = new Point(10, y);
            cpuLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(cpuLabel);
            y += cpuLabel.Height + 5;

            Label coresLabel = CreateInfoLabel("   Ядра/Потоки:", $"{currentSystemInfo.CPUCores}/{currentSystemInfo.CPUThreads}");
            coresLabel.Location = new Point(10, y);
            coresLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(coresLabel);
            y += coresLabel.Height + 5;

            // GPU информация
            Label gpuLabel = CreateInfoLabel("🎮 Видеокарта:", currentSystemInfo.GPUName);
            gpuLabel.Location = new Point(10, y);
            gpuLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(gpuLabel);
            y += gpuLabel.Height + 5;

            // RAM информация
            Label ramLabel = CreateInfoLabel("🧠 Оперативная память:", $"{currentSystemInfo.RAMTotalGB} GB");
            ramLabel.Location = new Point(10, y);
            ramLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(ramLabel);
            y += ramLabel.Height + 5;

            // Уровень системы
            Label levelLabel = CreateInfoLabel("📊 Уровень системы:", currentSystemInfo.SystemLevel, GetLevelColor(currentSystemInfo.SystemLevel));
            levelLabel.Location = new Point(10, y);
            levelLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(levelLabel);
            y += levelLabel.Height + 5;

            Label scoreLabel = CreateInfoLabel("   Общий балл:", $"{currentSystemInfo.SystemScore}/100");
            scoreLabel.Location = new Point(10, y);
            scoreLabel.Width = systemInfoPanel.Width - 40;
            systemInfoPanel.Controls.Add(scoreLabel);
        }

        private Label CreateInfoLabel(string title, string value, Color? valueColor = null)
        {
            Label label = new Label();
            label.Text = $"{title} {value}";
            label.Font = new Font("Segoe UI", 10);
            label.ForeColor = valueColor ?? Color.White;
            label.Height = 25;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private Color GetLevelColor(string level)
        {
            switch (level)
            {
                case "ВЫСОКИЙ": return Color.LightGreen;
                case "СРЕДНИЙ": return Color.Yellow;
                case "НИЗКИЙ": return Color.Orange;
                case "МИНИМАЛЬНЫЙ": return Color.Red;
                default: return Color.White;
            }
        }

        private void InitializeGamesList()
        {
            availableGames = new List<GameInfo>
            {
                new GameInfo
                {
                    Name = "VALORANT",
                    ExecutableName = "VALORANT.exe",
                    ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "VALORANT", "Saved", "Config", "Windows", "GameUserSettings.ini"),
                    Engine = "Unreal Engine 4",
                    IsInstalled = CheckGameInstallation("VALORANT")
                },
                new GameInfo
                {
                    Name = "Fortnite",
                    ExecutableName = "FortniteClient-Win64-Shipping.exe",
                    ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "FortniteGame", "Saved", "Config", "WindowsClient", "GameUserSettings.ini"),
                    Engine = "Unreal Engine 5",
                    IsInstalled = CheckGameInstallation("Fortnite")
                },
                new GameInfo
                {
                    Name = "The Finals",
                    ExecutableName = "TheFinals.exe",
                    ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "TheFinals", "Saved", "Config", "Windows", "GameUserSettings.ini"),
                    Engine = "Unreal Engine 5",
                    IsInstalled = CheckGameInstallation("TheFinals")
                }
            };

            UpdateGamesListUI();
        }

        private bool CheckGameInstallation(string gameName)
        {
            // Проверяем существование конфиг файла
            var game = availableGames.FirstOrDefault(g => g.Name == gameName);
            if (game != null)
            {
                // Проверяем есть ли конфиг файл
                if (File.Exists(game.ConfigPath))
                    return true;

                // Или ищем исполняемый файл
                if (FindExecutable(game.ExecutableName) != null)
                    return true;
            }
            return false;
        }

        private string FindExecutable(string exeName)
        {
            // Простой поиск в Program Files
            string[] searchPaths = {
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
            };

            foreach (string path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        string[] files = Directory.GetFiles(path, exeName, SearchOption.AllDirectories);
                        if (files.Length > 0)
                            return files[0];
                    }
                    catch { }
                }
            }

            return null;
        }

        private void UpdateGamesListUI()
        {
            if (gamesListPanel == null || availableGames == null)
                return;

            gamesListPanel.Controls.Clear();

            foreach (GameInfo game in availableGames)
            {
                Button gameButton = new Button();
                gameButton.Text = $"  {game.Name}";
                gameButton.Tag = game;
                gameButton.Height = 50;
                gameButton.Width = gamesListPanel.Width - 40;
                gameButton.FlatStyle = FlatStyle.Flat;
                gameButton.TextAlign = ContentAlignment.MiddleLeft;
                gameButton.Font = new Font("Segoe UI", 10);
                gameButton.ForeColor = game.IsInstalled ? Color.White : Color.Gray;
                gameButton.BackColor = game.IsInstalled ? Color.FromArgb(60, 60, 80) : Color.FromArgb(40, 40, 50);
                gameButton.FlatAppearance.BorderSize = 0;

                // Добавляем иконку
                if (game.Engine.Contains("Unreal Engine 5"))
                    gameButton.Text = "⚡ " + gameButton.Text;
                else
                    gameButton.Text = "🎮 " + gameButton.Text;

                if (!game.IsInstalled)
                    gameButton.Text += " (не установлена)";

                gameButton.Click += (s, e) => SelectGame(game);

                gamesListPanel.Controls.Add(gameButton);
            }
        }

        private void SelectGame(GameInfo game)
        {
            selectedGame = game;
            UpdateGameDetailsUI();
        }

        private void UpdateGameDetailsUI()
        {
            if (gameDetailsPanel == null || selectedGame == null)
                return;

            gameDetailsPanel.Controls.Clear();

            int y = 10;

            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = selectedGame.Name;
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.Cyan;
            titleLabel.Location = new Point(10, y);
            titleLabel.Width = gameDetailsPanel.Width - 40;
            titleLabel.Height = 40;
            gameDetailsPanel.Controls.Add(titleLabel);
            y += titleLabel.Height + 15;

            // Информация об игре
            Label engineLabel = CreateDetailLabel("Движок:", selectedGame.Engine);
            engineLabel.Location = new Point(10, y);
            engineLabel.Width = gameDetailsPanel.Width - 40;
            gameDetailsPanel.Controls.Add(engineLabel);
            y += engineLabel.Height + 5;

            Label statusLabel = CreateDetailLabel("Статус:", selectedGame.IsInstalled ? "✅ Установлена" : "❌ Не установлена");
            statusLabel.Location = new Point(10, y);
            statusLabel.Width = gameDetailsPanel.Width - 40;
            gameDetailsPanel.Controls.Add(statusLabel);
            y += statusLabel.Height + 5;

            Label configLabel = CreateDetailLabel("Конфиг файл:", selectedGame.ConfigPath);
            configLabel.Location = new Point(10, y);
            configLabel.Width = gameDetailsPanel.Width - 40;
            gameDetailsPanel.Controls.Add(configLabel);
            y += configLabel.Height + 20;

            if (selectedGame.IsInstalled)
            {
                // Проверяем существование конфига
                bool configExists = File.Exists(selectedGame.ConfigPath);
                Label configExistsLabel = CreateDetailLabel("Конфиг найден:", configExists ? "✅ Да" : "❌ Нет");
                configExistsLabel.Location = new Point(10, y);
                configExistsLabel.Width = gameDetailsPanel.Width - 40;
                gameDetailsPanel.Controls.Add(configExistsLabel);
                y += configExistsLabel.Height + 5;

                if (configExists)
                {
                    FileInfo configInfo = new FileInfo(selectedGame.ConfigPath);
                    Label sizeLabel = CreateDetailLabel("Размер:", $"{configInfo.Length / 1024} KB");
                    sizeLabel.Location = new Point(10, y);
                    sizeLabel.Width = gameDetailsPanel.Width - 40;
                    gameDetailsPanel.Controls.Add(sizeLabel);
                    y += sizeLabel.Height + 5;

                    Label dateLabel = CreateDetailLabel("Дата изменения:", configInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm"));
                    dateLabel.Location = new Point(10, y);
                    dateLabel.Width = gameDetailsPanel.Width - 40;
                    gameDetailsPanel.Controls.Add(dateLabel);
                    y += dateLabel.Height + 20;
                }
            }

            // Рекомендации
            Label recommendationsLabel = new Label();
            recommendationsLabel.Text = "📋 РЕКОМЕНДАЦИИ:";
            recommendationsLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            recommendationsLabel.ForeColor = Color.LightGreen;
            recommendationsLabel.Location = new Point(10, y);
            recommendationsLabel.Width = gameDetailsPanel.Width - 40;
            recommendationsLabel.Height = 30;
            gameDetailsPanel.Controls.Add(recommendationsLabel);
            y += recommendationsLabel.Height + 10;

            // Генерируем рекомендации
            List<string> recommendations = GenerateRecommendations();
            foreach (string recommendation in recommendations)
            {
                Label recLabel = new Label();
                recLabel.Text = $"• {recommendation}";
                recLabel.Font = new Font("Segoe UI", 10);
                recLabel.ForeColor = Color.LightGray;
                recLabel.Location = new Point(20, y);
                recLabel.Width = gameDetailsPanel.Width - 60;
                recLabel.Height = 25;
                gameDetailsPanel.Controls.Add(recLabel);
                y += recLabel.Height + 5;
            }
        }

        private Label CreateDetailLabel(string title, string value)
        {
            Label label = new Label();
            label.Text = $"{title} {value}";
            label.Font = new Font("Segoe UI", 10);
            label.ForeColor = Color.White;
            label.Height = 25;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private List<string> GenerateRecommendations()
        {
            List<string> recommendations = new List<string>();

            if (selectedGame == null || currentSystemInfo == null)
                return recommendations;

            // Общие рекомендации
            recommendations.Add($"Уровень графики: {GetGraphicsLevel()}");
            recommendations.Add($"Разрешение: {GetRecommendedResolution()}");

            // Специфичные для движка
            if (selectedGame.Engine.Contains("Unreal Engine 5"))
            {
                recommendations.Add("Включите Temporal Super Resolution (TSR)");
                recommendations.Add("Используйте Nanite для геометрии");
                recommendations.Add("Lumen: Среднее качество");
            }

            // Специфичные для игры
            if (selectedGame.Name == "VALORANT")
            {
                recommendations.Add("Тени: Средние или Низкие");
                recommendations.Add("Текстуры: Высокие");
                recommendations.Add("Эффекты: Низкие");
                recommendations.Add("Многоопоточная отрисовка: Включена");
            }
            else if (selectedGame.Name == "Fortnite")
            {
                recommendations.Add("Глобальное освещение: Среднее");
                recommendations.Add("Отражения: Средние");
                recommendations.Add("Пост-обработка: Средняя");
            }

            return recommendations;
        }

        private string GetGraphicsLevel()
        {
            if (currentSystemInfo == null) return "Средние";

            switch (currentSystemInfo.SystemLevel)
            {
                case "ВЫСОКИЙ": return "Ультра";
                case "СРЕДНИЙ": return "Высокие";
                case "НИЗКИЙ": return "Средние";
                case "МИНИМАЛЬНЫЙ": return "Низкие";
                default: return "Средние";
            }
        }

        private string GetRecommendedResolution()
        {
            if (currentSystemInfo == null) return "1920x1080";

            switch (currentSystemInfo.SystemLevel)
            {
                case "ВЫСОКИЙ": return "2560x1440 (2K)";
                case "СРЕДНИЙ": return "1920x1080 (Full HD)";
                case "НИЗКИЙ": return "1600x900 или 1920x1080";
                case "МИНИМАЛЬНЫЙ": return "1280x720 (HD)";
                default: return "1920x1080";
            }
        }

        private void OptimizeButton_Click(object sender, EventArgs e)
        {
            if (selectedGame == null)
            {
                MessageBox.Show("Выберите игру для оптимизации!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!selectedGame.IsInstalled)
            {
                MessageBox.Show("Игра не установлена!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Показываем прогресс
                optimizeButton.Enabled = false;
                progressBar.Visible = true;
                statusLabel.Text = "Оптимизация...";

                // Проверяем, запущена ли игра
                if (IsGameRunning(selectedGame.ExecutableName))
                {
                    DialogResult result = MessageBox.Show("Игра запущена! Закрыть игру перед оптимизацией?",
                        "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        KillGameProcess(selectedGame.ExecutableName);
                        System.Threading.Thread.Sleep(2000); // Ждем закрытия
                    }
                    else
                    {
                        optimizeButton.Enabled = true;
                        progressBar.Visible = false;
                        return;
                    }
                }

                // Создаем резервную копию
                if (File.Exists(selectedGame.ConfigPath))
                {
                    string backupPath = selectedGame.ConfigPath + ".backup";
                    File.Copy(selectedGame.ConfigPath, backupPath, true);
                    statusLabel.Text = "Создана резервная копия...";
                }

                // Оптимизируем настройки
                OptimizeGameSettings();

                statusLabel.Text = "Оптимизация завершена!";
                MessageBox.Show("Настройки успешно оптимизированы!\n\n" +
                    "Запустите игру чтобы увидеть изменения.",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оптимизации: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Ошибка оптимизации";
            }
            finally
            {
                optimizeButton.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private bool IsGameRunning(string executableName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executableName));
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private void KillGameProcess(string executableName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executableName));

                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch { }
        }

        private void OptimizeGameSettings()
        {
            if (!File.Exists(selectedGame.ConfigPath))
            {
                // Создаем базовый конфиг если его нет
                CreateDefaultConfig();
                return;
            }

            // Читаем текущий конфиг
            List<string> lines = new List<string>(File.ReadAllLines(selectedGame.ConfigPath));
            List<string> optimizedLines = new List<string>();

            bool inSystemSettings = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Пропускаем комментарии
                if (trimmedLine.StartsWith(";") || string.IsNullOrWhiteSpace(trimmedLine))
                {
                    optimizedLines.Add(line);
                    continue;
                }

                // Отслеживаем секцию
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    inSystemSettings = trimmedLine.Equals("[SystemSettings]", StringComparison.OrdinalIgnoreCase);
                    optimizedLines.Add(line);
                    continue;
                }

                // Оптимизируем настройки в секции SystemSettings
                if (inSystemSettings && line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        // Применяем оптимизации
                        string optimizedValue = GetOptimizedValue(key, selectedGame.Name);
                        if (optimizedValue != null)
                        {
                            optimizedLines.Add($"{key}={optimizedValue}");
                            continue;
                        }
                    }
                }

                optimizedLines.Add(line);
            }

            // Записываем обратно
            File.WriteAllLines(selectedGame.ConfigPath, optimizedLines.ToArray());
        }

        private string GetOptimizedValue(string key, string gameName)
        {
            // Базовые оптимизации для всех игр
            Dictionary<string, string> baseOptimizations = new Dictionary<string, string>
            {
                ["VSync"] = "False",
                ["FrameRateLimit"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "0" : "144",
                ["MultithreadedRendering"] = "True",
            };

            // Оптимизации для Valorant
            Dictionary<string, string> valorantOptimizations = new Dictionary<string, string>
            {
                ["ResolutionSizeX"] = "1920",
                ["ResolutionSizeY"] = "1080",
                ["LastUserConfirmedResolutionSizeX"] = "1920",
                ["LastUserConfirmedResolutionSizeY"] = "1080",
                ["QualityLevel"] = GetQualityLevelValue(),
                ["MaterialQualityLevel"] = GetQualityLevelValue(),
                ["TextureQualityLevel"] = "2", // Высокие текстуры
                ["ShadowQualityLevel"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["EffectsQualityLevel"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["AntiAliasing"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["RenderScale"] = "100",
                ["DisplayGamma"] = "2.2",
                ["ShowGrass"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "True" : "False",
                ["FoliageQuality"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
            };

            // Оптимизации для Fortnite
            Dictionary<string, string> fortniteOptimizations = new Dictionary<string, string>
            {
                ["ResolutionSizeX"] = "1920",
                ["ResolutionSizeY"] = "1080",
                ["LastUserConfirmedResolutionSizeX"] = "1920",
                ["LastUserConfirmedResolutionSizeY"] = "1080",
                ["QualityPreset"] = GetQualityLevelValue(),
                ["Effects"] = GetQualityLevelValue(),
                ["PostProcessing"] = GetQualityLevelValue(),
                ["Shadows"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["Textures"] = "2",
                ["AntiAliasing"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["GlobalIllumination"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
                ["Reflections"] = currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1",
            };

            // Объединяем оптимизации
            Dictionary<string, string> allOptimizations = new Dictionary<string, string>(baseOptimizations);

            if (gameName == "VALORANT")
            {
                foreach (KeyValuePair<string, string> opt in valorantOptimizations)
                    allOptimizations[opt.Key] = opt.Value;
            }
            else if (gameName == "Fortnite")
            {
                foreach (KeyValuePair<string, string> opt in fortniteOptimizations)
                    allOptimizations[opt.Key] = opt.Value;
            }

            // Возвращаем оптимизированное значение если ключ найден
            return allOptimizations.ContainsKey(key) ? allOptimizations[key] : null;
        }

        private string GetQualityLevelValue()
        {
            if (currentSystemInfo == null) return "1";

            switch (currentSystemInfo.SystemLevel)
            {
                case "ВЫСОКИЙ": return "3"; // Ultra
                case "СРЕДНИЙ": return "2"; // High
                case "НИЗКИЙ": return "1";  // Medium
                case "МИНИМАЛЬНЫЙ": return "0"; // Low
                default: return "1";
            }
        }

        private void CreateDefaultConfig()
        {
            // Создаем базовый конфиг файл
            List<string> defaultConfig = new List<string>
            {
                "; Конфигурационный файл, созданный Game Optimizer Pro",
                "; Дата создания: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                "",
                "[SystemSettings]",
                "ResolutionSizeX=1920",
                "ResolutionSizeY=1080",
                "LastUserConfirmedResolutionSizeX=1920",
                "LastUserConfirmedResolutionSizeY=1080",
                "FrameRateLimit=144",
                "VSync=False",
                "MultithreadedRendering=True",
                "",
                "; Графические настройки",
                "QualityLevel=" + GetQualityLevelValue(),
                "MaterialQualityLevel=" + GetQualityLevelValue(),
                "TextureQualityLevel=2",
                "ShadowQualityLevel=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "EffectsQualityLevel=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "AntiAliasing=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "RenderScale=100",
                "DisplayGamma=2.2",
                "",
                "; Дополнительные настройки",
                "ShowGrass=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "True" : "False"),
                "FoliageQuality=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "",
                "[ScalabilityGroups]",
                "sg.ResolutionQuality=100",
                "sg.ViewDistanceQuality=" + GetQualityLevelValue(),
                "sg.AntiAliasingQuality=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "sg.ShadowQuality=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "sg.PostProcessQuality=" + GetQualityLevelValue(),
                "sg.TextureQuality=2",
                "sg.EffectsQuality=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "sg.FoliageQuality=" + (currentSystemInfo.SystemLevel == "ВЫСОКИЙ" ? "2" : "1"),
                "",
                "; Конец файла"
            };

            // Создаем папку если её нет
            Directory.CreateDirectory(Path.GetDirectoryName(selectedGame.ConfigPath));

            // Записываем конфиг
            File.WriteAllLines(selectedGame.ConfigPath, defaultConfig.ToArray());
        }
    }
}