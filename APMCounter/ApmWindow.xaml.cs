using APMCounter.Service;
using APMCounter.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
namespace APMCounter
{
    public partial class MainWindow : Window
    {
        private ApmViewModel apmViewModel;
        private ActionService actionService;
     
        public MainWindow()
        {
            InitializeComponent();
            apmViewModel = new ApmViewModel();
            actionService = ActionService.Start(apmViewModel.bucket);
            this.DataContext = apmViewModel;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        ~MainWindow()
        {
            ActionService.End(actionService._hookID);
        }
    }
}
