using System.Windows;

namespace LectureMate.App;

/// <summary>
/// Main application window: live transcript, session controls and status bar.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Creates the window and loads its XAML.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }
}
