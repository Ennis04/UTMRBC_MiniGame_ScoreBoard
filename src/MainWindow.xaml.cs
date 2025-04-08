using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using MiniGame_ScoreBoard;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Collections.Generic;

namespace MiniGame_ScoreBoard
{
    public partial class MainWindow : Window
    {
        private int score = 0;
        private int totalSeconds = 0;
        private DispatcherTimer gameTimer;
        private DispatcherTimer countdownTimer;
        private DispatcherTimer preparationEndsTimer;
        private DispatcherTimer gameOverTimer;
        private int countdownValue;
        private Stopwatch stopwatch = new Stopwatch();
        public bool IsTimerRunning => gameTimer.IsEnabled;
        private bool isPreparation = false;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Stack<int> scoreHistory = new Stack<int>();
        private DispatcherTimer mirrorTimer;
        private ControlWindow controlWindow;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimers();
            UpdateScore();
        }

        private void InitializeMirroring()
        {
            mirrorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) }; // Update every 100ms
            mirrorTimer.Tick += (s, e) =>
            {
                if (controlWindow != null)
                {
                    controlWindow.UpdateMirror();
                }
            };
            mirrorTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            controlWindow = new ControlWindow(this); // Create and show Control Window
            controlWindow.Show();
            InitializeMirroring(); // Start updating the mirrored display
        }

        private void InitializeTimers()
        {
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            gameTimer.Tick += GameTimer_Tick;

            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;

            preparationEndsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            preparationEndsTimer.Tick += PreparationEndsTimer_Tick;

            gameOverTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            gameOverTimer.Tick += GameOverTimer_Tick;
        }

        private void PlaySound(string filePath)
        {
            mediaPlayer.Open(new Uri(filePath,UriKind.Relative));
            mediaPlayer.Play();
        }

        private void UpdateScore()
        {
            string scoreText = score.ToString("00");
            LeftDigit.Text = scoreText[0].ToString();
            RightDigit.Text = scoreText[1].ToString();
        }

        public void IncreaseScore(int points)
        {
            if (score + points <= 99)
            {
                scoreHistory.Push(score);
                score += points;
                PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Scored sound.mp3");
                UpdateScore();

                // Get remaining time
                string remainingTime = GameTimer.Text; // e.g., "02:39"

                controlWindow.AddScore(TeamName.Text, points, remainingTime);
            }
        }


        public void UndoScore()
        {
            if (scoreHistory.Count > 0)
            {
                score = scoreHistory.Pop();  // Restore the last score
                UpdateScore();

                controlWindow.RemoveLastScore();
            }
        }

        public int GetTotalPoints()
        {
            return score; // Assuming `currentScore` tracks the active team's score
        }

        public void SetTeamName(string teamName)
        {
            TeamName.Text = teamName; // Make sure 'teamLabel' exists in your UI
        }



        public async void StartPreparation()
        {
            isPreparation = true;
            totalSeconds = 60;
            GameTimer.Text = "01:00";
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Preparation Starts In.mp3");

            controlWindow.AddEventLog(TeamName.Text, "Preparation Starts");

            await Task.Delay(800);
            StartCountdown(5, "Preparation Starts In");
        }

        public async void StartGame()
        {
            if (!isPreparation)
            {
                totalSeconds = 180;
                GameTimer.Text = "03:00";
                PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Game Starts In.mp3");

                controlWindow.AddEventLog(TeamName.Text, "Game Starts");

                await Task.Delay(565);
                StartCountdown(5, "Game Starts In");
            }
        }

        public void PauseTimer()
        {
            gameTimer.Stop();
        }

        public void ResumeTimer()
        {
            gameTimer.Start();
        }

        private void StartCountdown(int seconds, string initialText)
        {
            countdownValue = seconds;
            CountdownPopup.FontSize = 150; // Set font size for the phrase
            CountdownPopup.Text = initialText;
            CountdownPopup.Visibility = Visibility.Visible;
            ScoreboardPanel.Visibility = Visibility.Collapsed;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (countdownValue > 0)
            {
                CountdownPopup.FontSize = 200; // Larger font size for countdown numbers
                CountdownPopup.Text = countdownValue.ToString();
                countdownValue--;
            }
            else
            {
                CountdownPopup.FontSize = 200; // Keep "Starts" visible in big font
                CountdownPopup.Text = "Starts";
                countdownTimer.Stop();

                // Short delay after "Starts" before starting the game timer
                DispatcherTimer hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                hideTimer.Tick += (s, args) =>
                {
                    hideTimer.Stop();
                    CountdownPopup.Visibility = Visibility.Collapsed;
                    ScoreboardPanel.Visibility = Visibility.Visible;

                    // Start the actual game timer here
                    stopwatch.Restart();
                    gameTimer.Start();
                };
                hideTimer.Start();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (totalSeconds > 0)
            {
                totalSeconds--;
                TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
                GameTimer.Text = time.ToString(@"mm\:ss");
            }
            else
            {
                gameTimer.Stop();
                stopwatch.Stop();

                if (isPreparation)
                    ShowPreparationEnds();
                else
                    ShowGameOver();
            }
        }

        private void ShowPreparationEnds()
        {
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Preparation Ends.mp3");

            controlWindow.AddEventLog(TeamName.Text, "Preparation Ends");

            PreparationEndsPopup.Visibility = Visibility.Visible;
            ScoreboardPanel.Visibility = Visibility.Collapsed;
            preparationEndsTimer.Start();
        }

        private void PreparationEndsTimer_Tick(object sender, EventArgs e)
        {
            preparationEndsTimer.Stop();
            PreparationEndsPopup.Visibility = Visibility.Collapsed;
            ScoreboardPanel.Visibility = Visibility.Visible;
            isPreparation = false; // Allow the game to be started manually
        }

        private void ShowGameOver()
        {
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Game Ends.mp3");

            controlWindow.AddEventLog(TeamName.Text, "Game Ends");

            GameOverPopup.Visibility = Visibility.Visible;
            ScoreboardPanel.Visibility = Visibility.Collapsed;
            gameOverTimer.Start();
        }

        private void GameOverTimer_Tick(object sender, EventArgs e)
        {
            gameOverTimer.Stop();
            GameOverPopup.Visibility = Visibility.Collapsed;
            ScoreboardPanel.Visibility = Visibility.Visible;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void RestartGame()
        {
            // Reset Score
            score = 0;
            UpdateScore();

            // Reset Timer
            totalSeconds = 0;
            GameTimer.Text = "00:00";

            // Stop All Timers
            gameTimer.Stop();
            countdownTimer.Stop();
            preparationEndsTimer.Stop();
            gameOverTimer.Stop();
            stopwatch.Reset();

            // Hide Popups & Show Scoreboard
            CountdownPopup.Visibility = Visibility.Collapsed;
            PreparationEndsPopup.Visibility = Visibility.Collapsed;
            GameOverPopup.Visibility = Visibility.Collapsed;
            ScoreboardPanel.Visibility = Visibility.Visible;

            // Reset other game states
            isPreparation = false;
        }

        public void ForceEndGame()
        {
            // Stop all timers immediately
            controlWindow.AddEventLog(TeamName.Text, "Game Ends");
            gameTimer.Stop();
            countdownTimer.Stop();
            preparationEndsTimer.Stop();
            gameOverTimer.Stop();
            stopwatch.Reset();

            // Hide all countdowns and preparation popups
            CountdownPopup.Visibility = Visibility.Collapsed;
            PreparationEndsPopup.Visibility = Visibility.Collapsed;

            // Show "Game Ends" message
            GameOverPopup.Visibility = Visibility.Visible;
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Game Ends.mp3");
            GameOverPopup.Text = "Game Ends";

            // Hide the scoreboard
            ScoreboardPanel.Visibility = Visibility.Collapsed;
        }

        public void DisqualifyFromGame()
        {
            // Stop all timers immediately
            controlWindow.AddEventLog(TeamName.Text, "Disqualified");
            gameTimer.Stop();
            countdownTimer.Stop();
            preparationEndsTimer.Stop();
            gameOverTimer.Stop();
            stopwatch.Reset();

            // Hide all countdowns and preparation popups
            CountdownPopup.Visibility = Visibility.Collapsed;
            PreparationEndsPopup.Visibility = Visibility.Collapsed;

            // Show "Game Ends" message
            GameOverPopup.Visibility = Visibility.Visible;
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Disqualified.mp3");
            GameOverPopup.Text = "Disqualified";

            // Hide the scoreboard
            ScoreboardPanel.Visibility = Visibility.Collapsed;
        }


    }
}
