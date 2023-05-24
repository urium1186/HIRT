using System;
using System.Collections.Concurrent;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.Controls
{

    public class SearchBox : TextBox
    {

        #region Data Members
        long lasttime = DateTime.MaxValue.Ticks;
        public static DependencyProperty HasTextProperty = DependencyProperty.Register(
          nameof(HasText),
          typeof(bool),
          typeof(SearchBox));

        public static DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
          nameof(PlaceholderText),
          typeof(string),
          typeof(SearchBox),
          new PropertyMetadata("Search"));

        public static DependencyProperty TextChangedCommandProperty = DependencyProperty.Register(
          nameof(TextChangedCommand),
          typeof(ICommand),
          typeof(SearchBox));

        ConcurrentStack<(ICommand, string)> _commands = new ConcurrentStack<(ICommand, string)>();

        #endregion

        #region Properties

        public bool HasText
        {
            get => (bool)GetValue(HasTextProperty);
            set => SetValue(HasTextProperty, value);
        }

        private Timer timer;

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public ICommand TextChangedCommand
        {
            get => (ICommand)GetValue(TextChangedCommandProperty);
            set => SetValue(TextChangedCommandProperty, value);
        }

        #endregion

        #region Constructor

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
              typeof(SearchBox),
              new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        #endregion

        #region Overrides

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            HasText = Text.Length != 0;

            if (timer == null)
            {
                timer = new Timer(500);

                timer.Elapsed += OnTimedEvent;

            }
            _commands.Push((TextChangedCommand, Text));
            if (!timer.Enabled)
                timer.Enabled = true;
            timer.Interval = 500;
            lasttime = DateTime.Now.Ticks;
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            long current_tick = DateTime.Now.Ticks;
            if (true)
            {

                try
                {
                    lock (_commands)
                    {
                        if (_commands.TryPop(out var temp))
                        {
                            timer.Stop();
                            _commands.Clear();
                            temp.Item1?.Execute(temp.Item2);

                        } //TextChangedCommand?
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }


            }
        }

        #endregion

    }

}
