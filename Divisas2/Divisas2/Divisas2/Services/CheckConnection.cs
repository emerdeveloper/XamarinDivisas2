using Divisas2.Modals;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Divisas2.Services
{
    public class CheckConnection
    {
        public async Task<Response> Check()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Please turn your internet",
                };
            }

            var isRechable = await CrossConnectivity.Current.IsRemoteReachable("www.google.com.ar");

            if (!isRechable)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Please check you internet connection",
                };
            }

            return new Response
            {
                IsSuccess = true,
                Message = "Ok",
            };
        }

        /*public async Task<Response> ListenerConnection()
        {
            var r = new Response();
            CrossConnectivity.Current.ConnectivityChanged += (s, ev) =>
            {
                r = new Response
                {
                    IsSuccess = ev.IsConnected,
                    Message = "",
                };
            };
            return r;
        }
        */
    }
}
