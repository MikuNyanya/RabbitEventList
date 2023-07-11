using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace rabbit_event_list
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //同一时间只启动一个程序
        private static Mutex mutex;
        public App() { this.Startup += new StartupEventHandler(App_Startup); }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new Mutex(true,"RabbitEventList",out ret);
            if (!ret)
            {
                MessageBox.Show("兔叽行程列表已经运行");
                Environment.Exit(0);
            }
        }
    }
}
