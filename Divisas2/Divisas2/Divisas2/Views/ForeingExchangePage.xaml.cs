using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Divisas2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForeingExchangePage : ContentPage
    {
        public ForeingExchangePage()
        {
            InitializeComponent();
            NavigationPage.SetTitleIcon(this, "money.png");
        }
    }
}