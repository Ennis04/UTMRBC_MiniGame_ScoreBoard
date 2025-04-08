using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniGame_ScoreBoard
{
    public partial class ControlWindow : Window
    {
        private MainWindow mainWindow;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string lastTeam = "";

        public ControlWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void PlaySound(string filePath)
        {
            mediaPlayer.Open(new Uri(filePath, UriKind.Relative));
            mediaPlayer.Play();
        }

        private void SetTeamA(object sender, RoutedEventArgs e)
        {
            if (lastTeam == "TEAM A") return; // Prevent redundant calls
            ShowTotalBeforeSwitch();
            lastTeam = "TEAM A";  // Update the last selected team immediately
            mainWindow.SetTeamName("TEAM A");
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Team A.mp3");
            AddTeamHeader("TEAM A");
            HistoryScrollViewer.ScrollToEnd();
            mainWindow.RestartGame();
        }

        private void SetTeamB(object sender, RoutedEventArgs e)
        {
            if (lastTeam == "TEAM B") return; // Prevent redundant calls
            ShowTotalBeforeSwitch();
            lastTeam = "TEAM B";  // Update the last selected team immediately
            mainWindow.SetTeamName("TEAM B");
            PlaySound("C:/Users/user/Visual Studio 2022/MiniGame_ScoreBoard/Team B.mp3");
            AddTeamHeader("TEAM B");
            HistoryScrollViewer.ScrollToEnd();
            mainWindow.RestartGame();
        }


        private void ShowTotalBeforeSwitch()
        {
            if (!string.IsNullOrEmpty(lastTeam)) // Ensure it's not empty on first selection
            {
                int totalPoints = mainWindow.GetTotalPoints();
                TextBlock totalEntry = new TextBlock
                {
                    Text = $"Total: {totalPoints} points",
                    FontSize = 16,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                };
                HistoryPanel.Children.Add(totalEntry);
            }
        }

        private void AddOnePoint(object sender, RoutedEventArgs e)
        {
            mainWindow.IncreaseScore(1);
        }

        private void AddTwoPoints(object sender, RoutedEventArgs e)
        {
            mainWindow.IncreaseScore(2);
        }

        private void AddThreePoints(object sender, RoutedEventArgs e)
        {
            mainWindow.IncreaseScore(3);
        }

        private void StartPreparation(object sender, RoutedEventArgs e)
        {
            mainWindow.StartPreparation();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            mainWindow.StartGame();
        }

        private void PauseGame(object sender, RoutedEventArgs e)
        {
            if (mainWindow.IsTimerRunning)
            {
                mainWindow.PauseTimer();
                PauseResumeButton.Content = "Resume";
            }
            else
            {
                mainWindow.ResumeTimer();
                PauseResumeButton.Content = "Pause";
            }
        }

        private void RestartGame(object sender, RoutedEventArgs e)
        {
            mainWindow.RestartGame();
        }

        private void EndGame(object sender, RoutedEventArgs e)
        {
            mainWindow.ForceEndGame();
        }

        private void Disqualified(object sender, RoutedEventArgs e)
        {
            mainWindow.DisqualifyFromGame();
        }

        private void UndoScore(object sender, RoutedEventArgs e)
        {
            mainWindow.UndoScore();
        }

        public async void UpdateMirror()
        {
            if (mainWindow == null) return;

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DrawingVisual visual = new DrawingVisual();
                    using (DrawingContext dc = visual.RenderOpen())
                    {
                        dc.DrawRectangle(new VisualBrush(mainWindow), null, new Rect(0, 0, mainWindow.ActualWidth, mainWindow.ActualHeight));
                    }

                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)mainWindow.ActualWidth,
                        (int)mainWindow.ActualHeight,
                        96, 96, PixelFormats.Pbgra32);

                    renderBitmap.Render(visual);

                    PreviewImage.Source = renderBitmap;
                });
            });
        }


        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void AddScore(string team, int points, string gameTime)
        {
            lastTeam = team; // Update last team

            // Create score log with remaining game time
            TextBlock scoreEntry = new TextBlock
            {
                Text = $"{DateTime.Now:HH:mm:ss} - Scored {points} points ({gameTime})",
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(5)
            };

            HistoryPanel.Children.Add(scoreEntry);

            // Auto-scroll
            HistoryScrollViewer.ScrollToEnd();
        }

        public void RemoveLastScore()
        {
            if (HistoryPanel.Children.Count > 0)
            {
                HistoryPanel.Children.RemoveAt(HistoryPanel.Children.Count - 1);
            }
        }

        private void AddSeparator()
        {
            Border separator = new Border
            {
                BorderThickness = new Thickness(0, 2, 0, 2),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(5, 10, 5, 10),
                Height = 2
            };
            HistoryPanel.Children.Add(separator);
        }

        public void AddEventLog(string team, string eventName)
        {
            lastTeam = team; // Update last team

            // Create event log
            TextBlock eventEntry = new TextBlock
            {
                Text = $"{DateTime.Now:HH:mm:ss} - {eventName}",
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(5)
            };

            HistoryPanel.Children.Add(eventEntry);

            // Auto-scroll
            HistoryScrollViewer.ScrollToEnd();
        }

        private void AddTeamHeader(string team)
        {
            if (HistoryPanel.Children.Count > 0) // Add separator only if history is not empty
            {
                AddSeparator();
            }

            TextBlock teamHeader = new TextBlock
            {
                Text = $"{team}",  // Display the team name
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(5),
            };

            HistoryPanel.Children.Add(teamHeader);
        }
    }
}
