using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MeBrowser.Helpers
{
    public static class SendKey
    {
        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null && Keyboard.PrimaryDevice.ActiveSource != null)
            {
                KeyEventArgs keyEventArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                };
                InputManager.Current.ProcessInput(keyEventArgs);
            }
        }
    }
}
