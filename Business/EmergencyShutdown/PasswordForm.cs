using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmergencyShutdown
{
    public partial class PasswordForm : Form
    {
        private const string PrivateKeyEncrypted =
            "UFOVMEdwtlQlcG2yUEvNy7NQ2Fk4V6rQCuV2XqnC+AQzTn0Mx4z2ksa8v5+SK7aWgOg54/WiD+yMfu7NR6Tdv6fR71C3RoF6oDMfGMLytL/1dqrFSmxm8hsMxJ0AKKALL0TD4OmAPYftZT/k5L8kGm+NESLmujeCeLaKuAxzEU4BMJ31Xt0SfYxTfI+E2GOLnyUD64vZji0m4CoxYfn07fkU0JGr6LAugrOf1VqGUvr+1Pob9re8qlotoy8WhZGjdVahh0400/uV1hPf9HSs89Wpr00lfQX7fkCpnqhvKz7jeXZ6HJ4vdyfK5b0weuFsnpDKE6bRfEKFC4UpwKQTiIY0mMT949Lpzai5HiF9tTIoRXpSObfnCR7vFfMPW3rTa9LzPU6KCijBFLj+4d+D0toYPvDoAvakpZfR/tYIiQsTlPGqOrAX0KJUgLmSrtSr+f8W59WfuK2NpfuIweGU1Vc6rRuxs4Yp17Ko/P0qKAabZr0Dip5BI1/ElKVfNIgWq9wztbhS9rydIoFXJhE0ezwp4B3ue3yIbe9y8Rmn1SEmREs/jO2Jf1gs0UswKm4YpvKjM+T8ge4aGXU6OtEsaPSCZNoMWHsUfDQxBH5ZMtW+wkQ3F/J9H3RKvLDVH6v5+np3rpIMkd73ajH44GLOAsR2XbSGbmxAPEf4xotI5CpKFBYxTx+6nf88XvwccFetzg6hqA8/eG3K7FefnGxtjXNgk7wLkRHnUOHM6RtS1hmaPu5wdR2OuhRPS0/NbX45gdPwz3JuK+svNhPzjIqwdEYntTERspcoNrnbtv5G8v9y1i4fF8mjzuCRCWnSz393ilBZQUsD8SDYZ+XvefcW7qHWFk92tD+Keqm8S7A3fLxWnU4LrFGgM/sRnSH+UESzec5NiYhgI+rH67K0Hecryp0xRM0ZtOvSyif1u9Gqucp3tA/Ab4SxUbux2ErwWAaT/+gZR/ZvEx58rEK98QMjFTgF9MhCdUoziqwNwALwJlRYQxLl00ilWZZ7kLPgo4ZLkx4xbIs7jn05dfkLBNrKvNCfnmluDVLyC2kpIWP0NA/TuqI01Eb5g4cjKxK8NBXPY5oJ0AsmXlPsGeWko7VMwvpWSfdECQxSkiYR1R6VEkmC2Lhy5yVujrMdMjVI7HjnRoreeZlpmrvbt/6nq7UKwZ1RINroQTAu7No6m2FtC6nojwTnUfzhi6txc4ofNpy6q8JfXlOo1q/JaOrkzj4Cg9hC9i9fF8bBYIVRcUs0niEqGxnkYAgBYIwhmBRK8YgpxStpiGj68upXLS84aDMnnIgXT+GwX0152pi1IIshfAk=";

        public PasswordForm()
        {
            InitializeComponent();
        }

        public string PrivateKey { get; private set; }

        private void PasswordForm_Load(object sender, EventArgs e)
        {
            passwordBox.Focus();
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                try
                {
                    PrivateKey = StringCipher.Decrypt(PrivateKeyEncrypted, passwordBox.Text);
                    DialogResult = DialogResult.OK;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
    }
}
