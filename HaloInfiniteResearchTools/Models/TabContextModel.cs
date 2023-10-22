using HaloInfiniteResearchTools.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HaloInfiniteResearchTools.Models
{
    public class TabContextModel : ObservableObject
    {

        #region Properties

        public ICollection<ITab> Tabs { get; }
        public ITab CurrentTab { get; set; }

        #endregion

        #region Constructor

        public TabContextModel()
        {
            Tabs = new ObservableCollection<ITab>();
        }

        #endregion

        #region Public Methods

        public void AddTab(ITab tab)
        {
            tab.CloseRequested += OnCloseTab;
            tab.CloseAllTabRequested += OnCloseAllTab;
            tab.CloseOthersTabRequested += OnCloseOthersTab;
            tab.CloseLeftTabRequested += OnCloseLeftTab;
            tab.CloseRightTabRequested += OnCloseRightTab;
            Tabs.Add(tab);
            CurrentTab = tab;
        }

        #endregion

        #region Event Handlers

        private void OnCloseTab(object sender, EventArgs e)
        {
            var tab = sender as ITab;
            if (tab is null)
                return;

            Tabs.Remove(tab);
            tab.CloseRequested -= OnCloseTab;
            tab.CloseAllTabRequested -= OnCloseAllTab;
            tab.CloseOthersTabRequested -= OnCloseOthersTab;
            tab.CloseLeftTabRequested -= OnCloseLeftTab;
            tab.CloseRightTabRequested -= OnCloseRightTab;
            tab.Dispose();
        }
         private void OnCloseAllTab(object sender, EventArgs e)
        {
            foreach (var item in Tabs)
            {
                item.CloseRequested -= OnCloseTab;
                item.CloseAllTabRequested -= OnCloseAllTab;
                item.CloseOthersTabRequested -= OnCloseOthersTab;
                item.CloseLeftTabRequested -= OnCloseLeftTab;
                item.CloseRightTabRequested -= OnCloseRightTab;
                item.Dispose();
            }
            Tabs.Clear();
            
        }
         private void OnCloseOthersTab(object sender, EventArgs e)
        {
            var tab = sender as ITab;
            if (tab is null)
                return;
            ITab[] itemToRemove =  new Tab[Tabs.Count-1];
            int i = 0;
            foreach (var item in Tabs)
            {
                if (item == tab)
                    continue; 
                item.CloseRequested -= OnCloseTab;
                item.CloseAllTabRequested -= OnCloseAllTab;
                item.CloseOthersTabRequested -= OnCloseOthersTab;
                item.CloseLeftTabRequested -= OnCloseLeftTab;
                item.CloseRightTabRequested -= OnCloseRightTab;
                item.Dispose();
                
                itemToRemove[i] = item;
                i++;
            }

            foreach (var item in itemToRemove)
            {
                Tabs.Remove(item);
            }
            
        } 
        private void OnCloseLeftTab(object sender, EventArgs e)
        {
            var tab = sender as ITab;
            if (tab is null)
                return;
            
            List<ITab> itemToRemove =  new List<ITab>();
            
            foreach (var item in Tabs)
            {
                if (item == tab)
                    break; 
                item.CloseRequested -= OnCloseTab;
                item.CloseAllTabRequested -= OnCloseAllTab;
                item.CloseOthersTabRequested -= OnCloseOthersTab;
                item.CloseLeftTabRequested -= OnCloseLeftTab;
                item.CloseRightTabRequested -= OnCloseRightTab;
                item.Dispose();
                
                itemToRemove.Add(item);
                
            }

            foreach (var item in itemToRemove)
            {
                Tabs.Remove(item);
            }
            
        } 
        private void OnCloseRightTab(object sender, EventArgs e)
        {
            var tab = sender as ITab;
            if (tab is null)
                return;
            List<ITab> itemToRemove = new List<ITab>();
            bool found= false;
            foreach (var item in Tabs)
            {
                if (item == tab)
                    found = true;
                else {
                    if (found) {
                        item.CloseRequested -= OnCloseTab;
                        item.CloseAllTabRequested -= OnCloseAllTab;
                        item.CloseOthersTabRequested -= OnCloseOthersTab;
                        item.CloseLeftTabRequested -= OnCloseLeftTab;
                        item.CloseRightTabRequested -= OnCloseRightTab;
                        item.Dispose();

                        itemToRemove.Add(item);
                    }
                    
                } 
                
                
            }

            foreach (var item in itemToRemove)
            {
                Tabs.Remove(item);
            }
            
        }

        #endregion

    }
}
