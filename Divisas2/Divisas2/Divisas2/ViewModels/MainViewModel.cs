using Divisas2.Modals;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Divisas2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private ExchangeRates exchangeRates;//contendra el JSON serealizado

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private bool isRunning;

        private bool isEnabled;

        private string message = "Conversor de Monedas";


        #endregion

        #region Properties
        public ObservableCollection<Rate> Rates { get; set; }

        public decimal Amount
        {
            set
            {
                if (amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }
            get
            {
                return amount;
            }
        }

        public double SourceRate
        {
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
                }
            }
            get
            {
                return sourceRate;
            }
        }

        public double TargetRate
        {
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
                }
            }
            get
            {
                return targetRate;
            }
        }

        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }

        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
                }
            }
            get
            {
                return message;
            }
        }
        #endregion


        #region Constructor
        public MainViewModel()
        {
            //Es necesario Iniciar los observables acá
            Rates = new ObservableCollection<Rate>();

            GetRates();
        }
        #endregion

        #region Methods
        private void LoadRates()
        {
            Rates.Clear();
            //Se aplica reflexion
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();//se crea el objeto

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);//obtiene el Codigo de la moneda ex: COP EUR PHP
                Rates.Add(new Rate
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),//Se obtiene el valor de la moneda
                });
            }
        }


        private async void GetRates()
        {
            //IsRunning = true;
            //IsEnabled = false;
            //Check if on WIFI or Data Connection active
            /*
            if (!CrossConnectivity.Current.IsConnected) {
                IsRunning = false;
                IsEnabled = false;

                await App.Current.MainPage.DisplayAlert("Error","Verifica tu conección a internet","Aceptar");//change Display for ForeingExchangePage
                return;
            }

            //Check get access to internet
            var isRechable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
            if (isRechable)
            {
                IsRunning = false;
                IsEnabled = false;
                await App.Current.MainPage.DisplayAlert("Error", "No hay acceso a internet", "Aceptar");//change Display for ForeingExchangePage
                return;
            }
            */
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();//result tiene el string retornado por el servicio
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);//Deserealiza el string retornado por la respuesta
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
                IsRunning = false;
                IsEnabled = false;
                return;
            }

            LoadRates();
            IsRunning = false;
            IsEnabled = true;

        }
        #endregion

        #region Commands
        public ICommand InvertCommand { get { return new RelayCommand(Change); } }

        private void Change() {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
        }

        public ICommand ConvertCommand { get { return new RelayCommand(ConvertMoney); } }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes ingresar un valor a convertir", "Aceptar");
                return;
            }

            if (SourceRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes seleccionar la moneda origen", "Aceptar");
                return;
            }

            if (TargetRate == -1)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes seleccionar la moneda destino", "Aceptar");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} = {1:N2}", amount, amountConverted);
        }
        #endregion

    }
}
