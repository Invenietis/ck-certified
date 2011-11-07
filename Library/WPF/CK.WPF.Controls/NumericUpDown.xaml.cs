using System;
using System.Windows;
using System.Windows.Controls;

namespace CK.WPF.Controls
{

	public partial class NumericUpDown : UserControl
    {

        #region Properties
        
        /// <summary>Gets or sets the min value.</summary>
        public Int32 MinValue { get; set; }
        /// <summary>Gets or sets the max value.</summary>
        public Int32 MaxValue { get; set; }
        /// <summary>Gets or sets the increment step value.</summary>
        public Int32 Step { get; set; }

        /// <summary>Gets or sets the current value.</summary>
        public Int32 IntValue
        {
            get { return (Int32)GetValue(IntValueProperty); }
            set { SetValue(IntValueProperty, value); }
        }

        #endregion

        public NumericUpDown()
        {
            InitializeComponent();

            // Set default values
            MinValue = 0;
            MaxValue = 100;
            Step = 1;

            this.LayoutRoot.DataContext = this;
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty IntValueProperty = DependencyProperty.Register( "IntValue", typeof( Int32 ), typeof( NumericUpDown ), new FrameworkPropertyMetadata( (Int32)0, new PropertyChangedCallback( OnValueChanged ) ) );

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown nudCtrl = obj as NumericUpDown;
            nudCtrl.OnValueChanged(new RoutedPropertyChangedEventArgs<Int32>((Int32)args.OldValue, (Int32)args.NewValue, ValueChangedEvent));
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Int32>),
            typeof(NumericUpDown));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Decimal> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<Int32> args)
        {
            RaiseEvent(args);
        }

        #region Up & Down Buttons Actions

        private void btUpButton_Click(object sender, EventArgs e)
        {
            if( IntValue + Step <= MaxValue ) IntValue += Step;
            else IntValue = MaxValue;
        }

        private void btDownButton_Click(object sender, EventArgs e)
        {
            if( IntValue - Step >= MinValue ) IntValue -= Step;
            else IntValue = MinValue;
        }

        #endregion
    
    }
}