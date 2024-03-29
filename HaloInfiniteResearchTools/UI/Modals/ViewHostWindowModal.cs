﻿using System.Windows.Controls;
using HaloInfiniteResearchTools.Views;

namespace HaloInfiniteResearchTools.UI.Modals
{

  public class ViewHostWindowModal : WindowModal
  {

    #region Constructor

    public ViewHostWindowModal( IView view )
    {
      Content = view;
      Title = view.ViewName;

      if ( view is Control viewControl )
      {
        // TODO: We shouldn't have to auto-adjust this and/or have hard-coded padding
        const double WINDOW_HEADER_FOOTER_PADDING = 60;
        ModalMinHeight = viewControl.Height + WINDOW_HEADER_FOOTER_PADDING;
        ModalMinWidth = viewControl.Width;
      }
    }

    #endregion

  }

}
