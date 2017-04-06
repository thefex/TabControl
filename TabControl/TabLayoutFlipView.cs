using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TabControl
{
    public class TabLayoutFlipView : FlipView
    {
        public static readonly DependencyProperty SelectedTabTextColorBrushProperty = DependencyProperty.Register(
            "SelectedTabTextColorBrush", typeof(Brush), typeof(TabLayoutFlipView),
            new PropertyMetadata(new SolidColorBrush(Colors.DeepPink)));

        public static readonly DependencyProperty InactiveTabTextColorBrushProperty = DependencyProperty.Register(
            "InactiveTabTextColorBrush", typeof(Brush), typeof(TabLayoutFlipView),
            new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        public static readonly DependencyProperty TabItemsSourceProperty = DependencyProperty.Register(
            "TabItemsSource", typeof(IEnumerable), typeof(TabLayoutFlipView),
            new PropertyMetadata(default(IEnumerable)));

        private readonly List<ListViewItem> listViewItems = new List<ListViewItem>();


        private bool hasLayoutScrollViewerInitialized;
        private int itemsCount;

        private double itemWidth;
        private ScrollViewer layoutScrollViewer;

        private int selectedIndex;
        private ListView tabsListView;

        public TabLayoutFlipView()
        {
            DefaultStyleKey = typeof(TabLayoutFlipView);
        }

        public Brush SelectedTabTextColorBrush
        {
            get { return (Brush)GetValue(SelectedTabTextColorBrushProperty); }
            set { SetValue(SelectedTabTextColorBrushProperty, value); }
        }

        public Brush InactiveTabTextColorBrush
        {
            get { return (Brush)GetValue(InactiveTabTextColorBrushProperty); }
            set { SetValue(InactiveTabTextColorBrushProperty, value); }
        }

        public IEnumerable TabItemsSource
        {
            get { return (IEnumerable)GetValue(TabItemsSourceProperty); }
            set { SetValue(TabItemsSourceProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            layoutScrollViewer = GetTemplateChild("ScrollingHost") as ScrollViewer;
            tabsListView = GetTemplateChild("TabListView") as ListView;

            tabsListView.ItemsSource = TabItemsSource;
            tabsListView.ItemClick += TabsListViewOnItemClick;
            tabsListView.IsItemClickEnabled = true;
            tabsListView.SelectionMode = ListViewSelectionMode.None;

            layoutScrollViewer.ViewChanging += LayoutScrollViewerOnViewChanging;
            layoutScrollViewer.ViewChanged += LayoutScrollViewerOnViewChanged;
        }

        private async void TabsListViewOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var listViewSelectedIndex = listViewItems.FindIndex(x => x.Content.Equals(itemClickEventArgs.ClickedItem));

            if (listViewSelectedIndex == -1)
                return;

            await ScrollToIndex(listViewSelectedIndex);
        }

        public async Task ScrollToIndex(int indexToScrollIn)
        {
            var pageDistance = Math.Abs(indexToScrollIn - SelectedIndex);
            var isPageFarAwayAndNotTooFar = pageDistance > 1 && pageDistance <= 4;

            if (indexToScrollIn < listViewItems.Count && indexToScrollIn >= 0)
            tabsListView.ScrollIntoView(listViewItems[indexToScrollIn].DataContext, ScrollIntoViewAlignment.Default);

            if (!isPageFarAwayAndNotTooFar)
            {
                SetTextColorBrushForTab(SelectedIndex, InactiveTabTextColorBrush);
                SelectedIndex = indexToScrollIn;
                SetTextColorBrushForTab(SelectedIndex, SelectedTabTextColorBrush);
                return;
            }

            while (SelectedIndex != indexToScrollIn)
            {
                SelectedIndex += (SelectedIndex > indexToScrollIn) ? -1 : 1;

                if (SelectedIndex != indexToScrollIn)
                    await Task.Delay(10);
            }
        }

        private void LayoutScrollViewerOnViewChanged(object sender,
            ScrollViewerViewChangedEventArgs scrollViewerViewChangedEventArgs)
        {
            if (!hasLayoutScrollViewerInitialized)
            {
                for (var i = 0; i < tabsListView.Items.Count; ++i)
                {
                    var listViewItem = tabsListView.ContainerFromIndex(i) as ListViewItem;
                    listViewItem.InvalidateMeasure();
                    itemWidth = Math.Max(itemWidth, listViewItem.DesiredSize.Width);
                    listViewItems.Add(listViewItem);
                    listViewItem.Loaded += ListViewItemOnLoaded;
                }

                for (var i = 0; i < tabsListView.Items.Count; ++i)
                {
                    var listViewItem = tabsListView.ContainerFromIndex(i) as ListViewItem;
                    listViewItem.Width = itemWidth;
                    SetTextColorBrushForTab(i, InactiveTabTextColorBrush);
                }

                itemsCount = tabsListView.Items.Count;

                SetupIndicatorForOffset(SelectedIndex);
                hasLayoutScrollViewerInitialized = true;
                layoutScrollViewer.ViewChanged -= LayoutScrollViewerOnViewChanged;
            }
        }

        private void ListViewItemOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var listViewItem = sender as ListViewItem;
            var brushToSet = listViewItem == listViewItems[SelectedIndex] ? SelectedTabTextColorBrush : InactiveTabTextColorBrush;
            (listViewItem.ContentTemplateRoot as TextBlock).Foreground = brushToSet;
        }


        private void LayoutScrollViewerOnViewChanging(object sender,
            ScrollViewerViewChangingEventArgs scrollViewerViewChangingEventArgs)
        {
            if (!hasLayoutScrollViewerInitialized)
                return; // prevent access memory exception.

            var pageHorizontalOffset = layoutScrollViewer.HorizontalOffset - 2 + 0.02; // 2.0 is default value on first page, 0.2 constant cause event is not raised on-last-page-swiped.

            SetupIndicatorForOffset(pageHorizontalOffset);
        }

        private void SetupIndicatorForOffset(double horizontalOffset)
        {
            var pageIndex = (int)(horizontalOffset);
            var fractionalPart = horizontalOffset - pageIndex;

            AdjustTextColor(pageIndex, fractionalPart);
        }

        private void AdjustTextColor(int pageIndex, double fractionalPart)
        {
            var pageIndexToBeSelected = fractionalPart >= 0.5 ? pageIndex + 1 : pageIndex;

            if (pageIndexToBeSelected >= itemsCount)
                pageIndexToBeSelected = itemsCount - 1;

            if (pageIndexToBeSelected == selectedIndex)
                return;

            selectedIndex = pageIndexToBeSelected;
            var actualPageBrush = fractionalPart >= 0.5 ? InactiveTabTextColorBrush : SelectedTabTextColorBrush;
            var nextPageBrush = fractionalPart >= 0.5 ? SelectedTabTextColorBrush : InactiveTabTextColorBrush;

            SetTextColorBrushForTab(pageIndex, actualPageBrush);
            SetTextColorBrushForTab(pageIndex + 1, nextPageBrush);
        }

        private void SetTextColorBrushForTab(int tabIndex, Brush colorBrush)
        {
            if (tabIndex >= itemsCount)
                return;

            var listViewItem = listViewItems[tabIndex];
            var contentTemplateRoot = listViewItem.ContentTemplateRoot as TextBlock;

            if (contentTemplateRoot != null)
                contentTemplateRoot.Foreground = colorBrush;
        }
    }
}
