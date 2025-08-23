using System.Diagnostics;

namespace WinInteropUtils_Test_App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowExceptionDialog(e.Exception);

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void ShowExceptionDialog(Exception ex)
        {
            var page = new TaskDialogPage
            {
                Heading = "Exception",
                Caption = "Exception",
                Text = "An exception was thrown: " + ex.GetType().FullName ?? ex.GetType().Name,
                Expander = new TaskDialogExpander
                {
                    Text = $"{ex.Message}\n\nStack trace:\n\n{ex.StackTrace}",
                    CollapsedButtonText = "View error info",
                    ExpandedButtonText = "Hide error info"
                },
                Icon = TaskDialogIcon.ShieldErrorRedBar,
                Buttons = [TaskDialogButton.OK]
            };

            if (ex.Source != null)
            {
                page.Expander.Text += "\n\n\nSource: " + ex.Source;
            }

            if (ex.InnerException != null)
            {
                var viewInnerBtn = new TaskDialogButton("View inner exception");
                viewInnerBtn.Click += (s, e) =>
                {
                    ShowExceptionDialog(ex.InnerException);
                };

                page.Buttons.Add(viewInnerBtn);
            }

            TaskDialog.ShowDialog(page, TaskDialogStartupLocation.CenterScreen);
        }
    }
}