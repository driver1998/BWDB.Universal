using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using BWDB.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BWDB.Universal
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailPage : Page
    {
        //private Build currentBuild;
        public Build CurrentBuild { get; set; }
        public static DetailPage CurrentPage;


        DataTransferManager DataTransfer;
        bool ScreenshotDialogOpened;

        public DetailPage()
        {
            this.InitializeComponent();

            CurrentPage = this;
            DataTransfer = DataTransferManager.GetForCurrentView();

            ScreenshotDialog.Opened += (sender, args) => ScreenshotDialogOpened = true;
            
            ScreenshotDialog.Closed += (sender, args) => ScreenshotDialogOpened = false;

            DataTransfer.DataRequested += (sender, args) =>
            {
                if (ScreenshotDialogOpened)
                {
                    System.Diagnostics.Debug.WriteLine(((Screenshot)ScreenshotFlipView.SelectedItem).Image.UriSource);
                    
                    
                    //args.Request.Data.SetBitmap();
                }
                else
                {
                    args.Request.Data.SetText(CurrentBuild.Buildtag);
                }
                args.Request.Data.SetWebLink(new Uri(@"http://www.betaworld.cn"));
                
                args.Request.Data.Properties.Title = "BetaWorld Library";
                
                // args.Request.Data.SetUri();
            };
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Build)
            {
                CurrentBuild = e.Parameter as Build;
                var screenshotList = await CurrentBuild.GetSceenshots(App.ScreenshotFolder);

                if (screenshotList != null && screenshotList.Count > 0)
                {
                    ScreenshotView.ItemsSource = screenshotList;
                    ScreenshotFlipView.SelectedItem = null;
                }
                else
                {
                    ScreenshotView.Visibility = Visibility.Collapsed;
                }

            }
        }




        private async void More_Click(object sender, RoutedEventArgs e)
        {
            var HeaderBinding = new Binding();
            var ContentBinding = new Binding();

            if (sender == SKUMoreButton)
            {
                HeaderBinding.Source = SKULabel;
                ContentBinding.Source = SKUText;
            }
            else if (sender == LanguageMoreButton)
            {
                HeaderBinding.Source = LanguageLabel;
                ContentBinding.Source = LanguageText;
            }
            else
            {
                HeaderBinding = null;
                ContentBinding = null;
            }

            HeaderBinding.Path = new PropertyPath("Text");
            ContentBinding.Path = new PropertyPath("Text");

            DetailDialogHeaderBlock.SetBinding(TextBlock.TextProperty, HeaderBinding);
            DetailDialogTextBlock.SetBinding(TextBlock.TextProperty, ContentBinding);

            await DetailDialog.ShowAsync();
        }

        

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == DetailDialogCloseButton)
            {
                DetailDialog.Hide();
            }
            else if (sender == InstallationDialogCloseButton)
            {
                InstallationDialog.Hide();
            }
            else if (sender == ScreenshotDialogCloseButton)
            {
                ScreenshotFlipView.SelectionChanged -= ScreenshotFlipView_SelectionChanged;
                ScreenshotFlipView.SelectedItem = null;
                ScreenshotDialog.Hide();
            }

        }

        private async void InstallInformation_Click(object sender, RoutedEventArgs e)
        {
            await InstallationDialog.ShowAsync();
        }
        
        private async void AdjustSize()
        {
            
            if (VisualTreeHelper.GetChildrenCount(ScreenshotFlipView) == 0) return;
            if ((ScreenshotFlipView).SelectedItem == null) return;

            //FlipView下的Grid
            var grid = (Grid)VisualTreeHelper.GetChild(ScreenshotFlipView, 0);

            //FlipView中隐藏的ScrollViewer
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(grid, 0);
            var border = (Border)VisualTreeHelper.GetChild(scrollViewer, 0);
            grid = (Grid)VisualTreeHelper.GetChild(border, 0);
            var scrollContentPresenter = (ScrollContentPresenter)VisualTreeHelper.GetChild(grid, 0);

            //FlipView的Content和ItemsHost
            var itemsPresenter = (ItemsPresenter)VisualTreeHelper.GetChild(scrollContentPresenter, 0);
            var virtualizingStackPanel = (VirtualizingStackPanel)VisualTreeHelper.GetChild(itemsPresenter, 1);

            FlipViewItem selectedItem = null;
            foreach (FlipViewItem item in virtualizingStackPanel.Children)
            {
                if (item.IsSelected)
                {
                    selectedItem = item;
                }
            }

            if (selectedItem == null) return;
            //FlipViewItem中自定义的ScrollViewer
            
            var contentPresenter = (ContentPresenter)VisualTreeHelper.GetChild(selectedItem, 0);
            grid = (Grid)VisualTreeHelper.GetChild(contentPresenter, 0);
            scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(grid, 0);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                scrollViewer.ChangeView(0f, 0f, 1.0f, true);

            }
            );

        }
        private void ScreenshotFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           AdjustSize();

        }

        private void buildPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rect = ApplicationView.GetForCurrentView().VisibleBounds;
            

            if (DeviceFamilyState.CurrentState == Desktop )
            {
                if (MainPage.CurrentPage.AdaptiveState.CurrentState == MainPage.CurrentPage.PhoneUI)
                {
                    ScreenshotDialog.MaxWidth = rect .Width;
                }
                else
                {
                    ScreenshotDialog.MaxWidth = rect.Width - 70;
                }
                
                ScreenshotDialog.MaxHeight = rect.Height - 70;
                DetailDialog.MaxHeight = rect.Height - 70;
                InstallationDialog.MaxHeight = rect.Height - 70;
            }
            else
            {
                
                ScreenshotDialog.MinHeight = rect.Height;
                ScreenshotFlipView.MinHeight = ScreenshotDialog.MinHeight;
                ScreenshotDialog.MaxWidth = rect.Width;
                ScreenshotDialog.MaxHeight = rect.Height;
            }

        }
        
        private void ScreenshotFlipView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }
        
        private void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                e.Handled = true;
            }
        }

        private void image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {


            //System.Diagnostics.Debug.WriteLine(1 + (e.Delta.Scale - 1) * 10);


            
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var image = (Image)sender;
                var delta = e.GetCurrentPoint(image).Properties.MouseWheelDelta;

                var grid = (Grid)VisualTreeHelper.GetParent(image);
                var scrollContentPresenter = (ScrollContentPresenter)VisualTreeHelper.GetParent(grid);
                var scrollGrid = (Grid)VisualTreeHelper.GetParent(scrollContentPresenter);
                var scrollBorder = (Border)VisualTreeHelper.GetParent(scrollGrid);

                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetParent(scrollBorder);

                var scrollCenterX = scrollViewer.ActualWidth / 2;
                var scrollCenterY = scrollViewer.ActualHeight / 2;
                var centerX = image.ActualWidth / 2;
                var centerY = image.ActualHeight / 2;
                var scale = scrollViewer.ZoomFactor + delta / 480f;

                scrollViewer.ChangeView(-(scrollCenterX - (centerX * scale)), -(scrollCenterY - (centerY * scale)), scale);
                e.Handled = true;
            }

            
        }

        private void image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var image = (Image)sender;
            var grid = (Grid)VisualTreeHelper.GetParent(image);
            var scrollContentPresenter = (ScrollContentPresenter)VisualTreeHelper.GetParent(grid);
            var scrollGrid = (Grid)VisualTreeHelper.GetParent(scrollContentPresenter);
            var scrollBorder = (Border)VisualTreeHelper.GetParent(scrollGrid);

            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetParent(scrollBorder);

            //System.Diagnostics.Debug.WriteLine(e.Velocities.Linear.X);

            //if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)


            var scrollCenterX = scrollViewer.ActualWidth / 2;
            var scrollCenterY = scrollViewer.ActualHeight / 2;
            //System.Diagnostics.Debug.WriteLine(1 + (e.Delta.Scale - 1) * 10);
            if (e.Delta.Scale != 1)
            {
                var centerX = image.ActualWidth / 2;
                var centerY = image.ActualHeight / 2;
                var scale = scrollViewer.ZoomFactor * (1 + (e.Delta.Scale - 1) * 2);
                
                scrollViewer.ChangeView(
                    
                    -(scrollCenterX-(centerX *scale)), 
                    -(scrollCenterY-(centerY*scale)), 
                    scale, false);
               
            }
            else
            {
                scrollViewer.ChangeView(
                   scrollViewer.HorizontalOffset - e.Delta.Translation.X,
              scrollViewer.VerticalOffset - e.Delta.Translation.Y,
                    null, true);
            }
            e.Handled = true;

        }

        private void image_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                var image = (Image)sender;
                var grid = (Grid)VisualTreeHelper.GetParent(image);
                var scrollContentPresenter = (ScrollContentPresenter)VisualTreeHelper.GetParent(grid);
                var scrollGrid = (Grid)VisualTreeHelper.GetParent(scrollContentPresenter);
                var scrollBorder = (Border)VisualTreeHelper.GetParent(scrollGrid);

                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetParent(scrollBorder);
                if (scrollViewer.ZoomFactor == 1)
                {

                    var count = ((List<Screenshot>)ScreenshotFlipView.ItemsSource).Count;
                    var index = ScreenshotFlipView.SelectedIndex;

                    if (e.Velocities.Linear.X > 0)
                    {
                        if (index - 1 >= 0) ScreenshotFlipView.SelectedIndex -= 1;

                    }
                    else
                    {
                        if (index + 1 < count) ScreenshotFlipView.SelectedIndex += 1;
                    }
                }
            }

        }

        private async void image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            
            var image = (Image)sender;

            var grid = (Grid)VisualTreeHelper.GetParent(image);
            var scrollContentPresenter = (ScrollContentPresenter)VisualTreeHelper.GetParent(grid);
            var scrollGrid = (Grid)VisualTreeHelper.GetParent(scrollContentPresenter);
            var scrollBorder = (Border)VisualTreeHelper.GetParent(scrollGrid);

            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetParent(scrollBorder);
            //await Task.Delay(1);


            //scrollViewer.ChangeView(0f, 0f, null, false);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                scrollViewer.ChangeView(0f, 0f, 1.0f, true);

            }
            );


        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            
            DataTransferManager.ShowShareUI();
        }

        private void image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
            /*
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                ((Image)sender).ManipulationMode = ManipulationModes.TranslateX & ManipulationModes.TranslateY;
            }
            else
            {
                ((Image)sender).ManipulationMode = ManipulationModes.System;
            }*/

        }

        private async void ScreenshotDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (DeviceFamilyState.CurrentState == Phone)
            {

                //设置手机状态栏
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            Windows.System.Threading.ThreadPoolTimer.CreateTimer(
                async (source) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ScreenshotFlipView.SelectedItem = (Screenshot)ScreenshotView.SelectedItem;
                    }
                    );
                }
            , TimeSpan.FromMilliseconds(10));
        }

        private async void ScreenshotView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            await ScreenshotDialog.ShowAsync();
        }

        private async void ScreenshotDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (DeviceFamilyState.CurrentState == Phone)
            {   
                //设置手机状态栏
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            }

            ScreenshotView.SelectionChanged -= ScreenshotView_SelectionChanged;
            ScreenshotView.SelectedItem = null;
            ScreenshotView.SelectionChanged += ScreenshotView_SelectionChanged;
        }

        private async void InstallationDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (DeviceFamilyState.CurrentState == Phone)
            {
                //设置手机状态栏
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }

        private async void InstallationDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (DeviceFamilyState.CurrentState == Phone)
            {
                //设置手机状态栏
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            }
        }


    }
}
