using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Composition;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using BWDB.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BWDB.Universal
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage CurrentPage;
        Compositor compositor;
        SpriteVisual topSprite;
        Binding AdaptiveVisibilityBinding; //LeftFrame = !MainFrame

        static NotifyChanged<Product> currentProduct = new NotifyChanged<Product>();
        public static NotifyChanged<Product> CurrentProduct { get => currentProduct; set => currentProduct = value; }

        public MainPage()
        {
            this.InitializeComponent();
            CurrentPage = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            AdaptiveVisibilityBinding = new Binding();
            AdaptiveVisibilityBinding.Source = MainPageFrame;
            AdaptiveVisibilityBinding.Path = new PropertyPath("Visibility");
            AdaptiveVisibilityBinding.Converter = new VisibilityReverseConverter();
            AdaptiveVisibilityBinding.Mode = BindingMode.TwoWay;

            var isPhoneUI = (AdaptiveState.CurrentState == PhoneUI);

            //设置标题栏
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            var coreViewTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;

            appTitleBar.BackgroundColor = Colors.Transparent;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.InactiveBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            
            coreViewTitleBar.ExtendViewIntoTitleBar = true;
            
            if (isPhoneUI)
            {
                LeftPageFrame.SetBinding(Frame.VisibilityProperty, AdaptiveVisibilityBinding);
                appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemChromeWhiteColor"]);
            }

            // 返回键事件
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            
            if (DeviceTypeState.CurrentState == Phone)
            {
                //设置手机状态栏
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = ((Color)Application.Current.Resources["BWDB_AccentColor"]);
                statusBar.ForegroundColor = ((Color)Application.Current.Resources["SystemChromeWhiteColor"]);
                statusBar.BackgroundOpacity = 0.8;
            }

            //获取系统版本号
            //sv是版本号字符串 十六进制下四位分组得到 A.B.C.D 的格式 如 10.0.15063.0
            var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;

            //RS2透明效果测试
            if (v3 >= 15031 && DeviceTypeState.CurrentState == Desktop)
            {
                compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

                //创建spriteVisual
                topSprite = compositor.CreateSpriteVisual();
                //spriteVisual的控制范围
                if (!isPhoneUI)
                {
                    topSprite.Size = new System.Numerics.Vector2((float)PanelGrid.ActualWidth, (float)PanelGrid.ActualHeight);
                }
                ElementCompositionPreview.SetElementChildVisual(PanelGrid, topSprite);

                topSprite.Brush = compositor.CreateHostBackdropBrush();
                
            }

            GetProductList();

            //第一次启动的时候强制刷新一下SelectItem
            if (ProductZoomInListView != null)
            {
                ProductZoomInListView.SelectedItem = ProductZoomInListView.Items[0];
            }
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var navigationView = SystemNavigationManager.GetForCurrentView();
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            var isPhoneUI = (AdaptiveState.CurrentState == PhoneUI);

            if (isPhoneUI && MainPageFrame.Visibility == Visibility.Visible)
            {
                MainPageFrame.Visibility = Visibility.Collapsed;

                navigationView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                e.Handled = true;

                appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemChromeWhiteColor"]);
            }
        }

        private void AdaptiveState_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {

            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            var navigationView = SystemNavigationManager.GetForCurrentView();

            if (e.NewState == PhoneUI)
            {
                LeftPageFrame.SetBinding(Frame.VisibilityProperty, AdaptiveVisibilityBinding);

                var isLeftPageVisible = (LeftPageFrame.Visibility == Visibility.Visible);

                if (isLeftPageVisible)
                {
                    appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemChromeWhiteColor"]);
                    navigationView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
                else
                {
                    appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    navigationView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }
                
                
            }
            else if (e.NewState == DesktopUI)
            {
                LeftPageFrame.SetBinding(Frame.VisibilityProperty, new Binding());
                if (MainPageFrame.SourcePageType != null)
                {
                    MainPageFrame.Visibility = Visibility.Visible;
                }
                appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemBaseHighColor"]);
                navigationView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (topSprite !=null)
            {
                var size = topSprite.Size;
                size.X = (float)PanelGrid.ActualWidth;
                size.Y = (float)PanelGrid.ActualHeight;
                topSprite.Size = size;
            }

            System.Diagnostics.Debug.WriteLine("a");
        }


        private async void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = false;
            SettingDialog.Focus(FocusState.Programmatic);
            SettingFrame.BackStack.Clear();
            SettingFrame.Navigate(typeof(SettingPage));
            await SettingDialog.ShowAsync();
        }


        //获取BuildList
        public void GetBuildList(int ProductID)
        {
            var Builds = App.OSInformation.GetBuilds(ProductID);
            var groupedBuilds = Builds.OrderBy(p => p.BuildID).GroupBy(p => p.Stage);

            var CollectionViewSource = new CollectionViewSource()
            {
                Source = groupedBuilds,
                IsSourceGrouped = true
            };
            BuildZoomInListView.ItemsSource = CollectionViewSource.View;
            BuildZoomOutListView.ItemsSource = CollectionViewSource.View.CollectionGroups;

            BuildZoomInListView.SelectedItem = null;
        }

        public void GetBuildList(string Keyword)
        {
            var Builds = App.OSInformation.GetBuilds(Keyword);
            var groupedBuilds = Builds.OrderBy(p => p.ProductID).ThenBy(p=>p.BuildID).GroupBy(p => p.ProductName);

            var CollectionViewSource = new CollectionViewSource()
            {
                Source = groupedBuilds,
                IsSourceGrouped = true
            };
            BuildZoomInListView.ItemsSource = CollectionViewSource.View;
            BuildZoomOutListView.ItemsSource = CollectionViewSource.View.CollectionGroups;

            BuildZoomInListView.SelectedItem = null;
        }

        //获取ProductList
        private void GetProductList()
        {
            if (ProductZoomInListView == null) return;

            //hack，防止刷新ProductList时发生SelectionChanged 然后列表被隐藏
            ProductZoomInListView.SelectionChanged -= ProductZoomInListView_SelectionChanged;

            var Products = App.OSInformation.GetProducts();
            IEnumerable<IGrouping<string, Product>> groupedProducts = null;

            //判断分类模式
            if (RadioButton_Year.IsChecked == true)
            {
                groupedProducts = Products.OrderBy(p => p.Year).GroupBy(p => p.Year.ToString());
            }
            else if (RadioButton_ProductLine.IsChecked == true)
            {
                groupedProducts = Products.OrderBy(p => p.TagID).ThenBy(p => p.ProductID).GroupBy(p => p.Tag);
            }
            else if (RadioButton_ProductFamily.IsChecked == true)
            {
                groupedProducts = Products.OrderBy(p => p.FamilyID).ThenBy(p => p.ProductID).GroupBy(p => p.Family);
            }

            var CollectionVS = new CollectionViewSource()
            {
                Source = groupedProducts,
                IsSourceGrouped = true
            };
            ProductZoomInListView.ItemsSource = CollectionVS.View;
            ProductZoomOutListView.ItemsSource = CollectionVS.View.CollectionGroups;

            ProductZoomInListView.SelectedItem = null;

            //恢复SelectionChanged
            ProductZoomInListView.SelectionChanged += ProductZoomInListView_SelectionChanged;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            GetProductList();
        }

        private void ProductZoomInListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentProduct.Item = ProductZoomInListView.SelectedItem as Product;

            if (CurrentProduct.Item != null)
            {
                GetBuildList(CurrentProduct.Item.ProductID);

                var isPhoneUI = (AdaptiveState.CurrentState == PhoneUI);
                if (!isPhoneUI && BuildZoomInListView.Items.Count > 0)
                {
                    NavigateToBuild(BuildZoomInListView.Items[0] as Build);
                }

                SplitView.IsPaneOpen = false;
            }

        }


        private void BuildZoomInListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var Build = e.ClickedItem as Build;
            NavigateToBuild(Build);
        }

        private void NavigateToBuild(Build build)
        {
            if (build != null)
            {
                var View = SystemNavigationManager.GetForCurrentView();
                var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;

                var isPhoneUI = (AdaptiveState.CurrentState == PhoneUI);
                if (isPhoneUI)
                {
                    View.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }

                MainPageFrame.BackStack.Clear();
                MainPageFrame.Navigate(typeof(DetailPage), build);
                MainPageFrame.Visibility = Visibility.Visible;
                appTitleBar.ButtonForegroundColor = ((Color)Application.Current.Resources["SystemBaseHighColor"]);
            }

        }
        
        private void SearchToggle_Checked(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus(FocusState.Keyboard);
            BuildZoomInListView.ItemsSource = null;
            BuildZoomOutListView.ItemsSource = null;
        }

        private void SearchToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            GetBuildList(currentProduct.Item.ProductID);
            SearchBox.Text = "";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchToggle.IsChecked == false) return;

            if (SearchBox.Text != "")
            {
                GetBuildList(SearchBox.Text);
            }
            else
            {
                BuildZoomInListView.ItemsSource = null;
                BuildZoomOutListView.ItemsSource = null;
            }
        }

        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            Appbar.IsOpen = false;
        }

        private void SettingDialogCloseButton_Click(object sender, RoutedEventArgs e)
        {
            SettingDialog.Hide();
        }
    }
}
