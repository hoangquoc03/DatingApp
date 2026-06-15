using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;
using DatingApp.Desktop.ViewModels;

namespace DatingApp.Desktop.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private bool _isDragging = false;
    private System.Windows.Point _clickPosition;

    public DashboardView()
    {
        InitializeComponent();
        DataContextChanged += DashboardView_DataContextChanged;
        Loaded += DashboardView_Loaded;
    }

    private void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        TriggerCardEntryAnimation();
    }

    private void DashboardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is DashboardViewModel vm)
        {
            vm.PropertyChanged += (s, ev) =>
            {
                if (ev.PropertyName == nameof(DashboardViewModel.CurrentUserImage))
                {
                    Application.Current.Dispatcher.Invoke(TriggerCardEntryAnimation);
                }
            };
        }
    }

    private void TriggerCardEntryAnimation()
    {
        if (SwipeCard == null) return;
        var cardScale = (SwipeCard.RenderTransform as TransformGroup)?.Children[0] as ScaleTransform;
        if (cardScale == null) return;

        CardTranslate.BeginAnimation(TranslateTransform.XProperty, null);
        CardTranslate.BeginAnimation(TranslateTransform.YProperty, null);
        CardRotate.BeginAnimation(RotateTransform.AngleProperty, null);
        cardScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        cardScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        var animScaleX = new DoubleAnimation(0.75, 1.0, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 4, EasingMode = EasingMode.EaseOut }
        };
        var animScaleY = new DoubleAnimation(0.75, 1.0, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 4, EasingMode = EasingMode.EaseOut }
        };
        var animOpacity = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(300));

        cardScale.CenterX = SwipeCard.ActualWidth > 0 ? SwipeCard.ActualWidth / 2 : 190;
        cardScale.CenterY = SwipeCard.ActualHeight > 0 ? SwipeCard.ActualHeight / 2 : 270;

        cardScale.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);
        cardScale.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);
        SwipeCard.BeginAnimation(UIElement.OpacityProperty, animOpacity);
    }

    private void SwipeCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
        CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
        CardRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);

        _isDragging = true;
        _clickPosition = e.GetPosition(this);
        SwipeCard.CaptureMouse();
    }

    private void SwipeCard_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDragging) return;

        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _clickPosition.X;
        var deltaY = currentPosition.Y - _clickPosition.Y;

        CardTranslate.X = deltaX;
        CardTranslate.Y = deltaY;

        CardRotate.CenterX = SwipeCard.ActualWidth / 2;
        CardRotate.CenterY = SwipeCard.ActualHeight + 50;

        CardRotate.Angle = deltaX / 10;
    }

    private void SwipeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        SwipeCard.ReleaseMouseCapture();

        var deltaX = CardTranslate.X;
        var deltaY = CardTranslate.Y;

        if (Math.Abs(deltaX) < 5 && Math.Abs(deltaY) < 5)
        {
            var vm = DataContext as DashboardViewModel;
            if (vm?.OpenUserDetailCommand.CanExecute(null) == true)
            {
                vm.OpenUserDetailCommand.Execute(null);
            }
            ResetCardPosition(false);
            return;
        }

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
            
            SwipeCard.BeginAnimation(UIElement.OpacityProperty, null);
            SwipeCard.Opacity = 1;
        };

        CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animX);
        SwipeCard.BeginAnimation(UIElement.OpacityProperty, animOpacity);
    }

    private void ResetCardPosition(bool animated = true)
    {
        if (animated)
        {
            var animX = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600)) { EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 4, EasingMode = EasingMode.EaseOut } };
            var animY = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600)) { EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 4, EasingMode = EasingMode.EaseOut } };
            var animRot = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600)) { EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 4, EasingMode = EasingMode.EaseOut } };
            
            animX.Completed += (s, e) => {
                CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
                CardTranslate.X = 0;
            };
            animY.Completed += (s, e) => {
                CardTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
                CardTranslate.Y = 0;
            };
            animRot.Completed += (s, e) => {
                CardRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);
                CardRotate.Angle = 0;
            };

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
