using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DealOrNoDeal.Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly Random Randomiser = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private volatile int _numberOfBlueAmounts, _numberOfGamesPlayed, _numberOfWinsImproved;

        private void RunSimulationClick(object sender, RoutedEventArgs e)
        {
            RunSimulation.IsEnabled = false;
            UpdateGameStatistics();

            var simThread = new Thread(Simulate){ IsBackground = true};
            simThread.Start();
        }

        private void Simulate()
        {


            for (int itr = 0; itr < 100000; itr++)
            {
                int?[] boxes = new int?[16];

                for (int i = 0; i < 16; i++)
                {
                    boxes[i] = i;
                }

                BoxChoice myChoiceColour;

                switch (_numberOfBlueAmounts)
                {
                    case 6:
                        //we must have picked a blue box
                        myChoiceColour = BoxChoice.Blue;
                        break;
                    case 7:
                        //we may have picked either colour
                        myChoiceColour = BoxChoice.Any;
                        break;
                    case 8:
                        //we must have picked a red box
                        myChoiceColour = BoxChoice.Red;
                        break;
                    default:
                        throw new Exception("Unknown number of blue choices");
                }

                int myChoice = GetRandomBox(myChoiceColour);

                boxes[myChoice] = null;

                int remainingBlueChoices = _numberOfBlueAmounts;

                //Now make the remaining blue choices
                while (remainingBlueChoices > 0)
                {
                    int choice = GetRandomBox(BoxChoice.Blue);

                    if (!boxes[choice].HasValue)
                        continue;

                    boxes[choice] = null;

                    remainingBlueChoices--;
                }

                int remainingRedChoices = 14 - _numberOfBlueAmounts;

                //Now make the remaining red choices
                while (remainingRedChoices > 0)
                {
                    int choice = GetRandomBox(BoxChoice.Red);

                    if (!boxes[choice].HasValue)
                        continue;

                    boxes[choice] = null;

                    remainingRedChoices--;
                }

                int otherBox = boxes.First(b => b.HasValue).Value;

                _numberOfGamesPlayed++;

                if (myChoice < otherBox)
                    _numberOfWinsImproved++;

                Dispatcher.Invoke(new Action(() =>
                                {
                                    PercentImprovedWinsBar.Maximum = _numberOfGamesPlayed;
                                    PercentImprovedWinsBar.Value = _numberOfWinsImproved;
                                }));

                Dispatcher.Invoke(new Action(() =>
                                {
                                    PercentImprovedWinsLabel.Content = (100.0 * _numberOfWinsImproved / _numberOfGamesPlayed).ToString("F2") + "%";
                                }));
            }

            MessageBox.Show("done");
        }

        private void UpdateGameStatistics(object sender, RoutedEventArgs e)
        {
            UpdateGameStatistics();
        }

        private void UpdateGameStatistics()
        {
            if (SixBluePicked.IsChecked.HasValue && SixBluePicked.IsChecked.Value)
                _numberOfBlueAmounts = 6;
            else if (SevenBluePicked.IsChecked.HasValue && SevenBluePicked.IsChecked.Value)
                _numberOfBlueAmounts = 7;
            else if (EightBluePicked.IsChecked.HasValue && EightBluePicked.IsChecked.Value)
                _numberOfBlueAmounts = 8;

            _numberOfGamesPlayed = 0;
            _numberOfWinsImproved = 0;
        }

        enum BoxChoice
        {
            Any,
            Red,
            Blue
        }

        private int GetRandomBox(BoxChoice choice)
        {
            int choiceStart, choiceEnd;

            switch (choice)
            {
                case BoxChoice.Blue:
                    //we must have picked a blue box
                    choiceStart = 0;
                    choiceEnd = 8;
                    break;
                case BoxChoice.Any:
                    //we may have picked either colour
                    choiceStart = 0;
                    choiceEnd = 16;
                    break;
                case BoxChoice.Red:
                    //we must have picked a red box
                    choiceStart = 8;
                    choiceEnd = 16;
                    break;
                default:
                    throw new Exception("Unknown number of blue choices");
            }

            return Randomiser.Next(choiceStart, choiceEnd);
        }
    }
}
