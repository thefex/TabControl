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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace TabControl.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            //TabLayoutFlipView.TabItemsSource = new List<string> {"ANDROID", "TABLAYOUT", "PORT"};

            //TabLayoutFlipView.ItemsSource = new List<TabContent>()
            //{
            //    new TabContent() { Content = "This is first tab of TabIndiciatorLayoutFlipView"},
            //    new TabContent() { Content = "That's a place where settings live."},
            //    new TabContent() { Content = "About application page"}
            //};

            var tabItems = Enumerable.Range(1, 50).Select(x => "Tab number " + x).ToList();
            tabItems[0] = "QWOEQWRO QKWOR KQOEK OQWek OQEK OQKWEQKOWEQew";
            SecondaryTabLayoutFlipView.TabItemsSource = tabItems;

            SecondaryTabLayoutFlipView.ItemsSource = tabItems.Select(x => new TabContent()
            {
                Content = "This is content of tab: " + x
            });
        }

        class TabContent
        {
            public string Content { get; set; }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
