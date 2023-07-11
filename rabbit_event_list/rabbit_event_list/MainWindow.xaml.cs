using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace rabbit_event_list
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<EventsListItem> eventsList = new List<EventsListItem>();
        Window addWindow = null;
        DispatcherTimer dpTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            //配置文件默认在 C:\Users\{UserName}\AppData\Local\{AppName}\{Version}\xxx.config
            //读取历史窗口位置
            this.Left = rabbitsettings.Default.winpositionX;
            this.Top = rabbitsettings.Default.winpositionY;

            //读取列表
            string listJson = rabbitsettings.Default.eventlistjson;
            if (null == listJson || "" == listJson.Trim())
            {
                listJson = "[]";
            }

            eventsList = JsonConvert.DeserializeObject<List<EventsListItem>>(listJson);
            eventsList = sortList(eventsList);
            listViewRefresh(eventsList);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("初始化定时器");
            //初始化定时器 每秒执行一次
            dpTimer.Interval = TimeSpan.FromSeconds(1);
            dpTimer.Tick += Timer_Exec;
            dpTimer.Start();
        }

        private void Timer_Exec(object sender, EventArgs e)
        {
            //Debug.WriteLine(DateTime.Now.ToString());
            //查看列表第一个时间是否已过期，如果已过期则进行列表清理重新排序
            if (null == eventsList|| eventsList.Count <= 0)
            {
                return;
            }

            if (DateTime.Parse(eventsList[0].EventsTime) >= DateTime.Now)
            {
                return;
            }

            for (int i = 0; i < eventsList.Count;)
            {
                if (DateTime.Parse(eventsList[i].EventsTime) < DateTime.Now)
                {
                    //Debug.WriteLine("删除 ");
                    //Debug.WriteLine(JsonConvert.SerializeObject(eventsList[i]));

                    eventsList.RemoveAt(i);
                    continue;
                }

                i++;
            }

            eventsList = sortList(eventsList);
            Debug.WriteLine(JsonConvert.SerializeObject(eventsList));
            this.list_events.ItemsSource = eventsList;
        }

        //左键拖拽窗口
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public List<EventsListItem> sortList(List<EventsListItem> itemList)
        {
            if (null == itemList || itemList.Count() <= 0)
            {
                return itemList;
            }

            EventsListItem[] tempArray = itemList.ToArray();
            //冒泡排序 时间正序
            for (int i = 0; i < tempArray.Length; i++)
            {  
                for (int j = i + 1; j < tempArray.Length; j++)
                {
                    DateTime dateTimeI = DateTime.Parse(tempArray[i].EventsTime);
                    DateTime dateTimeJ = DateTime.Parse(tempArray[j].EventsTime);
                    if (dateTimeI > dateTimeJ)
                    {
                        EventsListItem tempItem = tempArray[i];
                        tempArray[i] = tempArray[j];
                        tempArray[j] = tempItem;
                        break;
                    }
                }
            }

            //更新展示用的时间
            for (int i = 0;i< tempArray.Length; i++)
            {
                tempArray[i].EventsTimeShort = parseEventsTimeShort(tempArray[i].EventsTime);
            }
            
            return new List<EventsListItem>(tempArray);
        }

        private string parseEventsTimeShort(string eventsTime)
        {
            DateTime timeNow = DateTime.Now;
            DateTime timeEvents = DateTime.Parse(eventsTime);
            //如果今天的，只显示时间即可
            if (timeNow.Year == timeEvents.Year && timeNow.Month == timeEvents.Month && timeNow.Day == timeEvents.Day)
            {
                return timeEvents.ToString("HH:mm");
            }

            //如果是年内的，显示日期
            if (timeNow.Year == timeEvents.Year)
            {
                return timeEvents.ToString("MM-dd");
            }

            //如果不是今年的，显示年月日
            return timeEvents.ToString("yyyy-MM-dd");
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            this.button_add.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 255, 255, 255));
            this.button_add.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0));
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            this.button_add.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255));
            this.button_add.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
        }

        //添加新项目
        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            //弹出新窗口，在新窗口进行添加业务
            if (null != addWindow)
            {
                addWindow.Close();
            }
            addWindow = new AddWindow(this, -1);
            addWindow.Show();
        }

        //点击编辑
        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventsListItem selectedItem = list_events.SelectedItem as EventsListItem;
            if (null == selectedItem)
            {
                return;
            }

            //找出该数据在列表中的位置 这时候还是觉得该给数据带个id... 算了
            int selectedIndex = -1;
            for (int i = 0; i < eventsList.Count; i++)
            {
                //因为要对时间做缩短处理，所以不能对比时间，只能对比文本  果然还是得给个id吧
                if (selectedItem.EventsTime == eventsList[i].EventsTime && selectedItem.EventsTitle == eventsList[i].EventsTitle)
                {
                    selectedIndex = i;
                    break;
                }
            }
            //已经能想到的bug，如果在自动任务分类列表元素的时候用户操作删除，那会删掉错误的条目，真的得给个id啊...还是算了改写好麻烦

            //Debug.WriteLine(JsonConvert.SerializeObject(selectedItem));
            //Debug.WriteLine(selectedIndex);

            if (null != addWindow)
            {
                addWindow.Close();
            }
            addWindow = new AddWindow(this, selectedIndex);
            addWindow.Show();
        }

        public void listViewRefresh(List<EventsListItem> dataList)
        {
            this.list_events.ItemsSource = null;
            this.list_events.ItemsSource = dataList;
        }

        //窗口位置重置
        private void Menu_RePosition_Click(object sender, RoutedEventArgs e)
        {
            //窗口位置重置
            this.Left = 600;
            this.Top = 300;

        }

        //程序退出
        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            //记录窗口最后位置
            rabbitsettings.Default.winpositionX = this.RestoreBounds.Left;
            rabbitsettings.Default.winpositionY = this.RestoreBounds.Top;
            //记录事件列表
            rabbitsettings.Default.eventlistjson = JsonConvert.SerializeObject(eventsList);

            rabbitsettings.Default.Save();

            Application.Current.Shutdown();
        }

    }

    public class EventsListItem
    {
        public EventsListItem() { }

        public EventsListItem(string eventsTime, string eventsTitle, string eventsText)
        {
            this.EventsTime = eventsTime;
            this.EventsTitle = eventsTitle;
            this.EventsText = eventsText;
        }

        //事情时间
        public string EventsTime { get; set; }
        //短时间
        public string EventsTimeShort { get; set; }
        //事情标题
        public string EventsTitle { get; set; }
        //事情详情 暂未使用
        public string EventsText { get; set; }
    }


    //public class Shell_TrayWndHelper
    //{
    //    private const int SwHide = 0;
    //    private const int SwRestore = 9;

    //    [DllImport("user32.dll")]
    //    private static extern int ShowWindow(int hwnd, int nCmdShow);
    //    [DllImport("user32.dll")]
    //    private static extern int FindWindow(string lpClassName, string lpWindowName);

    //    public int Hide()
    //    {   
    //        return ShowWindow(FindWindow("Shell_TrayWnd", null), SwHide);
    //    }

    //    public int Show()
    //    {
    //        return ShowWindow(FindWindow("Shell_TrayWnd", null), SwRestore);
    //    }
    //}
}
