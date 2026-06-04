using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DatingApp.Desktop.ViewModels;

namespace DatingApp.Desktop.Views;

public partial class DashboardView : UserControl
{
    private bool _isDragging = false;
    private Point _clickPosition;

    public DashboardView()
    {
        InitializeComponent();
    }

    private void SwipeCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _clickPosition = e.GetPosition(this);
        SwipeCard.CaptureMouse();
    }

    private void SwipeCard_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _clickPosition.X;
        var deltaY = currentPosition.Y - _clickPosition.Y;

        CardTranslate.X = deltaX;
        CardTranslate.Y = deltaY;
        CardRotate.Angle = deltaX / 10; // Rotate slightly based on X
    }

    private void SwipeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        SwipeCard.ReleaseMouseCapture();

        var deltaX = CardTranslate.X;

        // Nếu vuốt quá trái/phải 120px thì lướt đi
        if (deltaX < -120)
        {
            AnimateOutAndExecute(-500, "PassCommand");
        }
        else if (deltaX > 120)
        {
            AnimateOutAndExecute(500, "LikeCommand");
        }
        else
        {
            // Trả về vị trí cũ
            ResetCardPosition();
        }
    }

    private void AnimateOutAndExecute(double toX, string commandName)
    {
        var animX = new DoubleAnimation(toX, TimeSpan.FromMilliseconds(200));
        var animOpacity = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));

        animX.Completed += (s, e) =>
        {
            var vm = DataContext as DashboardViewModel;
            if (commandName == "PassCommand") vm?.PassCommand.Execute(null);
            if (commandName == "LikeCommand") vm?.LikeCommand.Execute(null);

            ResetCardPosition(false);
            SwipeCard.Opacity = 1;
        };

        CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animX);
        SwipeCard.BeginAnimation(UIElement.OpacityProperty, animOpacity);
    }

    private void ResetCardPosition(bool animated = true)
    {
        if (animated)
        {
            var animX = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new BackEase { Amplitude = 0.5 } };
            var animY = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new BackEase { Amplitude = 0.5 } };
            var animRot = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animX);
            CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, animY);
            CardRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animRot);
        }
        else
        {
            CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
            CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
            CardRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);
            CardTranslate.X = 0;
            CardTranslate.Y = 0;
            CardRotate.Angle = 0;
        }
    }

}
