using HaloInfiniteResearchTools.Common;
using PropertyChanged;
using System;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.ViewModels
{

    public class ProgressViewModel : ViewModel, IProgressData
    {

        #region Properties

        public string Status { get; set; }

        public string UnitName { get; set; }
        public bool CanCancel { get; set; }

        public int CompletedUnits { get; set; }

        public int TotalUnits { get; set; }

        public ICommand CancelCommand { get; }

        public bool IsIndeterminate { get; set; }

        [DependsOn(nameof(CompletedUnits))]
        public string SubStatus
        {
            get
            {
                if (IsIndeterminate) return null;
                return $"{CompletedUnits} of {TotalUnits} {UnitName} ({PercentageComplete:0.00%})";
            }
        }

        [DependsOn(nameof(CompletedUnits))]
        public double PercentageComplete
        {
            get => (double)CompletedUnits / Math.Max(1, TotalUnits);
        }

        #endregion

        #region Constructor

        public ProgressViewModel(IServiceProvider serviceProvider)
          : base(serviceProvider)
        {
        }

        #endregion

    }

}
