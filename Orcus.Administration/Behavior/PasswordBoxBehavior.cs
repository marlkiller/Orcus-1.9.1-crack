using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Orcus.Administration.Behavior
{
    public class PasswordBoxBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty SecurePasswordProperty = DependencyProperty.Register(
            "SecurePassword", typeof (SecureString), typeof (PasswordBoxBehavior),
            new FrameworkPropertyMetadata(default(SecureString)) {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty GetPasswordFunctionProperty = DependencyProperty.Register(
            "GetPasswordFunction", typeof (Func<string>), typeof (PasswordBoxBehavior),
            new FrameworkPropertyMetadata(default(Func<string>)) {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty SetPasswordActionProperty = DependencyProperty.Register(
            "SetPasswordAction", typeof (Action<string>), typeof (PasswordBoxBehavior),
            new FrameworkPropertyMetadata(default(Action<string>)) {BindsTwoWayByDefault = true});

        public SecureString SecurePassword
        {
            get { return (SecureString) GetValue(SecurePasswordProperty); }
            set { SetValue(SecurePasswordProperty, value); }
        }

        public Action<string> SetPasswordAction
        {
            get { return (Action<string>) GetValue(SetPasswordActionProperty); }
            set { SetValue(SetPasswordActionProperty, value); }
        }

        public Func<string> GetPasswordFunction
        {
            get { return (Func<string>) GetValue(GetPasswordFunctionProperty); }
            set { SetValue(GetPasswordFunctionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            GetPasswordFunction = GetPassword;
            SetPasswordAction = SetPassword;
            AssociatedObject.PasswordChanged += AssociatedObjectOnPasswordChanged;
        }

        private void SetPassword(string s)
        {
            AssociatedObject.Password = s;
        }

        private string GetPassword()
        {
            return AssociatedObject.Password;
        }

        private void AssociatedObjectOnPasswordChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            SecurePassword = AssociatedObject.SecurePassword;
        }
    }
}