using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ydis.Model.DataStructures;
using Ydis.Properties;
using Ydis.ViewModels.CommonControlsViewModels;
using Ydis.ViewModels.DataStructures;
using Ydis.ViewModels.SessionSummary;
using Ydis.Views.Commands;

namespace Ydis.ViewModels.SelectedLevel.SessionsTab.SessionSummary
{
    /// <summary>
    /// View model for the detailled summary of one session
    /// </summary>
    public class SessionViewModel : BaseViewModel, IReplaceableViewViewModel
    {
        /// <summary>
        /// Header of the session summary.
        /// </summary>
        public SessionHeaderViewModel Header { get; }
        /// <summary>
        /// Graph of the statistics of the session.
        /// </summary>
        public LevelGraphViewModel Graph { get; }
        /// <summary>
        /// Currently dsplayed attempts summary. Can be a summary per percent or a list of attempts ordered by number.
        /// </summary>
        public BaseViewModel CurrentView { get; set; }
        /// <summary>
        /// Text of the button that switches the attempt display mode
        /// </summary>
        public string CurrentButtonText => ShowingDetails ? Resources.SessionViewShowStats : Resources.SessionViewShowAttempts;
        /// <summary>
        /// Command to cange the way attempts are displayed.
        /// </summary>
        public NavigatorCommand SwitchCommand { get; set; }
        /// <summary>
        /// Command to close the session summary.
        /// </summary>
        public GoBackCommand GoBack { get; }
        // Session summarized
        private ISession Session { get; }
        // Model for displaying the attempts grouped by %
        private LevelDataGridViewModel Datagrid { get; }
        // Model for displaying the attempts in a simple list
        private BaseViewModel AttemptList { get; }
        // Command to switch the way attempts are displayed to a summary.
        private NavigatorCommand AttemptsSummaryCommand { get; }
        // Command to switch the way attempts are displayed to a list.
        private NavigatorCommand AttemptsDetailsCommand { get; }
        // Wether attempts are displayed are a list or not.
        private bool ShowingDetails { get; set; }

        public SessionViewModel(SessionsTabMainViewModel parent, ISession s)
        {
            Session = s;
            GoBack = new GoBackCommand(parent);
            Header = new SessionHeaderViewModel(Session);
            var data = new SessionGroupData(Session);
            var stats = new SessionsStatistics(data, 1f);
            Graph = new LevelGraphViewModel(stats, Resources.GraphTitleIndividualSession);
            Datagrid = new LevelDataGridViewModel(stats);
            if(s is Session ns)
            {
                AttemptList = new AttemptListViewModel(ns.Attempts);
            } else if(s is PracticeSession ps)
            {
                AttemptList = new PracticeAttemptListViewModel(ps.Attempts);
            }
            AttemptsSummaryCommand = new NavigatorCommand(this, Datagrid);
            AttemptsDetailsCommand = new NavigatorCommand(this, AttemptList);

            ShowingDetails = false;
            CurrentView = Datagrid;
            SwitchCommand = AttemptsDetailsCommand;
        }

        /// <summary>
        /// Used to change how attempts are displayed.
        /// </summary>
        /// <param name="m">The model of the view part that will dsplay attempts.</param>
        public void ReplaceView(BaseViewModel m)
        {
            CurrentView = m;
            SwitchCommand = ShowingDetails ? AttemptsDetailsCommand : AttemptsSummaryCommand;
            ShowingDetails = !ShowingDetails;
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(SwitchCommand));
            OnPropertyChanged(nameof(CurrentButtonText));
        }
    }
}
