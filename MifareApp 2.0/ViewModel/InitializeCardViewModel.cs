using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MifareApp_2._0.Model;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MifareApp_2._0.ViewModel
{
    [ImplementPropertyChanged]
    public class InitializeCardViewModel : ViewModelBase
    {
        #region Connection
        public bool IsStarted { get; set; }

        public bool IsConnected { get; set; }

        public List<CardReader> Readers { get; set; }

        public CardReader SelectedReader { get; set; }

        public String KeyA { get; set; }

        public RelayCommand StartCommand { get; private set; }

        #endregion

        #region ID Card Number

        public String IDNumberContent { get; set; }

        #endregion

        public string status = "";
        System.Timers.Timer firstTimer;

        public InitializeCardViewModel()
        {
            Readers = new CardReader().getListReaders();
            IsStarted = false;
            IsConnected = false;
            StartCommand = new RelayCommand(StartMethod);
            firstTimer = new System.Timers.Timer();
        }

        private void StartMethod()
        {
            SelectedReader.sCardEstablishContext(out status);
            
            firstTimer.Interval = Constants.TIMER_INTERVAL_TIME;
            firstTimer.Elapsed += new ElapsedEventHandler(OnTimerTick);
            firstTimer.Start();

            IsStarted = true;
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            if (SelectedReader.GetStatusChange(out status) == 0)
            {
                firstTimer.Stop();

                if (IsConnected == false)
                {
                    SelectedReader.Connect(out status);
                    SelectedReader.LoadKey(0, KeyA, out status);
                    SelectedReader.Authentication(Constants.STUDENT_ID_BLOCK_NUMBER, 0, Constants.KEY_A_TYPE, out status);

                    if (status.Contains("Successful"))
                    {
                        IsConnected = true;
                    }
                    else
                    {
                        IsConnected = false;
                        IDNumberContent = Constants.AUTH_FAILED;
                        firstTimer.Start();
                        return;
                    }
                }
                
                SelectedReader.Connect(out status);
                SelectedReader.LoadKey(0, KeyA, out status);
                SelectedReader.Authentication(Constants.STUDENT_ID_BLOCK_NUMBER, 0, Constants.KEY_A_TYPE, out status);

                string blockValue = SelectedReader.Read(Constants.STUDENT_ID_BLOCK_NUMBER, out status);

                if (!blockValue.Equals(""))
                {
                    IDNumberContent = Conversions.ExtractIDNumber(blockValue);
                    SaveFile.SaveToFile(IDNumberContent);
                    HID.Beep(3000, 200);
                }
                else
                {
                    IDNumberContent = Constants.PASS_CORRECT_CARD;
                }

                Thread.Sleep(1500);
                IDNumberContent = Constants.PASS_CORRECT_CARD;
                firstTimer.Start();   
            }
        }
    }
}
