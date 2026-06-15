using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using KURAOrderSystem.ViewModels;

namespace KURAOrderSystem.Views
{
    public partial class BikkuraPonWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly int _milestone;
        private bool _isWin;
        private string _prizeName = string.Empty;

        private readonly string[] _prizes = new[]
        {
            "特製 熟成まぐろキーホルダー 🐟",
            "ミニチュア 鮮度くんマグネット 🍣",
            "くら寿司オリジナル ぷくぷくシール 🎨",
            "寿司ペン立てフィギュア ✏️",
            "超激レア！金賞 抗菌ミニマスクケース 🏆"
        };

        public BikkuraPonWindow(MainViewModel viewModel, int milestone)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _milestone = milestone;
            this.Owner = Application.Current.MainWindow;
            
            Loaded += BikkuraPonWindow_Loaded;
        }

        private async void BikkuraPonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Determine Win/Lose (30% win rate)
            Random rand = new Random();
            _isWin = rand.Next(1, 100) <= 30; // 30% chance of Atari

            if (_isWin)
            {
                _prizeName = _prizes[rand.Next(_prizes.Length)];
                PrizeNameText.Text = _prizeName;
            }

            // Save to Database (or local store)
            _viewModel.SaveBikkuraPonResultToDb(_milestone, _isWin, _isWin ? _prizeName : "なし");

            // 2. Play Stage 1: Shake Machine and Rotate Lever
            try { System.Media.SystemSounds.Beep.Play(); } catch { }
            Storyboard? shakeStoryboard = Resources["ShakeMachine"] as Storyboard;
            shakeStoryboard?.Begin(this);

            IntroText.Text = "カプセルをまわしているよ...";
            await Task.Delay(2000);

            // 3. Play Stage 2: Drop Capsule
            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
            IntroText.Text = "ポン！カプセルが出た！";
            DroppedCapsule.Visibility = Visibility.Visible;
            Storyboard? dropStoryboard = Resources["DropCapsule"] as Storyboard;
            dropStoryboard?.Begin(this);

            await Task.Delay(1500);

            // 4. Reveal Results
            IntroText.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;

            if (_isWin)
            {
                try { System.Media.SystemSounds.Hand.Play(); } catch { }
                WinPanel.Visibility = Visibility.Visible;
                SunburstGrid.Visibility = Visibility.Visible;
                Storyboard? rotateStoryboard = Resources["RotateSunburst"] as Storyboard;
                rotateStoryboard?.Begin(this);
            }
            else
            {
                try { System.Media.SystemSounds.Question.Play(); } catch { }
                LosePanel.Visibility = Visibility.Visible;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
