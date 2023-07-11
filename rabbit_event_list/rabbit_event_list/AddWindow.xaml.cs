using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace rabbit_event_list
{
    /// <summary>
    /// AddWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddWindow : Window
    {
        MainWindow mainWindow = null;

        //指示查看时，选择的数据索引
        int selectIndex = -1;
        public AddWindow(MainWindow tempWindow,int mainSelectIndex)
        {
            InitializeComponent();

            mainWindow = tempWindow;


            //窗口位置 在屏幕中央
            this.Left = SystemParameters.PrimaryScreenWidth / 2 - this.Width / 2;
            this.Top = SystemParameters.PrimaryScreenHeight / 2 - this.Height / 2;

            //初始化下拉框
            //年份 从当前年份往前10年到往后30年
            DateTime timeNow = DateTime.Now;
            this.cbox_year.ItemsSource = getItemSource(timeNow.Year - 10, timeNow.Year + 30);
            //月份
            this.cbox_month.ItemsSource = getItemSource(1, 12);
            //日 不管月份差别了 28原则 这程序主要目的不是这个
            this.cbox_day.ItemsSource = getItemSource(1, 31);
            //时
            this.cbox_hour.ItemsSource = getItemSource(0, 23);
            //分
            this.cbox_min.ItemsSource = getItemSource(0, 59);

            //初始化下拉框选择
            timeCboxInit(timeNow);
            //隐藏删除按钮
            this.button_del.Visibility = Visibility.Hidden;

            if (mainSelectIndex != -1)
            {
                selectIndex = mainSelectIndex;
                //编辑业务 需要把该数据展示到界面上
                EventsListItem tempItem = mainWindow.eventsList[selectIndex];

                //Debug.WriteLine("执行修改 "+ selectIndex);
                //Debug.WriteLine(JsonConvert.SerializeObject(tempItem));

                timeCboxInit(DateTime.Parse(tempItem.EventsTime));
                this.text_title.Text = tempItem.EventsTitle;

                //启用删除按钮
                this.button_del.Visibility = Visibility.Visible;
            }

        }

        //初始化下拉框选择为当前时间
        private void timeCboxInit(DateTime timeNow)
        {
            this.cbox_year.SelectedValue = timeNow.Year.ToString();
            this.cbox_month.SelectedValue = timeNow.Month.ToString();
            this.cbox_day.SelectedValue = timeNow.Day.ToString();
            this.cbox_hour.SelectedValue = timeNow.Hour.ToString();
            this.cbox_min.SelectedValue = timeNow.Minute.ToString();
        }

        //保存新项目
        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            //检查日期是否有效
            string inputDateStr = null;
            string inputTitle = this.text_title.Text;

            try
            {
                string year = this.cbox_year.SelectedValue.ToString();
                string month = this.cbox_month.SelectedValue.ToString();
                string day = this.cbox_day.SelectedValue.ToString();
                string hour = this.cbox_hour.SelectedValue.ToString();
                string min = this.cbox_min.SelectedValue.ToString();

                inputDateStr = String.Format("{0}-{1}-{2} {3}:{4}", year, month, day, hour, min);

                //尝试格式化，并以转换后的DateTime为准
                DateTime tempTime = DateTime.Parse(inputDateStr);
                inputDateStr = tempTime.ToString("yyyy-MM-dd HH:mm");

                if (null == inputDateStr || inputDateStr.Trim() == "")
                {
                    throw new IOException("这里是时间转化结果为空，理论不会出现");
                }
            }
            catch (Exception)
            {
                this.lab_err.Content = "日期格式有误";
                if (this.lab_err.Foreground == Brushes.Red)
                {
                    this.lab_err.Foreground = Brushes.Orange;
                }
                else
                {
                    this.lab_err.Foreground = Brushes.Red;
                }
                return;
            }

            Debug.WriteLine(inputDateStr + "  " + inputTitle);

            if (selectIndex != -1)
            {
                //修改已有数据 清除当前选中位置的数据，并重新插入新数据
                mainWindow.eventsList.RemoveAt(selectIndex);
            }

            //插入新数据 并重新排序
            mainWindow.eventsList.Add(new EventsListItem(inputDateStr, inputTitle, ""));
            mainWindow.eventsList = mainWindow.sortList(mainWindow.eventsList);
            mainWindow.listViewRefresh(mainWindow.eventsList);
            
            selectIndex = -1;
            //初始化输入框
            timeCboxInit(DateTime.Now);
            this.text_title.Text = "";
            this.Hide();
        }

        //取消新增
        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            //初始化输入框
            timeCboxInit(DateTime.Now);
            this.text_title.Text = "";
            selectIndex = -1;

            //隐藏当前窗口
            this.Hide();
        }

        //删除
        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            if (selectIndex == -1)
            {
                this.lab_err.Content = "删除出错了!";
                return;
            }

            mainWindow.eventsList.RemoveAt(selectIndex);
            mainWindow.eventsList = mainWindow.sortList(mainWindow.eventsList);
            mainWindow.list_events.ItemsSource = mainWindow.eventsList;

            //Debug.WriteLine("删除 "+ selectIndex);
            //Debug.WriteLine(JsonConvert.SerializeObject(mainWindow.eventsList));

            //初始化输入框
            timeCboxInit(DateTime.Now);
            this.text_title.Text = "";
            selectIndex = -1;

            //隐藏当前窗口
            this.Hide();
        }

        //左键拖拽窗口
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private List<string> getItemSource(int startNum, int endNum)
        {
            List<string> tempList = new List<string>();
            for (int i = startNum; i <= endNum; i++)
            {
                tempList.Add(i.ToString());
            }
            return tempList;
        }

    }
}
