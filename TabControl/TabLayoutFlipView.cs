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
    public sealed class TabLayoutFlipView : FlipView
    {
        public static readonly DependencyProperty TabItemsSourceProperty = DependencyProperty.Register(
            "TabItemsSource", typeof(IEnumerable), typeof(TabLayoutFlipView),
            new PropertyMetadata(default(IEnumerable), (e, a) =>
            {
                var control = e as TabLayoutFlipView;

                if (control.tabsListView != null)
                {
                    control.tabsListView.ItemsSource = a.NewValue;
                }
            }));



        public DataTemplate TabTemplate
        {
            get { return (DataTemplate)GetValue(TabTemplateProperty); }
            set { SetValue(TabTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabTemplateProperty =
            DependencyProperty.Register("TabTemplate", typeof(DataTemplate), typeof(TabLayoutFlipView), new PropertyMetadata(default(DataTemplate)));



        public DataTemplateSelector TabTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(TabTemplateSelectorProperty); }
            set { SetValue(TabTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabTemplateSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabTemplateSelectorProperty =
            DependencyProperty.Register("TabTemplateSelector", typeof(DataTemplateSelector), typeof(TabLayoutFlipView), new PropertyMetadata(default(DataTemplateSelector)));

        private bool hasLayoutScrollViewerInitialized;
        private int itemsCount;
        private ScrollViewer layoutScrollViewer;
        private int selectedIndex = -1;
        private int previouslySelectedIndex = -1;
        private ListView tabsListView;

        public TabLayoutFlipView()
        {
            DefaultStyleKey = typeof(TabLayoutFlipView);
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
            tabsListView.ContainerContentChanging += TabsListView_ContainerContentChanging;

            if (TabTemplateSelector != null)
                tabsListView.ItemTemplateSelector = TabTemplateSelector;
            else
                tabsListView.ItemTemplate = TabTemplate;

            tabsListView.ItemClick += TabsListViewOnItemClick;
            tabsListView.IsItemClickEnabled = true;
            tabsListView.SelectionMode = ListViewSelectionMode.None;
            layoutScrollViewer.ViewChanging += LayoutScrollViewerOnViewChanging;
            layoutScrollViewer.ViewChanged += LayoutScrollViewerOnViewChanged;
        }

        private void TabsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            OnTabContentChanging();
        }

        private async void TabsListViewOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var container = tabsListView.ContainerFromItem(itemClickEventArgs.ClickedItem);
            if (container == null)
                return;

            var selectedIndex = tabsListView.IndexFromContainer(container);

            if (selectedIndex == -1)
                return;

            var isPageFarAway = Math.Abs(selectedIndex - SelectedIndex) > 1;

            if (!isPageFarAway)
            {
                SelectedIndex = selectedIndex;
                return;
            }

            while (SelectedIndex != selectedIndex)
            {
                SelectedIndex += (SelectedIndex > selectedIndex) ? -1 : 1;

                if (SelectedIndex != selectedIndex)
                    await Task.Delay(15);
            }
        }

        private void LayoutScrollViewerOnViewChanged(object sender,
            ScrollViewerViewChangedEventArgs scrollViewerViewChangedEventArgs)
        {
            if (!hasLayoutScrollViewerInitialized)
            {
                itemsCount = tabsListView.Items.Count;

                SetupIndicatorForOffset(SelectedIndex);
                hasLayoutScrollViewerInitialized = true;
                layoutScrollViewer.ViewChanged -= LayoutScrollViewerOnViewChanged;
                
            }
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

            NotifyAboutTabSelectionChanged(pageIndex, fractionalPart);
        }

        private void NotifyAboutTabSelectionChanged(int pageIndex, double fractionalPart)
        {
            var pageIndexToBeSelected = fractionalPart >= 0.5 ? pageIndex + 1 : pageIndex;

            if (pageIndexToBeSelected >= itemsCount)
                pageIndexToBeSelected = itemsCount - 1;

            if (pageIndexToBeSelected == selectedIndex)
                return;

            previouslySelectedIndex = selectedIndex;
            selectedIndex = pageIndexToBeSelected;
            var tabSelectionChangedEventArgs = new TabSelectionChangedEventArgs()
            {
                NewSelectionIndex = selectedIndex,
                NewSelectionListViewItem = tabsListView.ContainerFromIndex(selectedIndex) as ListViewItem
            };

            if (previouslySelectedIndex != -1)
            {
                tabSelectionChangedEventArgs.OldSelectionIndex = previouslySelectedIndex;
                tabSelectionChangedEventArgs.OldSelectionListViewItem =
                    tabsListView.ContainerFromIndex(previouslySelectedIndex) as ListViewItem;
            }

            OnTabSelectionIndexChanged(tabSelectionChangedEventArgs);
        }
         

        public event Action<TabSelectionChangedEventArgs> TabSelectionIndexChanged;
        public event Action TabContentChanging;

        private void OnTabSelectionIndexChanged(TabSelectionChangedEventArgs obj)
        {
            TabSelectionIndexChanged?.Invoke(obj);
        }

        public ListViewItem GetTabItem(int fromIndex)
        {
            return tabsListView.ContainerFromIndex(fromIndex) as ListViewItem;
        }

        private void OnTabContentChanging()
        {
            TabContentChanging?.Invoke();
        }
    }

    public class TabSelectionChangedEventArgs
    {
        public int? OldSelectionIndex { get; internal set; }
        public int NewSelectionIndex { get; internal set; }

        public ListViewItem OldSelectionListViewItem { get; internal set; }

        public ListViewItem NewSelectionListViewItem { get; internal set; }
    }
}
