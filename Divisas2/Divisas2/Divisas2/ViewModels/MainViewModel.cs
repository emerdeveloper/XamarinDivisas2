using Divisas2.Modals;
using Divisas2.Services;
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

        private NameExchangeRates nameExchangeRates;

        private DataService dataService;

        private DialogService dialogService;

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private bool isRunning;

        private bool isEnabled;

        private string message = "Conversor de Monedas";

        private int targetSelectedIndex;

        private int sourceSelectedIndex;

        private bool flag = false;

        private bool flag2 = false;

        private bool flag3 = true;
        #endregion

        #region Properties
        public ObservableCollection<Rate> Rates { get; set; }

        public CheckConnection Check { get; set; }

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

        public int SourceSelectedIndex
        {
            set
            {
                if (sourceSelectedIndex != value)
                {
                    sourceSelectedIndex = value;

                    // trigger some action to take such as updating other labels or fields
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceSelectedIndex"));
                    // SelectedCountry = Countries[countriesSelectedIndex];
                }
            }
            get { return sourceSelectedIndex; }
        }

        public int TargetSelectedIndex
        {
            set
            {
                if (targetSelectedIndex != value)
                {
                    targetSelectedIndex = value;

                    // trigger some action to take such as updating other labels or fields
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetSelectedIndex"));
                    // SelectedCountry = Countries[countriesSelectedIndex];
                }
            }
            get { return targetSelectedIndex; }
        }

        public bool Flag
        {
            set
            {
                if (flag != value)
                {
                    flag = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Flag"));
                }
            }
            get
            {
                return flag;
            }
        }

        public bool Flag2
        {
            set
            {
                if (flag2 != value)
                {
                    flag2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Flag2"));
                }
            }
            get
            {
                return flag2;
            }
        }

        public bool Flag3
        {
            set
            {
                if (flag3 != value)
                {
                    flag3 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Flag3"));
                }
            }
            get
            {
                return flag3;
            }
        }
        #endregion


        #region Constructor
        public MainViewModel()
        {
            //Es necesario Iniciar los observables acá
            Rates = new ObservableCollection<Rate>();
            Check = new CheckConnection();
            dataService = new DataService();
            dialogService = new DialogService();

            GetRates();

            CrossConnectivity.Current.ConnectivityChanged += async (s, ev) =>
            {
                if (!ev.IsConnected)
                {
                    Flag = true;
                }
                else
                {
                    Flag = false;
                    var answer = await dialogService.ShowConfirm("Confirmar", "Desea actualizar los Rates");
                    if (!answer)
                    {
                        return;
                    }
                    IsRunning = true;
                    IsEnabled = false;
                    GetRatesFromCloud();
                }
            };
        }
        #endregion

        #region Methods
        private void LoadRates()
        {
            Rates.Clear();
            //Se aplica refletion
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();//se crea el objeto
            dataService.DeleteAll<Rate>(new Rate { });
            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);//obtiene el Codigo de la moneda ex: COP EUR PHP
                var taxRate = (double)property.GetValue(exchangeRates.Rates);
                var propertyNameExchangeRates = nameExchangeRates.GetType().
                                            GetRuntimeProperties().
                                            FirstOrDefault(p =>
                                            string.Equals(p.Name,
                                            code,
                                            StringComparison.OrdinalIgnoreCase));
                if (propertyNameExchangeRates != null)
                {
                    var nameTaxRate = "" + propertyNameExchangeRates.GetValue(nameExchangeRates);
                    Rates.Add(new Rate
                    {
                        Code = code,
                        TaxRate = taxRate,//Se obtiene el valor de la moneda
                        NameTaxRate = code + " - " + nameTaxRate,
                    });
                    //Save Data in Local
                    var rate = new Rate();
                    rate.Code = code;
                    rate.TaxRate = taxRate;
                    rate.NameTaxRate = code + " - " + nameTaxRate;
                    dataService.Insert<Rate>(rate);
                }
            }
        }

        private async void GetRates()
        {
            IsRunning = true;
            IsEnabled = false;

            var checkConnection = await Check.Check();//Check Internet Connection
            if (!checkConnection.IsSuccess)
            {
                GetRatesFromLocal();
            }

            GetRatesFromCloud();
        }

        public void GetRatesFromLocal()
        {
            if (dataService.First<Rate>(false) == null)
            {
                //show icon off line
                Flag2 = true;
                Flag3 = false;
                Flag = true;
                return;
            }

            var listRates = dataService.Get<Rate>(false);
            foreach (var item in listRates)
            {
                Rates.Add(item);
            }
            GetSavedRates();
            IsRunning = false;
            IsEnabled = true;
            Flag2 = false;
            Flag = true;
            Flag3 = true;
            return;
        }

        public async void GetRatesFromCloud()
        {
            try
            {
                var client = new HttpClient();
                var client2 = new HttpClient();
                //Consumiendo el segundo servicio //Nombre tasas
                client2.BaseAddress = new Uri("https://gist.githubusercontent.com");
                var url2 = "/picodotdev/88512f73b61bc11a2da4/raw/9407514be22a2f1d569e75d6b5a58bd5f0ebbad8";
                var responseNameRate = await client2.GetAsync(url2);

                //Precio tasas
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=8e58388d8f014422a049fef2fab8a61a";
                var response = await client.GetAsync(url);


                if (!response.IsSuccessStatusCode && !responseNameRate.IsSuccessStatusCode) //
                {
                    await dialogService.ShowMessage("Error", response.StatusCode.ToString());
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();//result tiene el string retornado por el servicio
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);//Deserealiza el string retornado por la respuesta

                //segundo servicio
                var result2 = await responseNameRate.Content.ReadAsStringAsync();
                nameExchangeRates = JsonConvert.DeserializeObject<NameExchangeRates>(result2);
            }
            catch (Exception ex)
            {
                await dialogService.ShowMessage("Error", ex.Message);
                IsRunning = false;
                IsEnabled = false;
                return;
            }

            LoadRates();
            GetSavedRates();
            IsRunning = false;
            IsEnabled = true;
        }

        //Get Last convertión
        public void GetSavedRates()
        {
            var indexRate = dataService.First<IndexRate>(false);
            if (indexRate != null)
            {
                Amount = indexRate.Amount;
                var r = Rates.ElementAt<Rate>(indexRate.SourceIndex);
                SourceRate = r.TaxRate;
                r = Rates.ElementAt<Rate>(indexRate.TargetIndex);
                TargetRate = r.TaxRate;
                SourceSelectedIndex = indexRate.SourceIndex;
                TargetSelectedIndex = indexRate.TargetIndex;
                ConvertMoney();
            }
        }
        #endregion

        #region Commands
        public ICommand InvertCommand { get { return new RelayCommand(Change); } }

        private void Change()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            ConvertMoney();
        }

        public ICommand ConvertCommand { get { return new RelayCommand(ConvertMoney); } }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await dialogService.ShowMessage("Error", "Debes ingresar un valor a convertir");
                return;
            }

            if (SourceRate == 0)
            {
                await dialogService.ShowMessage("Error", "Debes seleccionar la moneda origen");
                return;
            }

            if (TargetRate == -1)
            {
                await dialogService.ShowMessage("Error", "Debes seleccionar la moneda destino");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} ({2}) = {1:N2} ({3})",
                amount,
                amountConverted,
                Rates.ElementAt<Rate>(sourceSelectedIndex).Code,
                Rates.ElementAt<Rate>(targetSelectedIndex).Code);
            //save ObservableCollectio Index
            dataService.DeleteAllAndInsert<IndexRate>(new IndexRate
            {
                SourceIndex = sourceSelectedIndex,
                TargetIndex = targetSelectedIndex,
                Amount = amount,
            });
        }


        #endregion

    }
}
